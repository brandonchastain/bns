
namespace Bns.Dns.ResourceRecords
{
    public class CNameRecord : ResourceRecord
    {

        public string CName { get; set; } // max length of 255 octets

        public override RecordType GetRecordType() => RecordType.CNAME;

    }
}
