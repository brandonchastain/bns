using Bns.Dns.Serialization;
using Dns.Serialization.Utils;
using System;
namespace Bns.Dns.ResourceRecords
{
    public class MXRecord : ResourceRecord
    {
        public int Preference { get; set; }

        public string ExchangeDName { get; set; }

        public override RecordType GetRecordType() => RecordType.MX;

        public override byte[] ToByteArray()
        {
            var bytes = this.SerializeCommonFields();
            var dnameBytes = QNameSerializer.SerializeQName(this.ExchangeDName);
            var numBytes = dnameBytes.Count + 2;
            bytes.AppendIntAs2Bytes(numBytes);
            bytes.AppendIntAs2Bytes(this.Preference);
            bytes.AddRange(dnameBytes);

            return bytes.ToArray();
        }
    }
}
