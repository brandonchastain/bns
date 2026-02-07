
using Bns.Dns.Serialization;
using System.Collections.Generic;
using System.Net;

namespace Bns.Dns.ResourceRecords
{
    public class AAAARecord : ResourceRecord
    {
        public const int Length = 16;

        public IPAddress Address { get; set; }

        public override RecordType GetRecordType() => RecordType.AAAA;

        public override byte[] ToByteArray()
        {
            var addressBytes = this.Address.GetAddressBytes();

            var bytes = this.SerializeCommonFields();
            bytes.AppendIntAs2Bytes(Length);
            bytes.AddRange(addressBytes);
            return bytes.ToArray();
        }
    }
}
