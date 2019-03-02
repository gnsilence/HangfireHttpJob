using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Hangfire.MySql.Core;
using System.Data;
using Hangfire.HttpJob;
using Hangfire.Console;
using Hangfire.Dashboard.BasicAuthorization;
using StackExchange.Redis;

namespace JobsServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            if (HangfireSettings.Instance.UseRedis)
            {
                Redis = ConnectionMultiplexer.Connect(HangfireSettings.Instance.HangfireRedisConnectionString);
            }
        }
        public static ConnectionMultiplexer Redis;
        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(
                config =>
                {
                    if (HangfireSettings.Instance.UseSqlSerVer)
                    {
                        //使用SQL server
                        config.UseSqlServerStorage(HangfireSettings.Instance.HangfireSqlserverConnectionString)
                        .UseHangfireHttpJob().
                        UseConsole();
                    }

                    if (HangfireSettings.Instance.UseRedis)
                    {
                        //使用redis
                        config.UseRedisStorage(Redis)
                        .UseHangfireHttpJob()
                        .UseConsole();
                    }

                    if (HangfireSettings.Instance.UseMySql)
                    {
                        //使用mysql配置
                        config.UseStorage(new MySqlStorage(
                            HangfireSettings.Instance.HangfireMysqlConnectionString,
                            new MySqlStorageOptions
                            {
                                TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                                QueuePollInterval = TimeSpan.FromSeconds(15),
                                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                                PrepareSchemaIfNecessary = false,
                                DashboardJobListLimit = 50000,
                                TransactionTimeout = TimeSpan.FromMinutes(1),
                            })).UseConsole()//使用日志展示
                            .UseHangfireHttpJob();//启用http任务
                    }

                }
                );
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            var queues = new[] { "default", "apis", "localjobs" };
            app.UseHangfireServer(new BackgroundJobServerOptions()
            {
                ShutdownTimeout = TimeSpan.FromMinutes(30),//等待所有任务执行的时间当服务被关闭时
                Queues = queues,//队列
                WorkerCount = Math.Max(Environment.ProcessorCount, 20)//工作线程数，当前允许的最大线程，默认20
            });
            app.UseHangfireDashboard("/job", new DashboardOptions
            {
                AppPath = HangfireSettings.Instance.AppWebSite,//返回时跳转的地址
                Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = false,//是否启用ssl验证，即https
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users = new []
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login = HangfireSettings.Instance.LoginUser,//登录账号
                            PasswordClear =  HangfireSettings.Instance.LoginPwd//登录密码
                        }
                    }
                })
                }
            });
            app.UseMvc();
        }
    }
}
