using System;
namespace Bns.Dns.ResourceRecords
{
    public class NSRecord : ResourceRecord
    {
        public string DName { get; set; }

        public override RecordType GetRecordType() => RecordType.NS;
    }
}
