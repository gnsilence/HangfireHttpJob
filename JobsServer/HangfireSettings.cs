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
        public string HangfireMysqlConnectionString=>Configuration.GetConnectionString("hangfire.Mysql");

        /// <summary>
        ///  使用redis连接
        /// </summary>
        public string HangfireRedisConnectionString => Configuration.GetConnectionString("hangfire.redis");
    }
}
