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
using CommonUtils;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

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
            if (UseApollo ? ConfigSettings.Instance.UseRedis : HangfireSettings.Instance.UseRedis)
            {
                Redis = ConnectionMultiplexer.Connect(HangfireSettings.Instance.HangfireRedisConnectionString);
            }
        }
        public static ConnectionMultiplexer Redis;

        public static readonly string[] ApiQueues = new[] { "apis", "jobs", "task", "rjob", "pjob", "rejob", "default" };
        public IConfiguration Configuration { get; }
        /// <summary>
        /// 是否使用apollo配置中心
        /// </summary>
        private static readonly bool UseApollo = ConfigSettings.Instance.UseApollo;
        public void ConfigureServices(IServiceCollection services)
        {
            //健康检查地址添加
            var hostlist = UseApollo ? JsonConvert.DeserializeObject<List<HealthCheckInfo>>(ConfigSettings.Instance.HostServers) : HangfireSettings.Instance.HostServers;
            //添加健康检查地址
            hostlist.ForEach(s =>
            {
                services.AddHealthChecks().AddUrlGroup(new Uri(s.Uri), s.httpMethod.ToLower() == "post" ? HttpMethod.Post : HttpMethod.Get, $"{s.Uri}");
            });
            //redis集群检查地址添加
            var redislist = UseApollo ? ConfigSettings.Instance.HangfireRedisConnectionString.Split(",").ToList() : HangfireSettings.Instance.HangfireRedisConnectionString.Split(",").ToList();
            redislist.ForEach(
                k =>
                {
                    if (k.Contains(":"))
                    {
                        services.AddHealthChecks().AddRedis(k, $"Redis: {k}");
                    }
                }
                );
            //services.AddHealthChecks().AddRedis(HangfireSettings.Instance.HangfireRedisConnectionString);
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


                    if (UseApollo ? ConfigSettings.Instance.UseSqlSerVer : HangfireSettings.Instance.UseSqlSerVer)
                    {

                        //使用SQL server
                        _ = config.UseSqlServerStorage(UseApollo ? ConfigSettings.Instance.HangfireSqlserverConnectionString
                            : HangfireSettings.Instance.HangfireSqlserverConnectionString, new Hangfire.SqlServer.SqlServerStorageOptions()
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

                    if (UseApollo ? ConfigSettings.Instance.UseRedis : HangfireSettings.Instance.UseRedis)
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
                            DashboardFooter = "XXX公司后台任务管理V1.0.0.0",
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

                    if (UseApollo ? ConfigSettings.Instance.UseMySql : HangfireSettings.Instance.UseMySql)
                    {
                        //使用mysql配置
                        config.UseStorage(new MySqlStorage(
                            UseApollo ? ConfigSettings.Instance.HangfireMysqlConnectionString :
                            HangfireSettings.Instance.HangfireMysqlConnectionString,
                            new MySqlStorageOptions
                            {
                                TablePrefix = "hangfire",
                                TransactionIsolationLevel = IsolationLevel.ReadCommitted,//实物隔离级别，默认为读取已提交
                                QueuePollInterval = TimeSpan.FromSeconds(1),//队列检测频率，秒级任务需要配置短点，一般任务可以配置默认时间
                                JobExpirationCheckInterval = TimeSpan.FromHours(1),//作业到期检查间隔（管理过期记录）。默认值为1小时
                                CountersAggregateInterval = TimeSpan.FromMinutes(5),//聚合计数器的间隔。默认为5分钟
                                PrepareSchemaIfNecessary = true,//设置true，则会自动创建表
                                DashboardJobListLimit = 50000,//仪表盘作业列表展示条数限制
                                TransactionTimeout = TimeSpan.FromMinutes(1),//事务超时时间，默认一分钟
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
            var supportedCultures = new[]
            {
                new CultureInfo("zh-CN"),
                new CultureInfo("en-US")
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
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
            var queues = new[] { "default", "apis", "localjobs" };
            app.UseHangfireServer(new BackgroundJobServerOptions()
            {
                ServerTimeout = TimeSpan.FromMinutes(4),
                SchedulePollingInterval = TimeSpan.FromSeconds(1),//秒级任务需要配置短点，一般任务可以配置默认时间，默认15秒
                ShutdownTimeout = TimeSpan.FromMinutes(30),//超时时间
                Queues = ApiQueues,//队列
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
                AppPath = UseApollo ? ConfigSettings.Instance.AppWebSite : HangfireSettings.Instance.AppWebSite,//返回时跳转的地址
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
                            Login =UseApollo?ConfigSettings.Instance.LoginUser:HangfireSettings.Instance.LoginUser,//登录账号
                            PasswordClear =  UseApollo?ConfigSettings.Instance.LoginPwd:HangfireSettings.Instance.LoginPwd//登录密码
                        }
                    }
                })
                }
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
