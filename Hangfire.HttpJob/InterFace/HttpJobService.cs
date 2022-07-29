using Hangfire.HttpJob.Server;
using Hangfire.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.HttpJob.InterFace
{
    public class HttpJobService : IHttpJobService
    {
        /// <summary>
        /// 添加延续性任务，根据任务集合进行顺序执行
        /// </summary>
        /// <param name="httpjobs"></param>
        /// <returns></returns>
        public async Task AddContinueRecurringJobs(List<HttpJobItem> httpjobs)
        {
            var reslut = string.Empty;
            var jobid = string.Empty;
            try
            {
                httpjobs.ForEach(k =>
                {
                    if (!string.IsNullOrEmpty(jobid))
                    {
                        jobid = BackgroundJob.ContinueJobWith<Server.HttpJob>(jobid, a => a.ExcuteAsync(k, k.JobName, k.QueueName, k.IsRetry, null));
                    }
                    else
                    {
                        jobid = BackgroundJob.Enqueue<Server.HttpJob>(a => a.ExcuteAsync(k, k.JobName, k.QueueName, k.IsRetry, null));
                    }
                });
                reslut = "true";
            }
            catch (Exception ec)
            {
            }
        }

        /// <summary>
        /// 添加周期任务
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        public async Task AddOrUpdateRecurringJob(HttpJobItem httpJob)
        {
            RecurringJob.AddOrUpdate<Server.HttpJob>(httpJob.JobName, a => a.ExcuteAsync(httpJob, httpJob.JobName, httpJob.QueueName, httpJob.IsRetry, null), httpJob.Corn, new RecurringJobOptions()
            {
                QueueName = httpJob.QueueName,
                TimeZone = TimeZoneInfo.Local
            });
            await Task.CompletedTask;
        }

        /// <summary>
        /// 添加一个延迟任务
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        public async Task AddScheduleJob(HttpJobItem httpJob, TimeSpan timeSpan)
        {
            BackgroundJob.Schedule<Server.HttpJob>(a => a.ExcuteAsync(httpJob, httpJob.JobName, httpJob.QueueName, httpJob.IsRetry, null), timeSpan);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 添加一个延迟任务指定到具体执行时间
        /// </summary>
        /// <param name="httpJob"></param>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        public async Task AddScheduleJob(HttpJobItem httpJob, DateTimeOffset dateTimeOffset)
        {
            BackgroundJob.Schedule<Server.HttpJob>(a => a.ExcuteAsync(httpJob, httpJob.JobName, httpJob.QueueName, httpJob.IsRetry, null), dateTimeOffset);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除一个周期任务
        /// </summary>
        /// <param name="jobname"></param>
        /// <returns></returns>
        public async Task DeleteRecurringJob(string jobname)
        {
            RecurringJob.RemoveIfExists(jobname);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 添加一个队列任务
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        public async Task EnqueueBackGroundHttpJob(HttpJobItem httpJob)
        {
            BackgroundJob.Enqueue<Server.HttpJob>(a => a.ExcuteAsync(httpJob, httpJob.JobName, httpJob.QueueName, httpJob.IsRetry, null));
            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取所有周期任务
        /// </summary>
        /// <returns></returns>
        public async Task<List<HttpJobItem>> GetAllRecurringHttpJobs()
        {
            var list = new List<HttpJobItem>();
            using (var connection = JobStorage.Current.GetConnection())
            {
                var joblist = StorageConnectionExtensions.GetRecurringJobs(connection);
                joblist.ForEach(job =>
                {
                    var httpjob = JsonConvert.DeserializeObject<HttpJobItem>(job.Job.Args.FirstOrDefault().ToString());
                    list.Add(httpjob);
                });
            }
            return await Task.FromResult(list);
        }

        /// <summary>
        /// 手动触发一个周期任务
        /// </summary>
        /// <param name="JobName"></param>
        /// <returns></returns>
        public async Task TrrigerJob(string JobName)
        {
            RecurringJob.Trigger(JobName);
            await Task.CompletedTask;
        }
    }
}