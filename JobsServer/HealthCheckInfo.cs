using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobsServer
{
    public class HealthCheckInfo
    {
        /// <summary>
        /// 接口url地址
        /// </summary>
        public string Uri { get; set; }
        /// <summary>
        /// http方法
        /// </summary>
        public string httpMethod { get; set; }
    }
}
