using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobsServer.Hubs
{
    [Serializable]
    public class SignalrGroups
    {
        /// <summary>
        /// 用户组
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 链接id
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// 用户组内存集合
        /// </summary>
        public static List<SignalrGroups> UserGroups = new List<SignalrGroups> { };
    }
}
