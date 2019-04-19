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
using CommonUtils;
using NLog;
using System.Collections.Generic;

namespace Hangfire.HttpJob.Server
{
    public class HttpJob
    {
        /// <summary>
        /// 是否使用apollo配置中心
        /// </summary>
        private static readonly bool UseApollo = ConfigSettings.Instance.UseApollo;
        private static readonly Logger logger = new LogFactory().GetCurrentClassLogger();
        public static HangfireHttpJobOptions HangfireHttpJobOptions;
        private static MimeMessage mimeMessage;
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="jobname">任务名称</param>
        /// <param name="Url">地址</param>
        /// <param name="exception">异常信息</param>
        /// <returns></returns>
        public static bool SendEmail(string jobname, string Url, string exception)
        {
            try
            {
                mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(UseApollo ? ConfigSettings.Instance.SendMailAddress : HangfireHttpJobOptions.SendMailAddress));
                List<Emails> SendMailList = new List<Emails>();
                if (UseApollo)
                {
                    SendMailList = JsonConvert.DeserializeObject<List<Emails>>(ConfigSettings.Instance.SendMailList);
                    SendMailList.ForEach(k =>
                    {
                        mimeMessage.To.Add(new MailboxAddress(k.Email));
                    });
                }
                else
                {
                    HangfireHttpJobOptions.SendToMailList.ForEach(k =>
                    {
                        mimeMessage.To.Add(new MailboxAddress(k));
                    });
                }
                mimeMessage.Subject = UseApollo ? ConfigSettings.Instance.SMTPSubject : HangfireHttpJobOptions.SMTPSubject;
                var builder = new BodyBuilder
                {
                    //builder.TextBody = $"执行出错,任务名称【{item.JobName}】,错误详情：{ex}";
                    HtmlBody = SethtmlBody(jobname, Url, $"执行出错，错误详情:{exception}")
                };
                mimeMessage.Body = builder.ToMessageBody();
                var client = new SmtpClient();
                client.Connect(UseApollo ? ConfigSettings.Instance.SMTPServerAddress : HangfireHttpJobOptions.SMTPServerAddress,
                    UseApollo ? ConfigSettings.Instance.SMTPPort : HangfireHttpJobOptions.SMTPPort, true);     //连接服务
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(UseApollo ? ConfigSettings.Instance.SendMailAddress : HangfireHttpJobOptions.SendMailAddress,
                   UseApollo ? ConfigSettings.Instance.SMTPPwd : HangfireHttpJobOptions.SMTPPwd); //验证账号密码
                client.Send(mimeMessage);
                client.Disconnect(true);
            }
            catch (Exception ee)
            {
                logger.Info($"邮件服务异常，异常为：{ee}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 设置httpclient
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static HttpClient GetHttpClient(HttpJobItem item)
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
            //设置超时时间
            var HttpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(item.Timeout == 0 ? HangfireHttpJobOptions.GlobalHttpTimeOut : item.Timeout),
            };

            if (!string.IsNullOrEmpty(item.BasicUserName) && !string.IsNullOrEmpty(item.BasicPassword))
            {
                var byteArray = Encoding.ASCII.GetBytes(item.BasicUserName + ":" + item.BasicPassword);
                HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            }
            return HttpClient;
        }
        /// <summary>
        /// signalR推送使用
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="statuscode"></param>
        /// <returns></returns>
        public static string SendRequest(string url, string data)
        {
            var item = new HttpJobItem()
            {
                Data = "{" + $"\"message\":\"{data}\"" + "}",
                Url = url,
                Method = "post",
                ContentType = "application/json"
            };
            var client = GetHttpClient(item);
            var httpMesage = PrepareHttpRequestMessage(item);
            _ = client.SendAsync(httpMesage).GetAwaiter().GetResult();
            var httpcontent = new StringContent(item.Data.ToString());
            client.DefaultRequestHeaders.Add("Method", "Post");
            httpcontent.Headers.ContentType = new MediaTypeHeaderValue(item.ContentType);
            client.DefaultRequestHeaders.Add("UserAgent",
      "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36");
            client.DefaultRequestHeaders.Add("KeepAlive", "false");
            _ = client.PostAsync(item.Url, httpcontent).GetAwaiter().GetResult();
            string result = httpcontent.ReadAsStringAsync().GetAwaiter().GetResult();
            return result;
        }
        /// <summary>
        /// 邮件模板
        /// </summary>
        /// <param name="jobname"></param>
        /// <param name="url"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static string SethtmlBody(string jobname, string url, string exception)
        {
            var title = UseApollo ? ConfigSettings.Instance.SMTPSubject : HangfireHttpJobOptions.SMTPSubject;
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
        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 }, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        [DisplayName("任务名称:{1}")]
        [JobFilter(timeoutInSeconds: 3600)]
        public static void Excute(HttpJobItem item, string jobName = null, PerformContext context = null)
        {
            try
            {
                //此处信息会显示在执行结果日志中
                context.SetTextColor(ConsoleTextColor.Yellow);
                context.WriteLine($"任务开始执行,执行时间{DateTime.Now.ToString()}");
                context.WriteLine($"任务名称:{jobName}");
                context.WriteLine($"参数:{JsonConvert.SerializeObject(item)}");
                var client = GetHttpClient(item);
                var httpMesage = PrepareHttpRequestMessage(item);
                var httpResponse = new HttpResponseMessage();
                httpResponse = SendUrlRequest(item, client, httpMesage);
                HttpContent content = httpResponse.Content;
                string result = content.ReadAsStringAsync().GetAwaiter().GetResult();
                context.WriteLine($"执行结果：{result}");
            }
            catch (Exception ex)
            {
                //获取重试次数
                var count = context.GetJobParameter<string>("RetryCount");
                context.SetTextColor(ConsoleTextColor.Red);
                //signalR推送
                //SendRequest(UseApollo?ConfigSettings.Instance.ServiceAddress:ConfigSettings.Instance.URL+"/api/Publish/EveryOne", "测试");
                if (count == "3"&&ConfigSettings.Instance.UseEmail)//重试达到三次的时候发邮件通知
                {
                    SendEmail(item.JobName, item.Url, ex.ToString());
                }
                logger.Error(ex, "HttpJob.Excute");
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
        private static HttpResponseMessage SendUrlRequest(HttpJobItem item, HttpClient client, HttpRequestMessage httpMesage)
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

                httpResponse = client.PostAsync(item.Url, httpcontent).GetAwaiter().GetResult();
            }
            else
            {
                httpResponse = client.SendAsync(httpMesage).GetAwaiter().GetResult();
            }

            return httpResponse;
        }
    }



}
