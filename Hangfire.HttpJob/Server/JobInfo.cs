using System;
using System.Collections.Generic;
using System.Text;

namespace Hangfire.HttpJob.Server
{
    public class JobInfo
    {
        public string Type { get; set; }
        public string Method { get; set; }
        public string ParameterTypes { get; set; }
        public string Arguments { get; set; }
        public int RetryCount { get; set; }
}
}
