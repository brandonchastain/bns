﻿
using System.Net;

namespace Bns.Dns.ResourceRecords
{
    public class ARecord : ResourceRecord
    {
        public const int Length = 4;

        public IPAddress Address { get; set; }

        public override RecordType GetRecordType() => RecordType.A;
    }
}
