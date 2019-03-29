using Hangfire.HttpJob.Server;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobsServer
{
    public class HangfireSettings
    {
        /// <summary>
        /// 延迟加载
        /// </summary>
        private static readonly Lazy<HangfireSettings> _instance = new Lazy<HangfireSettings>(() => new HangfireSettings());

        public static HangfireSettings Instance => _instance.Value;

        public IConfigurationRoot Configuration { get; }

        private HangfireSettings()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
            //绑定服务到集合
            Configuration.GetSection("HealthChecks-UI:CheckUrls").Bind(HostServers);
            //绑定接收者邮箱
            Configuration.GetSection("SMTPConfig:SendToMailList").Bind(SendList);

            SendList.ForEach(p =>
            {
                SendMailList.Add(p.Email);
            });
            //绑定后台任务
            Configuration.GetSection("BackWorker").Bind(backWorker);
        }

        /// <summary>
        /// 服务地址
        /// </summary>
        public string ServiceAddress => Configuration["hangfire.server.serviceAddress"];

        /// <summary>
        /// 站点地址
        /// </summary>
        public string AppWebSite => Configuration["hangfire.server.website"];

        /// <summary>
        /// 登录用户名
        /// </summary>
        public string LoginUser => Configuration["hangfire.login.user"];

        /// <summary>
        /// 登录密码
        /// </summary>
        public string LoginPwd => Configuration["hangfire.login.pwd"];

        /// <summary>
        /// 使用redis
        /// </summary>
        public bool UseRedis => Convert.ToBoolean(Configuration["hangfire.UseRedis"]);
        /// <summary>
        /// 使用mysql
        /// </summary>
        public bool UseMySql => Convert.ToBoolean(Configuration["hangfire.UseMySql"]);
        /// <summary>
        /// 使用sqlserver
        /// </summary>
        public bool UseSqlSerVer => Convert.ToBoolean(Configuration["hangfire.UseSqlServer"]);

        /// <summary>
        /// sqlserver数据库连接
        /// </summary>
        public string HangfireSqlserverConnectionString => Configuration.GetConnectionString("hangfire.sqlserver");

        /// <summary>
        /// 使用mysql连接
        /// </summary>
        public string HangfireMysqlConnectionString => Configuration.GetConnectionString("hangfire.Mysql");

        /// <summary>
        ///  使用redis连接
        /// </summary>
        public string HangfireRedisConnectionString => Configuration.GetConnectionString("hangfire.redis");
        ///// <summary>
        ///// 使用Sqlite
        ///// </summary>
        //public string HangfireSqliteConnectionString=> Configuration["HealthChecks-UI:HealthCheckDatabaseConnectionString"];

        /// <summary>
        /// 健康检查api地址
        /// </summary>
        public List<HealthCheckInfo> HostServers { get; } = new List<HealthCheckInfo>();

        #region 邮件相关配置
        /// <summary>
        /// SMTP地址
        /// </summary>
        public string SMTPServerAddress => Configuration["SMTPConfig:SMTPServerAddress"];
        /// <summary>
        /// SMTP端口
        /// </summary>
        public int SMTPPort => Convert.ToInt32(Configuration["SMTPConfig:SMTPPort"]);
        /// <summary>
        /// 校验密码
        /// </summary>
        public string SMTPPwd => Configuration["SMTPConfig:SMTPPwd"];
        /// <summary>
        /// 发送者邮箱
        /// </summary>
        public string SendMailAddress => Configuration["SMTPConfig:SendMailAddress"];
        /// <summary>
        /// 邮件标题
        /// </summary>
        public string SMTPSubject => Configuration["SMTPConfig:SMTPSubject"];
        /// <summary>
        /// 接收者邮箱
        /// </summary>
        private List<Emails> SendList { get; } = new List<Emails>();
        /// <summary>
        /// 接收者邮箱
        /// </summary>
        public List<string> SendMailList { get; set; } = new List<string>();
        /// <summary>
        /// 使用后台进程
        /// </summary>
        public bool UseBackWorker => Convert.ToBoolean(Configuration["UseBackWorker"]);
        /// <summary>
        /// 后台进程
        /// </summary>
        public BackWorker backWorker = new BackWorker();
        #endregion
    }
    public class Emails
    {
        public string Email { get; set; }
    }
}
