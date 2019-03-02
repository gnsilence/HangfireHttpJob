使用说明
====
1.数据库配置，sqlserver ,redis 只需要在config中配置连接，就可以直接跑起来添加任务运行，mysql数据库需要用项目中的脚本先创建数据库及表，
然后配置连接就可以直接使用，其他数据源暂时没试

2.运行方式，宿主程序为webapi，需要用配置文件中的website地址运行，打开才是hangfire面板

3.添加了basic认证，账号密码在config配置，用来登录hangfire面板

windows服务部署
====
控制台方式运行,需要加参数 --console

Windows服务发布：直接发布webapi项目,在publish目录用管理员方式运行安装服务bat脚本，即可安装成功。

可以多实例部署
