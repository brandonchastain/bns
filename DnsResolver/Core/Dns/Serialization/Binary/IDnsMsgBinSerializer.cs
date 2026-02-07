using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.Dns.Serialization
{
    public interface IDnsMsgBinSerializer
    {
        DnsMessage Deserialize(byte[] buffer);

        byte[] Serialize(DnsMessage dnsMessage);
    }
}
