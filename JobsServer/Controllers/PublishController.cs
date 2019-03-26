using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JobsServer.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace JobsServer.Controllers
{
    [Produces("application/json")]
    [Route("api/Publish")]
    public class PublishController : Controller
    {
        private IHubContext<SignalrHubs> hubContext;
        public PublishController(IServiceProvider service)
        {
            hubContext = service.GetService<IHubContext<SignalrHubs>>();
        }
        /// <summary>
        /// 单个connectionid推送
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        [HttpPost, Route("AnyOne")]
        public IActionResult AnyOne([FromBody]IEnumerable<SignalrGroups> groups)
        {
            if (groups != null && groups.Any())
            {
                var ids = groups.Select(c => c.UserId);
                var list = SignalrGroups.UserGroups.Where(c => ids.Contains(c.UserId));
                foreach (var item in list)
                    hubContext.Clients.Client(item.ConnectionId).SendAsync("AnyOne", $"{item.ConnectionId}: {item.Content}");
            }
            return Ok();
        }

        /// <summary>
        /// 全部推送
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost, Route("EveryOne")]
        public IActionResult EveryOne([FromBody] MSG body)
        {
            var data = HttpContext.Response.Body;
            hubContext.Clients.All.SendAsync("EveryOne", $"{body.message}");
            return Ok();
        }

        /// <summary>
        /// 单个组推送
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [HttpPost, Route("AnyGroups")]
        public IActionResult AnyGroups([FromBody]SignalrGroups group)
        {
            if (group != null)
            {
                hubContext.Clients.Group(group.GroupName).SendAsync("AnyGroups", $"{group.Content}");
            }
            return Ok();
        }

        /// <summary>
        /// 多参数接收方式
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpGet, Route("MoreParamsRequest")]
        public IActionResult MoreParamsRequest(string message)
        {
            hubContext.Clients.All.SendAsync("MoreParamsRequest", message, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff"));
            return Ok();
        }
    }
    public class MSG
    {
        public string message { get; set; }
        public string excption { get; set; }
    }
}