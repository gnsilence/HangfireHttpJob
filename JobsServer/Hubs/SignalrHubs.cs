using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobsServer.Hubs
{
    public class SignalrHubs:Hub
    {
       /// <summary>
       ///用户加入组处理
       /// </summary>
       /// <param name="userid">用户唯一标识</param>
       /// <param name="GroupName">组名称</param>
       /// <returns></returns>
        public Task InitUsers(string userid,string GroupName)
        {
            Console.WriteLine($"{userid}加入用户组");
            Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
            SignalrGroups.UserGroups.Add(new SignalrGroups()
            {
                ConnectionId = Context.ConnectionId,
                GroupName = GroupName,
                UserId = userid
            });
            return Clients.All.SendAsync("UserJoin", "用户组数据更新,新增id为：" + Context.ConnectionId + " pid:" + userid);
        }
        /// <summary>
        /// 断线的时候处理
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            //掉线移除用户，不给其推送
            var user = SignalrGroups.UserGroups.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

            if (user != null)
            {
                Console.WriteLine($"用户:{user.UserId}已离线");
                SignalrGroups.UserGroups.Remove(user);
                Groups.RemoveFromGroupAsync(Context.ConnectionId, user.GroupName);
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
