﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.Dns.Serialization
{
    public interface IJsonSerializer
    {
        string ToJson(object o);
        string PrettyPrint(string json);
    }
}
