using Newtonsoft.Json;

namespace Hangfire.HttpApiJob.Server
{
    public class HttpJobItem
    {
        public HttpJobItem()
        {
            Method = "Post";
            ContentType = "application/json";
            Timeout = 20000;
            DelayFromMinutes = 15;
        }
        /// <summary>
        /// 请求Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 请求类型
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; set; }
        /// <summary>
        /// 延迟多长时间开始执行
        /// </summary>
        public int DelayFromMinutes { get; set; }
        /// <summary>
        /// Corn表达式
        /// </summary>
        public string Corn { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// 账号
        /// </summary>
        public string BasicUserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string BasicPassword { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    /// <summary>
    /// 周期性任务
    /// </summary>
    public class RecurringJobItem
    {
        public string Url { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public string Data { get; set; }

        public string ContentType { get; set; }

        public int Timeout { get; set; }
        public string Corn { get; set; }
        public string JobName { get; set; }
        public string BasicUserName { get; set; }
        public string BasicPassword { get; set; }
    }
}
