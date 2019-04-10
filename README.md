使用说明
====
1.数据库配置，sqlserver ,redis，mysql只需要在config中配置连接，就可以直接跑起来添加任务运行，
其他数据库暂时用不到所以没试,(推荐使用redis，可以使用redis集群+多实例部署实现故障迁移和高可用)

2.运行方式，宿主程序为webapi，需要用配置文件中的website地址运行，打开才是hangfire面板

3.添加了basic认证，账号密码在config配置，用来登录hangfire面板

4.任务类型：

周期任务: 在周期任务面板，可以添加，编辑(注意不要编辑名称,目前是根据任务名称修改作业，更改名称后被认为是一个新的作业)

计划任务：在作业中的计划下，可以新增计划任务，计划任务只会执行一次，可以设置执行时间

Windows服务部署
====
控制台方式运行,需要加参数 --console

Windows服务发布：直接发布webapi项目,在publish目录用管理员方式运行安装服务bat脚本，即可安装成功。

可以多实例部署


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
新增功能
====

1，邮件推送配置，使用的腾讯的smtp，需要去邮箱设置里开启端口和获取密码，使用的mailkit插件，可以设置邮件模板
目前设置的为任务失败重试达到最大次数时会推送邮件，并将任务设置为失败。具体配置在appsetting中

2，signalR推送，服务寄宿的webapi，使用webapi推送，需要对应版本的js才能支持推送到web

3，分布式锁，在job过滤器中申请分布式锁，这样可以防止相同的任务并发执行，默认使用的秒作为超时时间单位。

4，接口健康检查，可以配置检查地址，然后访问的地址后面加上hc，可以在ui界面查看检查情况，需要每个接口提供检查地址
具体参考appsettings中的配置

5，后台进程，用来实现长时间持续运行的任务，或者秒级任务

6，暂停和继续任务，实现的原理是通过在set中添加任务属性，在过滤器中跳过执行达到暂停的目的，移除属性值后任务继续执行，
任务暂停后前台会渲染列表字体为红色，方便快速找出被暂停的任务

4月新增功能
====

1，升级hangfire版本到1.7，hangfire的sqlserver库到1.7版本，替换原有mysql存储库，升级1.7版本后支持通过cron表达式新增秒级任务
但原有的在线表达式生成中部分表达式不能使用，需要自行测试表达式或者查看(https://github.com/HangfireIO/Cronos)

2，通过宿主的webapi实现暴露接口，供外部程序添加，修改，删除任务，以及手动触发周期任务，还可以添加一个任务集合实现继承的连续任务

3，新增任务搜索，可以在周期任务，完成的历史任务中根据任务名称搜索，区分大小写的模糊搜索，并且可以标识出是否被暂停

