
using Bns.Dns.Serialization;
using System.Collections.Generic;
using System.Net;

namespace Bns.Dns.ResourceRecords
{
    public class ARecord : ResourceRecord
    {
        public const int RDataLength = 4;

        public IPAddress Address { get; set; }

        public override RecordType GetRecordType() => RecordType.A;

        public override byte[] ToByteArray()
        {
            var addressBytes = this.Address.GetAddressBytes();

            var bytes = this.SerializeCommonFields();
            bytes.AppendIntAs2Bytes(RDataLength);
            bytes.AddRange(addressBytes);
            return bytes.ToArray();
        }
    }
}
