using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Bns.StubResolver.Core
{
    public class ResolverOptions
    {

        public int ListenPort { get; set; } = 53;

        public string RrIpAddress { get; set; } = "10.0.1.29";
    }
}
