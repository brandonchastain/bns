using Bns.StubResolver.Dns.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Bns.StubResolver.Dns.ResourceRecords
{
    public class ARecord : ResourceRecord
    {
        public const int Length = 4;

        public IPAddress Address { get; set; }

        public override RecordType GetRecordType() => RecordType.A;
    }
}
