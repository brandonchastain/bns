using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.Dns
{
    public enum ResponseCode
    {
        NoError = 0,
        FormatError = 1,
        ServerFailure = 2,
        NameError = 3,
        NotImplemented = 4,
        Refused = 5,
    }
}
