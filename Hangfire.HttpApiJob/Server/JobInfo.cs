using System;
using System.Collections.Generic;
using System.Text;

namespace Hangfire.HttpApiJob.Server
{
    public class JobInfo
    {
        public string Type { get; set; }
        public string Method { get; set; }
        public string ParameterTypes { get; set; }
        public string Arguments { get; set; }
}
}
