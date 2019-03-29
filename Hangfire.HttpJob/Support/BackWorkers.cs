using Hangfire.Annotations;
using Hangfire.HttpJob.Server;
using Hangfire.Server;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Console;

namespace Hangfire.HttpJob.Support
{
    public class BackWorkers : IBackgroundProcess
    {
        /// <summary>
        /// 执行频率/秒
        /// </summary>
        public static BackWorker _backWorker;
        public BackWorkers(BackWorker backWorker)
        {
            _backWorker = backWorker;
        }
        public void Execute([NotNull] BackgroundProcessContext context)
        {

            using (var connetion = context.Storage.GetConnection())
            {
                //申请分布式锁
                using (connetion.AcquireDistributedLock($"{_backWorker.JobName}:secondsJob", TimeSpan.FromSeconds(_backWorker.Internal)))
                {
                    BackgroundJob.Enqueue<DoTest>(job => job.SendRequest(_backWorker.JobName, _backWorker, null));
                }
            }
            context.Wait(TimeSpan.FromSeconds(_backWorker.Internal));
        }

    }

    [Queue("api")]
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public class DoTest
    {
        private static readonly Logger logger = new LogFactory().GetCurrentClassLogger();
        [DisplayName("BackGroundJob:{0}")]
        public void SendRequest(string jobname, BackWorker backWorker, [CanBeNull]PerformContext pcontext)
        {
            //将日志输出到控制台展示
            pcontext.WriteLine($"任务名称:{jobname},执行时间{DateTime.Now}");
            try
            {
                var reslut = SendUrlRequest(backWorker);
                System.Console.WriteLine($"{reslut}");
                pcontext.WriteLine($"执行结果：{reslut}");
            }
            catch (Exception ec)
            {
                pcontext.WriteLine($"执行任务失败,出现异常：{ec}");
            }
        }
        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="backWorker"></param>
        /// <returns></returns>
        private string SendUrlRequest(BackWorker backWorker)
        {
            string statusCode = string.Empty;
            if (backWorker.Method.ToLower() == "post")
            {
                HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(backWorker.Data));
                httpContent.Headers.ContentType = new MediaTypeHeaderValue(backWorker.ContentType);
                httpContent.Headers.ContentType.CharSet = "utf-8";
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Method", "Post");
                httpClient.DefaultRequestHeaders.Add("UserAgent",
                  "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("KeepAlive", "false");
                HttpResponseMessage response = httpClient.PostAsync(backWorker.UrL, httpContent).Result;
                statusCode = response.StatusCode.ToString();
                while (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    return result;
                }
            }
            else
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue(backWorker.ContentType));
                HttpResponseMessage response = httpClient.GetAsync(backWorker.UrL).Result;
                string result = default(string);
                statusCode = response.StatusCode.ToString();
                if (response.IsSuccessStatusCode)
                {
                    Task<string> t = response.Content.ReadAsStringAsync();
                    result = t.Result;
                    return result;
                }
            }
            return statusCode;
        }
    }
}
