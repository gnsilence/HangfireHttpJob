using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
namespace JobsServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            //如果是控制台下使用，后面加上console参数即可，默认是服务方式
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            if (isService)
            {
                //获取当前程序所在目录
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
            }

            var builder = CreateWebHostBuilder(
                args.Where(arg => arg != "--console").ToArray());

            var host = builder.Build();

            if (isService)
            {
                // To run the app without the CustomWebHostService change the
                // next line to host.RunAsService();
                host.RunAsService();
            }
            else
            {
                host.Run();
            }
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls(HangfireSettings.Instance.ServiceAddress)//启用配置的地址
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddEventLog();//启用系统事件日志，
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Configure the app here.
                }).UseStartup<Startup>();
        }
    }
}
