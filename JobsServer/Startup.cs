using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire.MySql.Core;
using System.Data;
using Hangfire.HttpJob;
using Hangfire.Console;
using Hangfire.Dashboard.BasicAuthorization;
using StackExchange.Redis;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using HealthChecks.Uris;
using Hangfire.SQLite;

namespace JobsServer
{
    public class RandomHealthCheck
        : IHealthCheck
    {

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (DateTime.UtcNow.Minute % 2 == 0)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            return Task.FromResult(HealthCheckResult.Unhealthy(description: "出现异常"));
        }
    }
    public class Startup
    {
        ////sqlite数据库路径
        //private static string SqliteDbPath =
        //    AppDomain.CurrentDomain.BaseDirectory + ("\\HangfireDb;");

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
            //添加健康检查地址
            HangfireSettings.Instance.HostServers.ForEach(s =>
            {
                services.AddHealthChecks().AddUrlGroup(new Uri(s.Uri), s.httpMethod.ToLower() == "post" ? HttpMethod.Post : HttpMethod.Get, $"{s.Uri}");
            });
            services.AddHangfire(
                config =>
                {
                    //sqlite数据库，操作缓慢不建议使用
                    //config.UseSQLiteStorage($"Data Source={SqliteDbPath};", new SQLiteStorageOptions()
                    //{
                    //    TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                    //    QueuePollInterval = TimeSpan.FromSeconds(15),
                    //    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    //    CountersAggregateInterval = TimeSpan.FromMinutes(5),
                    //    PrepareSchemaIfNecessary = false,
                    //    DashboardJobListLimit = 50000,
                    //    TransactionTimeout = TimeSpan.FromMinutes(1),
                    //}).UseHangfireHttpJob().UseConsole();


                    if (HangfireSettings.Instance.UseSqlSerVer)
                    {
                        //使用SQL server
                        config.UseSqlServerStorage(HangfireSettings.Instance.HangfireSqlserverConnectionString)
                        .UseHangfireHttpJob()
                        .UseConsole();
                    }

                    if (HangfireSettings.Instance.UseRedis)
                    {
                        //使用redis
                        config.UseRedisStorage(Redis)
                        .UseHangfireHttpJob(new HangfireHttpJobOptions()
                        {
                            SendToMailList=HangfireSettings.Instance.SendMailList,
                            SendMailAddress=HangfireSettings.Instance.SendMailAddress,
                            SMTPServerAddress=HangfireSettings.Instance.SMTPServerAddress,
                            SMTPPort=HangfireSettings.Instance.SMTPPort,
                            SMTPPwd=HangfireSettings.Instance.SMTPPwd,
                            SMTPSubject=HangfireSettings.Instance.SMTPSubject
                        })
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
            services.AddHealthChecksUI();
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
            //重写json报告数据，可用于远程调用获取健康检查结果
            var options = new HealthCheckOptions();
            options.ResponseWriter = async (c, r) =>
            {
                c.Response.ContentType = "application/json";

                var result = JsonConvert.SerializeObject(new
                {
                    status = r.Status.ToString(),
                    errors = r.Entries.Select(e => new { key = e.Key, value = e.Value.Status.ToString() })
                });
                await c.Response.WriteAsync(result);
            };

            app.UseHealthChecks("/healthz", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.UseHealthChecks("/health", options);//获取自定义格式的json数据
            app.UseHealthChecksUI(setup =>
            {
                setup.UIPath = "/hc"; // 健康检查的UI面板地址
                setup.ApiPath = "/hc-api"; // 用于api获取json的检查数据
            });
            app.UseMvc();
        }
    }
}
