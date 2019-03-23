using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hangfire.HttpJob.Support
{
    public class JobFilter: JobFilterAttribute,
     IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
    {
        private static readonly ILog Logger = LogProvider.For<JobFilter>();
        public void OnCreated(CreatedContext filterContext)
        {
            Logger.InfoFormat(
            "创建任务 `{0}` id为 `{1}`",
            filterContext.Job.Method.Name,
            filterContext.BackgroundJob?.Id);
            
        }

        public void OnCreating(CreatingContext filterContext)
        {
            Logger.Info($"开始创建任务{filterContext.Job.Method.Name}");
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            //执行job
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            //开始执行job
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            //设置过期时间，任务将在三天后过期，过期的任务会自动被扫描并删除
            context.JobExpirationTimeout = TimeSpan.FromDays(3);
        }

        public void OnStateElection(ElectStateContext context)
        {
            var failedState = context.CandidateState as FailedState;
            if (failedState != null)
            {
                
                Logger.WarnFormat(
                    "任务 `{0}` 执行失败，异常为 `{1}`",
                    context.BackgroundJob.Id,
                    failedState.Exception);
            }
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(3);
        }
    }
}
