using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.StubResolver.Dns
{
    public enum HeaderOpCode
    {
        StandardQuery = 0,
        InverseQuery = 1,
        ServerStatusRequest = 2,
        Undefined = 4,
    }
}
