using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.StubResolver.Dns.ResourceRecords
{
    public class PTRRecord : ResourceRecord
    {
        public string PtrDName { get; set; }

        public override RecordType GetRecordType() => RecordType.PTR;
    }
}
