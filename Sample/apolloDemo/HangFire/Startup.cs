using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.HttpJob;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace HangFire
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Redis = ConnectionMultiplexer.Connect("127.0.0.1,password=123456,allowAdmin=true,SyncTimeout=10000");
        }

        public static ConnectionMultiplexer Redis;
        private static readonly string[] queues = new[] { "jobs", "apis", "default", "testjob", "newjob" };
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddHangfire(config =>
            {
                //使用redis
                config.UseRedisStorage(Redis, new Hangfire.Redis.RedisStorageOptions()
                {
                    FetchTimeout = TimeSpan.FromMinutes(5),
                    Prefix = "{hangfire}:",
                    //活动服务器超时时间
                    InvisibilityTimeout = TimeSpan.FromHours(1),
                    //任务过期检查频率
                    ExpiryCheckInterval = TimeSpan.FromHours(1),
                    DeletedListSize = 10000,
                    SucceededListSize = 10000
                })

                .UseHangfireHttpJob(new HangfireHttpJobOptions()
                {
                    AddHttpJobButtonName = "添加计划任务",
                    AddRecurringJobHttpJobButtonName = "添加定时任务",
                    EditRecurringJobButtonName = "编辑定时任务",
                    PauseJobButtonName = "暂停或开始",
                    DashboardTitle = "XXX公司任务管理",
                    DashboardName = "后台任务管理",
                    DashboardFooter = "XXX公司后台任务管理V1.0.0.0"
                })
                .UseConsole(new ConsoleOptions()
                {
                    BackgroundColor = "#000079"
                })
                .UseDashboardMetric(DashboardMetrics.AwaitingCount)
                .UseDashboardMetric(DashboardMetrics.ProcessingCount)
                .UseDashboardMetric(DashboardMetrics.RecurringJobCount)
                .UseDashboardMetric(DashboardMetrics.RetriesCount)
                .UseDashboardMetric(DashboardMetrics.FailedCount);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            
            var supportedCultures = new[]
            {
                new CultureInfo("zh-CN"),
                new CultureInfo("en-US")
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                //指定默认的语言为中文
                DefaultRequestCulture = new RequestCulture("zh-CN"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHangfireServer(new BackgroundJobServerOptions()
            {
                ServerTimeout = TimeSpan.FromMinutes(4),
                SchedulePollingInterval = TimeSpan.FromSeconds(1),//秒级任务需要配置短点，一般任务可以配置默认时间，默认15秒
                ShutdownTimeout = TimeSpan.FromMinutes(30),//超时时间
                Queues = queues,//队列
                WorkerCount = Math.Max(Environment.ProcessorCount, 40)//工作线程数，当前允许的最大线程，默认20
            });
            //只读面板，只能读取不能操作
            app.UseHangfireDashboard("/job-read", new DashboardOptions
            {
                IgnoreAntiforgeryToken = true,
                AppPath = "#",//返回时跳转的地址
                DisplayStorageConnectionString = false,//是否显示数据库连接信息
                IsReadOnlyFunc = Context =>
                {
                    return true;
                },
                Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = false,//是否启用ssl验证，即https
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users = new []
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login = "read",
                            PasswordClear = "only"
                        },
                        new BasicAuthAuthorizationUser
                        {
                            Login = "test",
                            PasswordClear = "123456"
                        },
                        new BasicAuthAuthorizationUser
                        {
                            Login = "guest",
                            PasswordClear = "123@123"
                        }
                    }
                })
                }
            });
            app.UseHangfireDashboard("/hjob", new DashboardOptions
            {
                //防止CSRF攻击
                IgnoreAntiforgeryToken=true,//忽略token，linux下不支持
                AppPath = "#",
                DisplayStorageConnectionString = false,//是否显示数据库连接信息
                IsReadOnlyFunc = Context =>
                {

                    return false;
                },
                Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = false,//是否启用ssl验证，即https
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users = new []
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login ="admin",//登录账号
                            PasswordClear =  "123456"//登录密码
                        }
                    }
                })
                }
            });
            app.UseMvc();
        }
    }
}
