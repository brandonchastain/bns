using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Bns.Dns.ResourceRecords
{
    public class WKSRecord : ResourceRecord
    {
        public IPAddress Address { get; set; }

        public int Protocol { get; set; }

        public override RecordType GetRecordType() => RecordType.WKS;
    }
}
