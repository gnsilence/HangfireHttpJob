using Hangfire.Dashboard;
using Hangfire.HttpJob.Dashboard.Pages;
using Hangfire.HttpJob.Dashboard;
using Hangfire.HttpJob.Server;
using Hangfire.HttpJob.Support;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Hangfire.HttpJob.InterFace;

namespace Hangfire.HttpJob
{
    public static class GlobalConfigurationExtension
    {
        public static IGlobalConfiguration UseHangfireHttpJob(this IGlobalConfiguration config, HangfireHttpJobOptions options = null)
        {
            if (options == null) options = new HangfireHttpJobOptions();
            var assembly = typeof(HangfireHttpJobOptions).GetTypeInfo().Assembly;
            //处理http请求
            DashboardRoutes.Routes.Add("/httpjob", new HttpJobDispatcher(options));
            DashboardRoutes.Routes.AddRazorPage("/corn", x => new CornJobsPage());

            var jsPath = DashboardRoutes.Routes.Contains("/js[0-9]+") ? "/js[0-9]+" : "/js[0-9]{3}";
            DashboardRoutes.Routes.Append(jsPath, new EmbeddedResourceDispatcher(assembly, "Hangfire.HttpJob.Content.jsoneditor.js"));
            DashboardRoutes.Routes.Append(jsPath, new DynamicJsDispatcher(options));
            DashboardRoutes.Routes.Append(jsPath, new EmbeddedResourceDispatcher(assembly, "Hangfire.HttpJob.Content.httpjob.js"));

            var cssPath = DashboardRoutes.Routes.Contains("/css[0-9]+") ? "/css[0-9]+" : "/css[0-9]{3}";
            DashboardRoutes.Routes.Append(cssPath, new EmbeddedResourceDispatcher(assembly, "Hangfire.HttpJob.Content.jsoneditor.css"));
            DashboardRoutes.Routes.Append(cssPath, new DynamicCssDispatcher(options));

            if (options.GlobalHttpTimeOut < 60) options.GlobalHttpTimeOut = 60;
            Server.HttpJob.HangfireHttpJobOptions = options;
            JobFilter.HangfireHttpJobOptions = options;
            AutomaticRetrySetAttribute.HangfireHttpJobOptions = options;
            return config;
        }

        public static void AddHttpJob(this IServiceCollection services)
        {
            // 启用异步io
            services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true);
            // 添加httpClientFactory
            services.AddHttpClient();

            services.TryAddSingleton<IHttpJobService, HttpJobService>();
        }
    }
}