using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.StubResolver.Dns.ResourceRecords
{
    public class SOARecord : ResourceRecord
    {
        public string MName { get; set; }

        public string RName { get; set; }
        
        public uint Serial { get; set; }

        public int RefreshInterval { get; set; }

        public int RetryInterval { get; set; }

        public int ExpireInterval { get; set; }

        public uint Minimum { get; set; }

        public override RecordType GetRecordType() => RecordType.SOA;

    }
}
