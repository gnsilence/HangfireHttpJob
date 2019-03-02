using System.Net;

namespace Hangfire.HttpJob
{
    public class HangfireHttpJobOptions
    {
        public int GlobalHttpTimeOut { get; set; } = 5000;
        public string AddHttpJobButtonName { get; set; } = "新增计划任务";
        public string AddRecurringJobHttpJobButtonName { get; set; } = "添加周期任务";
        public string AddCronButtonName { get; set; } = "Cron表达式生成";
        public string EditRecurringJobButtonName { get; set; } = "编辑周期任务";
        public string CloseButtonName { get; set; } = "关闭";
        public string SubmitButtonName { get; set; } = "提交";
        public string ScheduledEndPath { get; set; } = "jobs/scheduled";
        public string RecurringEndPath { get; set; } = "/recurring";
        public string AddCron { get; set; } = "/recurring";
        public string EditRecurringJobEndPath { get; set; } = "/recurring";

        /// <summary>
        /// 代理设置
        /// </summary>
        public IWebProxy Proxy { get; set; }


    }
}
