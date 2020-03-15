using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.Dns.ResourceRecords
{
    public class PTRRecord : ResourceRecord
    {
        public string PtrDName { get; set; }

        public override RecordType GetRecordType() => RecordType.PTR;
    }
}
