using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.Heartbeat;
using Hangfire.HttpJob;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using StackExchange.Redis;
using System.Globalization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region 后台任务 hangfire

// 如果使用iis需要单独设置异步io
builder.Services.Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);
// 添加httpjob的依赖
builder.Services.AddHttpJob();
//builder.Host.UseSerilog((context, logger) =>
//{
//    logger.ReadFrom.Configuration(context.Configuration);
//    logger.Enrich.FromLogContext();
//});

#region 配置日志 log4net

builder.Logging.AddLog4Net(Path.Combine(AppContext.BaseDirectory, "log4net.config"));
ILoggerRepository repository = LogManager.CreateRepository("ServerSampleRepository");
XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));

#endregion 配置日志 log4net

builder.Services.AddHangfire(config =>
{
    // 自定义样式及脚本导入，必须设置为嵌入式资源
    DashboardRoutes.AddStylesheet(typeof(Program).GetTypeInfo().Assembly, "ServerSample.Content.job.css");
    DashboardRoutes.AddJavaScript(typeof(Program).GetTypeInfo().Assembly, "ServerSample.Content.job.js");

    config.UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(1));
    config.UseDarkModeSupportForDashboard();
    var Redis = ConnectionMultiplexer.Connect(builder.Configuration["RedisServer"].ToString());
    config.UseRedisStorage(Redis, new Hangfire.Redis.RedisStorageOptions()
    {
        Db = 10,
        FetchTimeout = TimeSpan.FromMinutes(5),
        Prefix = "{IMFHangfire}:",
        //活动服务器超时时间
        InvisibilityTimeout = TimeSpan.FromHours(1),
        //任务过期检查频率
        ExpiryCheckInterval = TimeSpan.FromHours(1),
        DeletedListSize = 10000,
        SucceededListSize = 10000
    })
    .UseHangfireHttpJob(new HangfireHttpJobOptions()
    {
        UseEmail = true,// 使用邮箱
        AutomaticDelete = 2,// 设置作业执行后多久过期，单位天， 默认3天
        // 重试配置
        AttemptsCountArray = new List<int>() { 5, 10, 20 },//重试时间间隔，数组长度是重试次数
        AddHttpJobButtonName = "添加计划任务",
        AddRecurringJobHttpJobButtonName = "添加定时任务",
        EditRecurringJobButtonName = "编辑定时任务",
        PauseJobButtonName = "暂停或开始",
        //DashboardTitle = "后台任务",
        DashboardName = "后台任务管理",
        DashboardFooter = "后台任务管理",
    })
    .UseConsole(new ConsoleOptions()
    {
        BackgroundColor = "#000000"
    })
    .UseDashboardMetrics(new DashboardMetric[] { DashboardMetrics.AwaitingCount, DashboardMetrics.ProcessingCount, DashboardMetrics.RecurringJobCount, DashboardMetrics.RetriesCount, DashboardMetrics.FailedCount, DashboardMetrics.SucceededCount })
                            ;
});

var listqueue = new[] { "default", "apis", "localjobs" };// 队列，必须包含默认default
builder.Services.AddHangfireServer(op =>
{
    op.ServerTimeout = TimeSpan.FromMinutes(4);
    op.SchedulePollingInterval = TimeSpan.FromSeconds(1);// 秒级任务需要配置短点，一般任务可以配置默认时间，默认15秒
    op.ShutdownTimeout = TimeSpan.FromMinutes(30);// 超时时间
    op.Queues = listqueue.ToArray();// 队列
    op.WorkerCount = Math.Max(Environment.ProcessorCount, 40);// 工作线程数，当前允许的最大线程，默认20
    op.StopTimeout = TimeSpan.FromSeconds(20);
});

#endregion 后台任务 hangfire

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var supportedCultures = new[]
           {
                new CultureInfo("zh-CN")
            };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("zh-CN"),
    // Formatting numbers, dates, etc.
    SupportedCultures = supportedCultures,
    // UI strings that we have localized.
    SupportedUICultures = supportedCultures
});
// 登录面板设置
app.UseHangfireDashboard("/job", new Hangfire.DashboardOptions
{
    AppPath = "#",// 返回时跳转的地址
    DisplayStorageConnectionString = false,// 是否显示数据库连接信息
    IsReadOnlyFunc = Context =>
    {
        var isreadonly = false;
        return isreadonly;
    },
    //AsyncAuthorization=
    Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = false,// 是否启用ssl验证，即https
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users = new []
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login ="admin",// 登录账号
                            PasswordClear = "admin"// 登录密码
                        }
                    }
                })
    }
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();