using Bns.Dns.Serialization;
using Dns.Serialization.Utils;
using System;
namespace Bns.Dns.ResourceRecords
{
    public class NSRecord : ResourceRecord
    {
        public string DName { get; set; }

        public override RecordType GetRecordType() => RecordType.NS;

        public override byte[] ToByteArray()
        {
            var bytes = this.SerializeCommonFields();
            var qBytes = QNameSerializer.SerializeQName(this.DName);

            bytes.AppendIntAs2Bytes(qBytes.Count);
            bytes.AddRange(qBytes);

            return bytes.ToArray();
        }
    }
}
