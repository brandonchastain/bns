using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Bns.StubResolver.Dns.ResourceRecords
{
    public class WKSRecord : ResourceRecord
    {
        public IPAddress Address { get; set; }

        public int Protocol { get; set; }

        public override RecordType GetRecordType() => RecordType.WKS;
    }
}
