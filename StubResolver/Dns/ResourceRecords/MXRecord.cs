using System;
namespace Bns.StubResolver.Dns.ResourceRecords
{
    public class MXRecord : ResourceRecord
    {
        public int Preference { get; set; }

        public string ExchangeDName { get; set; }

        public override RecordType GetRecordType() => RecordType.MX;
    }
}
