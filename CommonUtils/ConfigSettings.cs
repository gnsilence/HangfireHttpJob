using Com.Ctrip.Framework.Apollo;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonUtils
{
    /// <summary>
    /// Apollo配置中心使用此配置
    /// </summary>
    public class ConfigSettings
    {
        private static readonly Lazy<ConfigSettings> _instance = new Lazy<ConfigSettings>(() => new ConfigSettings());
        public static ConfigSettings Instance => _instance.Value;
        public IConfigurationRoot Configuration { get; set; }
        public ConfigSettings()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            //获取私有命名空间
            builder.AddApollo(builder.Build().GetSection("apolloserver"))
                .AddDefault()
                .AddNamespace("application")//当前实例的私有基础配置(私有，不共享，只对当前运行程序生效)
                .AddNamespace("development.Server.Common")//基础配置(公共配置,所有实例共享)
                .AddNamespace("development.Server.Email")//邮件配置(公共配置，所有实例共享)
                .AddNamespace("development.Server.Health");//健康检查配置(公共配置，所有实例共享)

            Configuration = builder.Build();

        }
        /// <summary>
        /// 站点地址,本地配置使用
        /// </summary>
        public string URL => Configuration["hangfire.server.serviceAddress"];
        /// <summary>
        /// 服务地址
        /// </summary>
        public string ServiceAddress => Configuration["server.serviceAddress"];

        /// <summary>
        /// 站点地址
        /// </summary>
        public string AppWebSite => Configuration["server.website"];

        /// <summary>
        /// 登录用户名
        /// </summary>
        public string LoginUser => Configuration["login.user"];

        /// <summary>
        /// 登录密码
        /// </summary>
        public string LoginPwd => Configuration["login.pwd"];

        /// <summary>
        /// 使用redis
        /// </summary>
        public bool UseRedis => Convert.ToBoolean(Configuration["UseRedis"]);
        /// <summary>
        /// 使用mysql
        /// </summary>
        public bool UseMySql => Convert.ToBoolean(Configuration["UseMySql"]);
        /// <summary>
        /// 使用sqlserver
        /// </summary>
        public bool UseSqlSerVer => Convert.ToBoolean(Configuration["UseSqlServer"]);

        /// <summary>
        /// sqlserver数据库连接
        /// </summary>
        public string HangfireSqlserverConnectionString => Configuration["sqlserver"];

        /// <summary>
        /// 使用mysql连接
        /// </summary>
        public string HangfireMysqlConnectionString => Configuration["Mysql"];

        /// <summary>
        ///  使用redis连接
        /// </summary>
        public string HangfireRedisConnectionString => Configuration["redis"];
        ///// <summary>
        ///// 使用Sqlite
        ///// </summary>
        //public string HangfireSqliteConnectionString=> Configuration["HealthChecks-UI:HealthCheckDatabaseConnectionString"];
        /// <summary>
        /// 是否使用apollo配置中心
        /// </summary>
        public bool UseApollo => Convert.ToBoolean(Configuration["hangfire.UseApollo"]);
        /// <summary>
        /// 健康检查api地址
        /// </summary>
        public string HostServers => Configuration["CheckUrls"];
        /// <summary>
        /// 已完成作业过期时间(过期后会被自动删除)
        /// </summary>
        public int AutomaticDelete => Convert.ToInt32(Configuration["AutomaticDelete"]);

        #region 邮件相关配置
        /// <summary>
        /// SMTP地址
        /// </summary>
        public string SMTPServerAddress => Configuration["SMTPServerAddress"];
        /// <summary>
        /// SMTP端口
        /// </summary>
        public int SMTPPort => Convert.ToInt32(Configuration["SMTPPort"]);
        /// <summary>
        /// 校验密码
        /// </summary>
        public string SMTPPwd => Configuration["SMTPPwd"];
        /// <summary>
        /// 发送者邮箱
        /// </summary>
        public string SendMailAddress => Configuration["SendMailAddress"];
        /// <summary>
        /// 邮件标题
        /// </summary>
        public string SMTPSubject => Configuration["SMTPSubject"];

        /// <summary>
        /// 接收者邮箱
        /// </summary>
        public string SendMailList => Configuration["SendToMailList"];
        /// <summary>
        /// 使用后台进程
        /// </summary>
        public bool UseBackWorker => Convert.ToBoolean(Configuration["UseBackWorker"]);
        /// <summary>
        /// 后台进程
        /// </summary>
        //public BackWorker backWorker = new BackWorker();
        #endregion
    }

    public class HealthCheckInfo
    {
        /// <summary>
        /// 接口url地址
        /// </summary>
        public string Uri { get; set; }
        /// <summary>
        /// http方法
        /// </summary>
        public string HttpMethod { get; set; }
    }

    public class BackWorker
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string UrL { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 方法类型
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// 执行频率/秒
        /// </summary>
        public int Internal { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// 请求参数类型
        /// </summary>
        public string ContentType { get; set; }
    }

    public class Emails
    {
        public string Email { get; set; }
    }
}
