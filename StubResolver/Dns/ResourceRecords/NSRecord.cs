using System;
namespace Bns.StubResolver.Dns.ResourceRecords
{
    public class NSRecord : ResourceRecord
    {
        public string DName { get; set; }

        public override RecordType GetRecordType() => RecordType.NS;
    }
}
