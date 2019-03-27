using System.Collections.Generic;
using System.Net;

namespace Hangfire.HttpJob
{
    public class HangfireHttpJobOptions
    {
        public int GlobalHttpTimeOut { get; set; } = 900;
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
        /// SMTP地址
        /// </summary>
        public string SMTPServerAddress { get; set; }
        /// <summary>
        /// SMTP端口
        /// </summary>
        public int SMTPPort { get; set; }
        /// <summary>
        /// 校验密码
        /// </summary>
        public string SMTPPwd { get; set; }
        /// <summary>
        /// 发送者邮箱
        /// </summary>
        public string SendMailAddress { get; set; }
        /// <summary>
        /// 邮件标题
        /// </summary>
        public string SMTPSubject { get; set; }
        /// <summary>
        /// 接收者邮箱
        /// </summary>
        public List<string> SendToMailList { get; set; }

        /// <summary>
        /// 代理设置
        /// </summary>
        public IWebProxy Proxy { get; set; }


    }
}
