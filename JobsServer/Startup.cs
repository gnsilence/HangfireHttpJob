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
using Hangfire.Heartbeat;
using Hangfire.Heartbeat.Server;
using Hangfire.Dashboard;
using JobsServer.Hubs;
using System.Net;
using Hangfire.HttpJob.Support;
using Hangfire.Server;

namespace JobsServer
{
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
                    //使用服务器资源监视
                    config.UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(1));
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
                        _ = config.UseSqlServerStorage(HangfireSettings.Instance.HangfireSqlserverConnectionString, new Hangfire.SqlServer.SqlServerStorageOptions()
                        {
                            //每隔一小时检查过期job
                            JobExpirationCheckInterval = TimeSpan.FromHours(1),
                            QueuePollInterval = TimeSpan.FromSeconds(1)
                        })
                        .UseHangfireHttpJob(new HangfireHttpJobOptions()
                        {
                            SendToMailList = HangfireSettings.Instance.SendMailList,
                            SendMailAddress = HangfireSettings.Instance.SendMailAddress,
                            SMTPServerAddress = HangfireSettings.Instance.SMTPServerAddress,
                            SMTPPort = HangfireSettings.Instance.SMTPPort,
                            SMTPPwd = HangfireSettings.Instance.SMTPPwd,
                            SMTPSubject = HangfireSettings.Instance.SMTPSubject
                        })
                        .UseConsole(new ConsoleOptions()
                        {
                            BackgroundColor = "#000079"
                        })
                        .UseDashboardMetric(DashboardMetrics.RecurringJobCount)
                        .UseDashboardMetric(DashboardMetrics.AwaitingCount);
                    }

                    if (HangfireSettings.Instance.UseRedis)
                    {
                        //使用redis
                        config.UseRedisStorage(Redis, new Hangfire.Redis.RedisStorageOptions()
                        {
                            //活动服务器超时时间
                            InvisibilityTimeout = TimeSpan.FromHours(1),
                            //任务过期检查频率
                            ExpiryCheckInterval = TimeSpan.FromMinutes(30),
                            DeletedListSize = 1000,
                            SucceededListSize = 1000
                        })
                        .UseHangfireHttpJob(new HangfireHttpJobOptions()
                        {
                            SendToMailList = HangfireSettings.Instance.SendMailList,
                            SendMailAddress = HangfireSettings.Instance.SendMailAddress,
                            SMTPServerAddress = HangfireSettings.Instance.SMTPServerAddress,
                            SMTPPort = HangfireSettings.Instance.SMTPPort,
                            SMTPPwd = HangfireSettings.Instance.SMTPPwd,
                            SMTPSubject = HangfireSettings.Instance.SMTPSubject
                        })
                        .UseConsole(new ConsoleOptions()
                        {
                            BackgroundColor = "#000079"
                        })
                        .UseDashboardMetric(DashboardMetrics.AwaitingCount)
                        .UseDashboardMetric(DashboardMetrics.ProcessingCount)
                        .UseDashboardMetric(DashboardMetrics.RecurringJobCount)
                        .UseDashboardMetric(DashboardMetrics.RetriesCount)
                        .UseDashboardMetric(DashboardMetrics.FailedCount)
                        .UseDashboardMetric(DashboardMetrics.ServerCount);
                    }

                    if (HangfireSettings.Instance.UseMySql)
                    {
                        //使用mysql配置
                        config.UseStorage(new MySqlStorage(
                            HangfireSettings.Instance.HangfireMysqlConnectionString,
                            new MySqlStorageOptions
                            {
                                TablePrefix="hangfire",
                                TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                                QueuePollInterval = TimeSpan.FromSeconds(1),//检测频率，秒级任务需要配置短点，一般任务可以配置默认时间
                                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                                PrepareSchemaIfNecessary = false,
                                DashboardJobListLimit = 50000,
                                TransactionTimeout = TimeSpan.FromMinutes(1),
                            })).UseConsole(new ConsoleOptions()
                            {
                                BackgroundColor = "#000079"
                            })//使用日志展示
                            .UseHangfireHttpJob(new HangfireHttpJobOptions()
                            {
                                SendToMailList = HangfireSettings.Instance.SendMailList,
                                SendMailAddress = HangfireSettings.Instance.SendMailAddress,
                                SMTPServerAddress = HangfireSettings.Instance.SMTPServerAddress,
                                SMTPPort = HangfireSettings.Instance.SMTPPort,
                                SMTPPwd = HangfireSettings.Instance.SMTPPwd,
                                SMTPSubject = HangfireSettings.Instance.SMTPSubject
                            });//启用http任务
                    }

                }
                );
            #region SignalR

            services.AddSignalR();
            //设置redis底板
            //.AddRedis(HangfireSettings.Instance.HangfireRedisConnectionString, options =>
            //{
            //    options.Configuration.DefaultDatabase = 5;
            //    options.Configuration.ChannelPrefix = "MySignalR";
            //    options.Configuration.ConnectRetry = 3;
            //    options.Configuration.ReconnectRetryPolicy = new ExponentialRetry(5000);
            //    options.Configuration.ConnectTimeout = 60000;
            //    //options.ConnectionFactory= async writer =>
            //    //{
            //    //    var config = new ConfigurationOptions
            //    //    {
            //    //        AbortOnConnectFail = false
            //    //    };
            //    //    config.EndPoints.Add(IPAddress.Loopback, 0);
            //    //    config.SetDefaultPorts();
            //    //    var connection = await ConnectionMultiplexer.ConnectAsync(config, writer);
            //    //    connection.ConnectionFailed += (_, e) =>
            //    //    {
            //    //        Console.WriteLine("Connection to Redis failed.");
            //    //    };

            //    //    if (!connection.IsConnected)
            //    //    {
            //    //        Console.WriteLine("Did not connect to Redis.");
            //    //    }

            //    //    return connection;
            //    //};

            //});
            //跨域设置
            services.AddCors(options => options.AddPolicy("CorsPolicy",
            builder =>
            {
                builder.AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowCredentials();
            }));
            //依赖注入
            services.AddSingleton<IServiceProvider, ServiceProvider>();
            #endregion
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
                SchedulePollingInterval=TimeSpan.FromSeconds(1),//秒级任务需要配置短点，一般任务可以配置默认时间，默认15秒
                ShutdownTimeout = TimeSpan.FromMinutes(30),//超时时间
                Queues = queues,//队列
                WorkerCount = Math.Max(Environment.ProcessorCount, 40)//工作线程数，当前允许的最大线程，默认20
            },
            //服务器资源检测频率
            additionalProcesses: new IBackgroundProcess[] { new SystemMonitor(checkInterval: TimeSpan.FromSeconds(1)) }//new[] { new SystemMonitor(checkInterval: TimeSpan.FromSeconds(1))}
            );
            #region 后台进程
            if (HangfireSettings.Instance.UseBackWorker)
            {
                var listprocess = new List<IBackgroundProcess>
                {
                    new BackWorkers(HangfireSettings.Instance.backWorker)
                };
                app.UseHangfireServer(new BackgroundJobServerOptions()
                {
                    ServerName = $"{Environment.MachineName}-BackWorker",
                    WorkerCount = 20,
                    Queues = new[] { "test", "api", "demo" }
                }, additionalProcesses: listprocess);
            }
            #endregion

            app.UseHangfireDashboard("/job", new DashboardOptions
            {
                AppPath = HangfireSettings.Instance.AppWebSite,//返回时跳转的地址
                DisplayStorageConnectionString = false,//是否显示数据库连接信息
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
            var options = new HealthCheckOptions
            {
                ResponseWriter = async (c, r) =>
                {
                    c.Response.ContentType = "application/json";

                    var result = JsonConvert.SerializeObject(new
                    {
                        status = r.Status.ToString(),
                        errors = r.Entries.Select(e => new { key = e.Key, value = e.Value.Status.ToString() })
                    });
                    await c.Response.WriteAsync(result);
                }
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
            #region SignalR
            //跨域支持
            app.UseCors("CorsPolicy");
            app.UseSignalR(routes =>
            {
                routes.MapHub<SignalrHubs>("/Hubs");
            });
            app.UseWebSockets();
            #endregion
            app.UseMvc();
        }
    }
}
