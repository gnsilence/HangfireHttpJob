using System.Collections.Generic;
using System.Net;

namespace Hangfire.HttpJob
{
    public class HangfireHttpJobOptions
    {
        /// <summary>
        /// 默认超时时间/秒
        /// </summary>
        public int GlobalHttpTimeOut { get; set; } = 900;

        /// <summary>
        /// 新增计划任务按钮名称
        /// </summary>
        public string AddHttpJobButtonName { get; set; } = "新增计划任务";

        /// <summary>
        /// 添加周期任务按钮名称
        /// </summary>
        public string AddRecurringJobHttpJobButtonName { get; set; } = "添加周期任务";

        /// <summary>
        /// cron表达式按钮名称
        /// </summary>
        public string AddCronButtonName { get; set; } = "Cron表达式生成";

        /// <summary>
        /// 编辑任务按钮名称
        /// </summary>
        public string EditRecurringJobButtonName { get; set; } = "编辑周期任务";

        internal string CloseButtonName { get; set; } = "关闭";
        internal string SubmitButtonName { get; set; } = "提交";

        /// <summary>
        /// 停止或开始任务按钮名称
        /// </summary>
        public string PauseJobButtonName { get; set; } = "停止或开始任务";

        internal string ScheduledEndPath { get; set; } = "jobs/scheduled";
        internal string RecurringEndPath { get; set; } = "/recurring";
        internal string AddCron { get; set; } = "/recurring";
        internal string EditRecurringJobEndPath { get; set; } = "/recurring";

        /// <summary>
        /// 更改Dashboard标题
        /// </summary>
        public string DashboardTitle { get; set; } = "任务管理面板";

        /// <summary>
        /// 管理面板名称
        /// </summary>
        public string DashboardName { get; set; } = "任务管理";

        /// <summary>
        /// 更改底部footer取代hangfire版本名称
        /// </summary>
        public string DashboardFooter { get; set; } = "XXX后台管理V1.0.0";

        /// <summary>
        /// 是否达到错误次数时发送邮件
        /// </summary>
        public bool UseEmail { get; set; } = false;

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

        /// <summary>
        /// 多久作业过期(默认3天)
        /// </summary>
        public int AutomaticDelete { get; set; } = 3;

        /// <summary>
        /// 任务失败后重试的间隔时间，单位秒，重试次数是数组长度
        /// </summary>
        public List<int> AttemptsCountArray { get; set; } = new List<int>() { 10, 20, 30, 40, 50, 60 };

        /// <summary>
        /// 当接口内部返回非200 状态码，标记作业为删除
        /// </summary>
        public bool DeleteOnFail { get; set; }
    }
}