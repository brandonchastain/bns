
using Bns.Dns.Serialization;
using Dns.Serialization.Utils;

namespace Bns.Dns.ResourceRecords
{
    public class CNameRecord : ResourceRecord
    {
        public string CName { get; set; } // max length of 255 octets

        public override RecordType GetRecordType() => RecordType.CNAME;

        public override byte[] ToByteArray()
        {
            var bytes = this.SerializeCommonFields();
            var qBytes = QNameSerializer.SerializeQName(this.CName);

            bytes.AppendIntAs2Bytes(qBytes.Count);
            bytes.AddRange(qBytes);

            return bytes.ToArray();
        }
    }
}
