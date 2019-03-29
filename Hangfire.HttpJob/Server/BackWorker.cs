using System;
using System.Collections.Generic;
using System.Text;

namespace Hangfire.HttpJob.Server
{
   public class BackWorker
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string UrL { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 方法类型
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// 执行频率/秒
        /// </summary>
        public int Internal { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// 请求参数类型
        /// </summary>
        public string ContentType { get; set; }
    }
}
