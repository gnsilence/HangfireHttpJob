using Hangfire.Client;
using Hangfire.Common;
using Hangfire.HttpJob.Server;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.HttpJob.Support
{
    /// <summary>
    /// 任务过滤
    /// </summary>
    public class JobFilter : JobFilterAttribute,
     IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
    {
        //超时时间
        /// <summary>
        /// 分布式锁过期时间
        /// </summary>
        private readonly int _timeoutInSeconds;

        public static HangfireHttpJobOptions HangfireHttpJobOptions;

        public JobFilter()
        {
        }

        private readonly ILog _logger = LogProvider.For<JobFilter>();

        public void OnCreated(CreatedContext filterContext)
        {
            _logger.Info(
            $"创建任务 `{filterContext.Job.Method.Name}` id为 `{filterContext.BackgroundJob?.Id}`"
           );
        }

        public void OnCreating(CreatingContext filterContext)
        {
            //判断任务是否被暂停
            using (var connection = JobStorage.Current.GetConnection())
            {
                var conts = connection.GetAllItemsFromSet($"JobPauseOf:{filterContext.Job.Args[1]}");
                if (conts.Contains("true"))
                {
                    filterContext.Canceled = true;//任务被暂停不执行直接跳过
                    return;
                }
            }
            _logger.Info($"开始创建任务{filterContext.Job.Method.Name}");
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            if (!filterContext.Items.ContainsKey("DistributedLock"))
            {
                throw new InvalidOperationException("找不到分布式锁，没有为该任务申请分布式锁.");
            }
            //释放分布式锁
            var distributedLock = (IDisposable)filterContext.Items["DistributedLock"];
            distributedLock.Dispose();
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            //设置分布式锁,分布式锁会阻止两个相同的任务并发执行，用任务名称和方法名称作为锁
            var jobresource = $"{filterContext.BackgroundJob.Job.Args[1]}.{filterContext.BackgroundJob.Job.Method.Name}";
            var locktime = JsonConvert.DeserializeObject<HttpJobItem>(filterContext.BackgroundJob.Job.Args[0].ToString())?.LockTimeOut ?? 0;
            var locktimeout = TimeSpan.FromSeconds(locktime);
            try
            {
                //申请分布式锁
                var distributedLock = filterContext.Connection.AcquireDistributedLock(jobresource, locktimeout);
                filterContext.Items["DistributedLock"] = distributedLock;
            }
            catch (Exception ec)
            {
                //获取锁超时，取消任务，任务会默认置为成功
                filterContext.Canceled = true;
                _logger.Error($"任务{filterContext.BackgroundJob.Job.Args[1]}超时,任务id{filterContext.BackgroundJob.Id}异常信息:{ec}");
            }
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            //设置过期时间，任务将在三天后过期，过期的任务会自动被扫描并删除
            context.JobExpirationTimeout = TimeSpan.FromDays(HangfireHttpJobOptions.AutomaticDelete);
        }

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState failedState)
            {
                _logger.Warn(
                   $"任务 `{context.BackgroundJob.Id}` 执行失败，异常为 `{failedState.Exception}`");
            }
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(HangfireHttpJobOptions.AutomaticDelete);
        }

        /// <summary>
        /// 清除日志文件，每隔20天按日期清理一次
        /// </summary>
        [Obsolete("已删除")]
        private Task DeleteLogfiles()
        {
            DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}/logs/");
            if (!dir.Exists) { return Task.CompletedTask; }
            var taskdelete = Task.Run(() =>
              {
                  try
                  {
                      FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
                      foreach (FileSystemInfo i in fileinfo)
                      {
                          if (i is DirectoryInfo)            //判断是否文件夹
                          {
                              var dridate = Convert.ToDateTime(i.Name);
                              if ((DateTime.Now - dridate).TotalDays >= 20)
                              {
                                  DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                                  subdir.Delete(true);          //删除子目录和文件
                              }
                          }
                      }
                  }
                  catch (Exception ex)
                  {
                      _logger.Error($"删除日志文件出错：{ex.Message}");
                  }
              });
            return Task.CompletedTask;
        }
    }
}