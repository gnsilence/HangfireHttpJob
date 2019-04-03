using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Hangfire.Server;
using Hangfire;
using Hangfire.Storage;

namespace JobsServer.Controllers
{
    [Produces("application/json")]
    [Route("api/AddJobs")]
    public class AddJobsController : Controller
    {
        /// <summary>
        /// 添加一个队列任务立即被执行
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        [HttpPost, Route("AddBackGroundJob")]
        public JsonResult AddBackGroundJob([FromBody] Hangfire.HttpJob.Server.HttpJobItem httpJob)
        {
            var addreslut = string.Empty;
            try
            {
                addreslut = BackgroundJob.Enqueue(() => Hangfire.HttpJob.Server.HttpJob.Excute(httpJob, httpJob.JobName, null));
            }
            catch (Exception ec)
            {
                return Json(new Message() { Code = false, ErrorMessage = ec.ToString() });
            }
            return Json(new Message() { Code = true, ErrorMessage = "" });
        }

        /// <summary>
        /// 添加一个周期任务
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        [HttpPost, Route("AddOrUpdateRecurringJob")]
        public JsonResult AddOrUpdateRecurringJob([FromBody] Hangfire.HttpJob.Server.HttpJobItem httpJob)
        {
            try
            {
                RecurringJob.AddOrUpdate(httpJob.JobName, () => Hangfire.HttpJob.Server.HttpJob.Excute(httpJob, httpJob.JobName, null), httpJob.Corn, TimeZoneInfo.Local);
            }
            catch (Exception ec)
            {
                return Json(new Message() { Code = false, ErrorMessage = ec.ToString() });
            }
            return Json(new Message() { Code = true, ErrorMessage = "" });
        }

        /// <summary>
        /// 删除一个周期任务
        /// </summary>
        /// <param name="jobname"></param>
        /// <returns></returns>
        [HttpGet,Route("DeleteJob")]
        public JsonResult DeleteJob(string jobname)
        {
            try
            {
                RecurringJob.RemoveIfExists(jobname);
            }
            catch (Exception ec)
            {
                return Json(new Message() { Code = false, ErrorMessage = ec.ToString() });
            }
            return Json(new Message() { Code = true, ErrorMessage = "" });
        }
        /// <summary>
        /// 手动触发一个任务
        /// </summary>
        /// <param name="jobname"></param>
        /// <returns></returns>
        [HttpGet, Route("TriggerRecurringJob")]
        public JsonResult TriggerRecurringJob(string jobname)
        {
            try
            {
                RecurringJob.Trigger(jobname);
            }
            catch (Exception ec)
            {
                return Json(new Message() { Code = false, ErrorMessage = ec.ToString() });
            }
            return Json(new Message() { Code = true, ErrorMessage = "" });
        }
        /// <summary>
        /// 添加一个延迟任务
        /// </summary>
        /// <param name="httpJob">httpJob.DelayFromMinutes（延迟多少分钟执行）</param>
        /// <returns></returns>
        [HttpPost, Route("AddScheduleJob")]
        public JsonResult AddScheduleJob([FromBody] Hangfire.HttpJob.Server.HttpJobItem httpJob)
        {
            var reslut = string.Empty;
            try
            {
                reslut = BackgroundJob.Schedule(() => Hangfire.HttpJob.Server.HttpJob.Excute(httpJob, httpJob.JobName, null), TimeSpan.FromMinutes(httpJob.DelayFromMinutes));
            }
            catch (Exception ec)
            {
                return Json(new Message() { Code = false, ErrorMessage = ec.ToString() });
            }
            return Json(new Message() { Code = true, ErrorMessage = "" });
        }
        /// <summary>
        /// 添加连续任务,多个任务依次执行，只执行一次
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        [HttpPost, Route("AddContinueJob")]
        public JsonResult AddContinueJob([FromBody] List<Hangfire.HttpJob.Server.HttpJobItem> httpJobItems)
        {
            var reslut = string.Empty;
            var jobid = string.Empty;
            try
            {
                httpJobItems.ForEach(k =>
                {
                    if (!string.IsNullOrEmpty(jobid))
                    {
                        jobid = BackgroundJob.ContinueJobWith(jobid, () => RunContinueJob(k));
                    }
                    else
                    {
                        jobid = BackgroundJob.Enqueue(() => Hangfire.HttpJob.Server.HttpJob.Excute(k, k.JobName, null));
                    }
                });
                reslut = "true";
            }
            catch (Exception ec)
            {
                return Json(new Message() { Code = false, ErrorMessage = ec.ToString() });
            }
            return Json(new Message() { Code = true, ErrorMessage = "" });
        }
        /// <summary>
        /// 执行连续任务
        /// </summary>
        /// <param name="httpJob"></param>
        public void RunContinueJob(Hangfire.HttpJob.Server.HttpJobItem httpJob)
        {
            BackgroundJob.Enqueue(() => Hangfire.HttpJob.Server.HttpJob.Excute(httpJob, httpJob.JobName, null));
        }
    }
    /// <summary>
    /// 返回消息
    /// </summary>
    public class Message
    {
        public bool Code { get; set; }
        public string ErrorMessage { get; set; }
    }
}