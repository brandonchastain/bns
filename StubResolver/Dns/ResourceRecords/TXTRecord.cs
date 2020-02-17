using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.StubResolver.Dns.ResourceRecords
{
    public class TXTRecord : ResourceRecord
    {
        public string TextData { get; set; }

        public int Length { get; set; }

        public override RecordType GetRecordType() => RecordType.TXT;
    }
}
