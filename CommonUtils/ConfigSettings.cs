using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonUtils
{
   public class ConfigSettings
    {
        private static readonly Lazy<ConfigSettings> _instance = new Lazy<ConfigSettings>(() => new ConfigSettings());
        public static ConfigSettings Instance => _instance.Value;
        public IConfigurationRoot Configuration { get; }
        public ConfigSettings()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }
        /// <summary>
        /// 站点地址
        /// </summary>
        public string URL => Configuration["hangfire.server.serviceAddress"];
    }
}
