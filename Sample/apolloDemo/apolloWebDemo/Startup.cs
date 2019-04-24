using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.HttpJob;
using Hangfire.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Swagger;

namespace apolloWebDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Redis = ConnectionMultiplexer.Connect("47.105.185.242,password=abc@123,allowAdmin=true,SyncTimeout=10000");
        }

        public IConfiguration Configuration { get; }
        public static ConnectionMultiplexer Redis;
        private static readonly string[] queues = new[] { "jobs","apis","default","testjob","newjob"};
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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

                //config.UseSqlServerStorage("Server=.;Database=ApiDataBase;uid=sa;pwd=123456;MultipleActiveResultSets=true", new Hangfire.SqlServer.SqlServerStorageOptions()
                //{
                //    //每隔一小时检查过期job
                //    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                //    QueuePollInterval = TimeSpan.FromSeconds(1)
                //})
                //        .UseHangfireHttpJob(new HangfireHttpJobOptions()
                //        {
                //        })
                //        .UseConsole(new ConsoleOptions()
                //        {
                //            BackgroundColor = "#000079"
                //        })
                //        .UseDashboardMetric(DashboardMetrics.RecurringJobCount)
                //        .UseDashboardMetric(DashboardMetrics.AwaitingCount);
            });
            //services.AddSwaggerGen(s =>
            //{
            //    s.SwaggerDoc("apidoc", new Microsoft.OpenApi.Models.OpenApiInfo
            //    {
            //        Title = "apititle",
            //        Version = "v1.0",
            //        Description = "描述"
            //        //Contact = new Contact
            //        //{
            //        //    Name = Configuration["Swagger.Contact.Name"],
            //        //    Email = Configuration["Swagger.Contact.Email"]
            //        //}
            //    });

            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }
            app.UseHangfireServer(new BackgroundJobServerOptions()
            {
                ServerTimeout = TimeSpan.FromMinutes(4),
                SchedulePollingInterval = TimeSpan.FromSeconds(1),//秒级任务需要配置短点，一般任务可以配置默认时间，默认15秒
                ShutdownTimeout = TimeSpan.FromMinutes(30),//超时时间
                Queues = queues,//队列
                WorkerCount = Math.Max(Environment.ProcessorCount, 40)//工作线程数，当前允许的最大线程，默认20
            });
            app.UseHangfireDashboard("/hjob", new DashboardOptions
            {
                AppPath = "#",
                DisplayStorageConnectionString = false,//是否显示数据库连接信息
                IsReadOnlyFunc = Context =>
                {

                    return false;
                }
            });
            //app.UseSwagger();
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint($"apidoc/swagger.json", "DemoAPI V1");
            //});
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
