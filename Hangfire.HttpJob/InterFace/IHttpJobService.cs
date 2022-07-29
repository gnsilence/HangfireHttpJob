using Hangfire.HttpJob.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.HttpJob.InterFace
{
    public interface IHttpJobService
    {
        /// <summary>
        /// 手动触发一个周期任务
        /// </summary>
        /// <param name="JobName"></param>
        /// <returns></returns>
        Task TrrigerJob(string JobName);

        /// <summary>
        /// 添加或修改周期任务
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        Task AddOrUpdateRecurringJob(HttpJobItem httpJob);

        /// <summary>
        /// 将httpjob当作队列任务调用
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        Task EnqueueBackGroundHttpJob(HttpJobItem httpJob);

        /// <summary>
        /// 删除一个周期任务
        /// </summary>
        /// <param name="jobname"></param>
        /// <returns></returns>
        Task DeleteRecurringJob(string jobname);

        /// <summary>
        /// 添加一个延迟任务
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        Task AddScheduleJob(HttpJobItem httpJob, TimeSpan timeSpan);

        /// <summary>
        /// 添加一个延迟任务,支持指定到具体执行时间
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        Task AddScheduleJob(HttpJobItem httpJob, DateTimeOffset dateTimeOffset);

        /// <summary>
        /// 添加n个连续执行的任务，按照数组顺序依次执行
        /// </summary>
        /// <param name="httpjobs"></param>
        /// <returns></returns>
        Task AddContinueRecurringJobs(List<HttpJobItem> httpjobs);

        /// <summary>
        ///获取所有周期任务集合
        /// </summary>
        /// <returns></returns>
        Task<List<HttpJobItem>> GetAllRecurringHttpJobs();
    }
}