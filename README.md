2022-7-27
====
# 破坏性更新
## 1：移除配置中心，identityserver等不实用的配置
## 2：升级hangfire 到最新版1.8.0 RC1,添加 .net6的示例项目 移除旧的示例项目
## 3：重要变化：
### job任务使用HttpClientFactory代替直接使用HttpClient发送请求，httpjob 的创建，发送接口请求全部改为异步方法
### 加入Kestrel参数的异步io设置(iis下需要在宿主项目设置异步io) 需要使用Service.AddHttpJob来配置，可参考示例项目
### 增加配置项，现在重试次数需要在配置中进行配置，数组的长度就是重试次数，增加任务过期天数配置
### Dashboard UI 增加暗黑模式，根据电脑系统，黑色及浅色主题自动切换 1.8.0 新特性
### 现在可以通过 DashboardRoutes.AddStylesheet 及 DashboardRoutes.AddJavaScript 注入自定义样式及脚本，可以实现修改系统样式及添加自定义功能  1.8.0 新特性
### 修复.net 6 下 增加httpjob失败的bug

********
以下文档废弃：
2019.5.3
====
更新hangfire到1.7.2 
现在更改环境后作业会自动转换时区，无需手动修改作业，如：Linux和Windows共用一个redis集群。

演示地址：http://47.105.185.242:9006/hjob
账号：admin
密码：123456

只读面板地址：http://47.105.185.242:9006/job-read
账号：test
密码：123456

演示环境：阿里云服务器 Centos 7.3 + Docker + Redis,使用源码中的示例程序直接部署(Linux下要注意时区问题，
时区不影响作业正常执行，时间会显示有误)


使用说明
====
拓展的目的是用来调用api接口，达到不依赖业务代码，部署方便，使用方便。

快速使用核心功能集成到项目中：

引用项目包 Install-Package Hangfire.HttpJob.Ext -Version 1.0.4 

引用项目包 Install-Package Hangfire.HttpJob.Ext.Storage.All -Version 1.0.0 

或者nuget管理器搜索以上包安装

其中Hangfire.HttpJob.Ext.Storage.All 主要是集成了开源存储库方便使用：

AzureDocumentDB，SqlServer，MySql， Oracle ，SQLite ，Redis, LiteDB , PostgreSql , Mongo 
以及Dashboard.BasicAuthorization 升级到standard 2.0 

然后只需要在core的startup中配置即可，配置方式见文末


任务类型：

周期任务: 在周期任务面板，可以添加，编辑(注意不要编辑名称,目前是根据任务名称修改作业，更改名称后被认为是一个新的作业)

计划任务：在作业中的计划下，可以新增计划任务，计划任务只会执行一次，可以设置执行时间

Windows服务部署
====
注意：控制台方式运行调试时,需要加参数 --console

Windows服务发布：直接发布webapi项目,在publish目录用管理员方式运行安装服务bat脚本，即可安装成功。

可以多实例部署，推荐使用redis集群+多实例部署实现高可用

部分截图
====

任务面板
====
![image](https://github.com/gnsilence/HangfireHttpJob/blob/master/JobsServer/screenshots/Dashboard.png)

read-only 面板
====
![image](https://github.com/gnsilence/HangfireHttpJob/blob/master/JobsServer/screenshots/readonly.png)

图表展示
====
![image](https://github.com/gnsilence/HangfireHttpJob/blob/master/JobsServer/screenshots/Charts.png)

计划任务，延迟指定分钟后执行，只执行一次
====
![image](https://github.com/gnsilence/HangfireHttpJob/blob/master/JobsServer/screenshots/planjob.png)

通过接口添加任务，触发任务
====
![image](https://github.com/gnsilence/HangfireHttpJob/blob/master/JobsServer/screenshots/ApiForAddJob.png)

redis集群测试
====
![image](https://github.com/gnsilence/HangfireHttpJob/blob/master/JobsServer/screenshots/redisclusertest.png)


新增搜索框功能，在完成的作业和周期作业中可以模糊搜索任务进行操作(模糊搜索区分大小写)
====
![image](https://github.com/gnsilence/HangfireHttpJob/blob/master/JobsServer/screenshots/SearchJobs.png)

自身redis集群健康检查,方便查出哪个地址出现问题，也可以添加其他可以访问到的redis地址
====
![image](https://github.com/gnsilence/HangfireHttpJob/blob/master/JobsServer/screenshots/redischeck.png)


使用Apollo统一配置中心对hangfire多实例进行统一管理
====
![image](https://github.com/gnsilence/HangfireHttpJob/blob/master/JobsServer/screenshots/apollo.png)

其他功能
====

1，邮件推送配置，使用的腾讯的smtp，需要去邮箱设置里开启端口和获取密码，使用的mailkit插件，可以设置邮件模板
目前设置的为任务失败重试达到最大次数时会推送邮件，并将任务设置为失败。需要在appsetting中配置

2，signalR推送，服务寄宿的webapi，使用webapi推送，需要对应版本的js才能支持推送到web(参考源码)


3，接口健康检查，可以配置检查地址，然后访问的地址后面加上hc，可以在ui界面查看检查情况，需要每个接口提供检查地址
具体参考appsettings中的配置(参考源码)

4，后台进程，用来实现长时间持续运行的任务(一般用来执行长时间运行的任务)

5，暂停和继续任务，实现的原理是通过在set中添加任务属性，在过滤器中跳过执行达到暂停的目的，移除属性值后任务继续执行，
任务暂停后前台会渲染列表字体为红色，方便快速找出被暂停的任务

6，新增重试，队列名称参数，可以在新增周期任务及计划任务中添加，实现手动分配队列，指定是否需要重试，在某些场景下，不需要重试时
可以使用此设置。


其他：
====

升级1.7版本后支持通过cron表达式新增秒级任务
但原有的在线表达式生成中部分表达式不能使用，需要自行测试表达式或者查看(https://github.com/HangfireIO/Cronos)

通过宿主的webapi实现暴露接口，供外部程序添加，修改，删除任务，以及手动触发周期任务，还可以添加一个任务集合实现继承的连续任务

新增任务搜索，可以在周期任务，完成的历史任务中根据任务名称搜索，区分大小写的模糊搜索，并且可以标识出是否被暂停

升级部分类库到standa 2.0 ,升级hangfire到最新版1.7.1

添加使用apollo配置中心的共用配置文件，配置方式:

私有配置：每一个实例在apollo中添加一个应用，只配置私有命名空间application，只需要添加访问地址之类的实例私有配置

共用配置：单独使用一个实例添加公共命名空间，无需引用这个应用id到系统，这样可以保证所有实例
统一使用公有配置部分，而且可以实现各个实例运行不同的地址，端口，及其他私有配置。(共用配置如邮件通知,任务过期时间，健康检查地址)


核心功能配置方式参考
====

配置部分代码，支持配置面板的按钮名称，无需配置config,(需要其他功能参考源码项目)：

startup中

 public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            //使用redis集群，注意集群需要手动提前配置好，主从节点

            Redis = ConnectionMultiplexer.Connect("127.0.0.1:6380,127.0.0.1:7001,127.0.0.1:7003,allowAdmin=true,SyncTimeout=10000");
        }


public static ConnectionMultiplexer Redis;

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
                .UseDashboardMetric(DashboardMetrics.FailedCount)
                .UseDashboardMetric(DashboardMetrics.ServerCount);

            }

         app.UseHangfireServer(new BackgroundJobServerOptions()
            {
                ServerTimeout = TimeSpan.FromMinutes(4),
                SchedulePollingInterval = TimeSpan.FromSeconds(1),//秒级任务需要配置短点，一般任务可以配置默认时间，默认15秒
                ShutdownTimeout = TimeSpan.FromMinutes(30),//超时时间
                Queues = new[] { "apis" },//队列
                WorkerCount = Math.Max(Environment.ProcessorCount, 40)//工作线程数，当前允许的最大线程，默认20
            });

            //此处hjob可以随意填写
            app.UseHangfireDashboard("/hjob", new DashboardOptions
            {
                AppPath = "#",
                DisplayStorageConnectionString = false,//是否显示数据库连接信息
                IsReadOnlyFunc = Context =>
                {

                    return false;//是否只读面板
                }
            });

