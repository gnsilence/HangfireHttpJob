using Hangfire.Console;
using Hangfire.Logging;
using Hangfire.Server;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using MailKit;
using MimeKit;
using MailKit.Net.Smtp;
using Hangfire.HttpJob.Support;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Hangfire.HttpJob.Server
{
    public class HttpJob
    {
        private readonly ILog _logger = LogProvider.For<HttpJob>();
        public static HangfireHttpJobOptions HangfireHttpJobOptions;
        private static MimeMessage mimeMessage;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpJob(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="jobname">任务名称</param>
        /// <param name="Url">地址</param>
        /// <param name="exception">异常信息</param>
        /// <returns></returns>
        public async Task<bool> SendEmail(string jobname, string Url, string exception)
        {
            try
            {
                mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress("mail", HangfireHttpJobOptions.SendMailAddress));
                var SendMailList = HangfireHttpJobOptions.SendToMailList;
                HangfireHttpJobOptions.SendToMailList.ForEach(k =>
                {
                    mimeMessage.To.Add(new MailboxAddress("mail", k));
                });
                mimeMessage.Subject = HangfireHttpJobOptions.SMTPSubject;
                var builder = new BodyBuilder
                {
                    //builder.TextBody = $"执行出错,任务名称【{item.JobName}】,错误详情：{ex}";
                    HtmlBody = SethtmlBody(jobname, Url, $"执行出错，错误详情:{exception}")
                };
                mimeMessage.Body = builder.ToMessageBody();
                var client = new SmtpClient();
                await client.ConnectAsync(HangfireHttpJobOptions.SMTPServerAddress, HangfireHttpJobOptions.SMTPPort, true);     //连接服务
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(HangfireHttpJobOptions.SendMailAddress,
                     HangfireHttpJobOptions.SMTPPwd); //验证账号密码
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }
            catch (Exception ee)
            {
                _logger.Info($"邮件服务异常，异常为：{ee}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 设置httpclient
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static HttpClient GetHttpClient(HttpJobItem item, HttpClient httpClient)
        {
            var handler = new HttpClientHandler();
            if (HangfireHttpJobOptions.Proxy == null)
            {
                handler.UseProxy = false;
            }
            else
            {
                handler.Proxy = HangfireHttpJobOptions.Proxy;
            }
            httpClient.Timeout = TimeSpan.FromSeconds(item.Timeout == 0 ? HangfireHttpJobOptions.GlobalHttpTimeOut : item.Timeout);
            ////设置超时时间
            //var HttpClient = new HttpClient(handler)
            //{
            //    Timeout = TimeSpan.FromSeconds(item.Timeout == 0 ? HangfireHttpJobOptions.GlobalHttpTimeOut : item.Timeout),
            //};

            if (!string.IsNullOrEmpty(item.BasicUserName) && !string.IsNullOrEmpty(item.BasicPassword))
            {
                var byteArray = Encoding.ASCII.GetBytes(item.BasicUserName + ":" + item.BasicPassword);
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
            return httpClient;
        }

        //  /// <summary>
        //  /// signalR推送使用
        //  /// </summary>
        //  /// <param name="url"></param>
        //  /// <param name="data"></param>
        //  /// <param name="statuscode"></param>
        //  /// <returns></returns>
        //  public static string SendRequest(string url, string data)
        //  {
        //      var item = new HttpJobItem()
        //      {
        //          Data = "{" + $"\"message\":\"{data}\"" + "}",
        //          Url = url,
        //          Method = "post",
        //          ContentType = "application/json"
        //      };
        //      var client = GetHttpClient(item);
        //      var httpMesage = PrepareHttpRequestMessage(item);
        //      _ = client.SendAsync(httpMesage).GetAwaiter().GetResult();
        //      var httpcontent = new StringContent(item.Data.ToString());
        //      client.DefaultRequestHeaders.Add("Method", "Post");
        //      httpcontent.Headers.ContentType = new MediaTypeHeaderValue(item.ContentType);
        //      client.DefaultRequestHeaders.Add("UserAgent",
        //"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36");
        //      client.DefaultRequestHeaders.Add("KeepAlive", "false");
        //      _ = client.PostAsync(item.Url, httpcontent).GetAwaiter().GetResult();
        //      string result = httpcontent.ReadAsStringAsync().GetAwaiter().GetResult();
        //      return result;
        //  }

        /// <summary>
        /// 邮件模板
        /// </summary>
        /// <param name="jobname"></param>
        /// <param name="url"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static string SethtmlBody(string jobname, string url, string exception)
        {
            var title = HangfireHttpJobOptions.SMTPSubject;
            var htmlbody = $@"<h3 align='center'>{title}</h3>
                            <h3>执行时间：</h3>
                            <p>
                                {DateTime.Now}
                            </p>
                            <h3>
                                任务名称：<span> {jobname} </span><br/>
                            </h3>
                            <h3>
                                请求路径：{url}
                            </h3>
                            <h3><span></span>
                                执行结果：<br/>
                            </h3>
                            <p>
                                {exception}
                            </p> ";
            return htmlbody;
        }

        public static HttpRequestMessage PrepareHttpRequestMessage(HttpJobItem item)
        {
            var request = new HttpRequestMessage(new HttpMethod(item.Method), item.Url);
            if (item.Headers.Any())
            {
                item.Headers.ForEach(h =>
                {
                    request.Headers.Add(h.Key, h.Value);
                });
            }
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(item.ContentType));
            if (!item.Method.ToLower().Equals("get"))
            {
                if (!string.IsNullOrEmpty(item.Data.ToString()))
                {
                    var bytes = Encoding.UTF8.GetBytes(item.Data.ToString());
                    request.Content = new ByteArrayContent(bytes, 0, bytes.Length);
                }
            }
            return request;
        }

        /// <summary>
        /// 执行任务，DelaysInSeconds(重试时间间隔/单位秒)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="jobName"></param>
        /// <param name="context"></param>
        [AutomaticRetrySet(LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisplayName("Args : [  JobName : {1}  |  Queue : {2}  |  IsRetry : {3} ]")]
        [JobFilter]
        public async Task ExcuteAsync(HttpJobItem item, string jobName = null, string queuename = null, bool isretry = false, PerformContext context = null)
        {
            try
            {
                //此处信息会显示在执行结果日志中
                context.SetTextColor(ConsoleTextColor.Yellow);
                context.WriteLine($"任务开始执行,执行时间{DateTime.Now.ToString()}");
                context.WriteLine($"任务名称:{jobName}");
                context.WriteLine($"参数:{JsonConvert.SerializeObject(item)}");
                var client = _httpClientFactory.CreateClient("httpjob");
                client = GetHttpClient(item, client);
                var httpMesage = PrepareHttpRequestMessage(item);
                var httpResponse = new HttpResponseMessage();
                httpResponse = await SendUrlRequest(item, client, httpMesage);
                HttpContent content = httpResponse.Content;
                string result = await content.ReadAsStringAsync();
                context.WriteLine($"执行结果：{result}");
            }
            catch (Exception ex)
            {
                //获取重试次数
                var count = context.GetJobParameter<string>("RetryCount");
                context.SetTextColor(ConsoleTextColor.Red);
                if (count == HangfireHttpJobOptions.AttemptsCountArray.Count().ToString() && HangfireHttpJobOptions.UseEmail)//重试达到三次的时候发邮件通知
                {
                    await SendEmail(item.JobName, item.Url, ex.ToString());
                }
                _logger.Error($"任务执行出错: \n 错误消息：{ex.Message} \n 异常详情：{ex.StackTrace}");
                context.WriteLine($"执行出错：{ex.Message}");
                throw;//不抛异常不会执行重试操作
            }
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="item"></param>
        /// <param name="client"></param>
        /// <param name="httpMesage"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> SendUrlRequest(HttpJobItem item, HttpClient client, HttpRequestMessage httpMesage)
        {
            HttpResponseMessage httpResponse;
            if (item.Method.ToLower() == "post")
            {
                var httpcontent = new StringContent(item.Data.ToString());
                client.DefaultRequestHeaders.Add("Method", "Post");
                httpcontent.Headers.ContentType = new MediaTypeHeaderValue(item.ContentType);
                client.DefaultRequestHeaders.Add("UserAgent",
          "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36");
                client.DefaultRequestHeaders.Add("KeepAlive", "false");

                httpResponse = await client.PostAsync(item.Url, httpcontent);
            }
            else
            {
                httpResponse = await client.SendAsync(httpMesage);
            }

            return httpResponse;
        }
    }
}