using Hangfire.Dashboard;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.HttpJob.Dashboard
{
    public class DynamicJsDispatcher : IDashboardDispatcher
    {
        private readonly HangfireHttpJobOptions _options;
        public DynamicJsDispatcher(HangfireHttpJobOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _options = options;
        }

        public Task Dispatch(DashboardContext context)
        {
            var builder = new StringBuilder();

            builder.Append(@"(function (hangFire) {")
                .Append("hangFire.httpjobConfig = hangFire.httpjobConfig || {};")
                .AppendFormat("hangFire.httpjobConfig.AddHttpJobButtonName = '{0}';", _options.AddHttpJobButtonName)
                .AppendFormat("hangFire.httpjobConfig.AddRecurringJobHttpJobButtonName = '{0}';", _options.AddRecurringJobHttpJobButtonName)
                .AppendFormat("hangFire.httpjobConfig.AddCronButtonName = '{0}';", _options.AddCronButtonName)
                .AppendFormat("hangFire.httpjobConfig.EditRecurringJobButtonName = '{0}';", _options.EditRecurringJobButtonName)
                .AppendFormat("hangFire.httpjobConfig.CloseButtonName = '{0}';", _options.CloseButtonName)
                .AppendFormat("hangFire.httpjobConfig.SubmitButtonName = '{0}';", _options.SubmitButtonName)
                .AppendFormat("hangFire.httpjobConfig.GlobalHttpTimeOut = {0};", _options.GlobalHttpTimeOut)
                .AppendFormat("hangFire.httpjobConfig.AddHttpJobUrl = '{0}/httpjob?op=backgroundjob';", context.Request.PathBase)
                .AppendFormat("hangFire.httpjobConfig.AddCronUrl = '{0}/corn';", context.Request.PathBase)
                .AppendFormat("hangFire.httpjobConfig.AddRecurringJobUrl = '{0}/httpjob?op=recurringjob';", context.Request.PathBase)
                 .AppendFormat("hangFire.httpjobConfig.GetRecurringJobUrl = '{0}/httpjob?op=GetRecurringJob';", context.Request.PathBase)
                 .AppendFormat("hangFire.httpjobConfig.EditRecurringJobUrl = '{0}/httpjob?op=EditRecurringJob';", context.Request.PathBase)
                .AppendFormat("hangFire.httpjobConfig.NeedAddNomalHttpJobButton = location.href.substring(location.href.length-'{0}'.length)=='{0}';", _options.ScheduledEndPath)
                .AppendFormat("hangFire.httpjobConfig.NeedAddRecurringHttpJobButton = location.href.substring(location.href.length-'{0}'.length)=='{0}';", _options.RecurringEndPath)
                .AppendFormat("hangFire.httpjobConfig.NeedAddCronButton = location.href.substring(location.href.length-'{0}'.length)=='{0}';", _options.AddCron)
                .AppendFormat("hangFire.httpjobConfig.NeedEditRecurringJobButton = location.href.substring(location.href.length-'{0}'.length)=='{0}';", _options.EditRecurringJobEndPath)
                .Append("})(window.Hangfire = window.Hangfire || {});")
                .AppendLine();

            return context.Response.WriteAsync(builder.ToString());
        }
    }
}
