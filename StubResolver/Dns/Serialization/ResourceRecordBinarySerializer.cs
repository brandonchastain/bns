using Bns.StubResolver.Dns.ResourceRecords;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.StubResolver.Dns.Serialization
{
    public class ResourceRecordBinarySerializer
    {
        private readonly DnsQuestionBinarySerializer dnsSerializer;

        public ResourceRecordBinarySerializer(DnsQuestionBinarySerializer dnsSerializer)
        {
            this.dnsSerializer = dnsSerializer;
        }

        public byte[] ToByteArray(ResourceRecord rr)
        {
            var bytes = new List<byte>();
            var qNameBytes = this.dnsSerializer.SerializeQName(rr.Name);
            bytes.AddRange(qNameBytes);

            int rrTypeNum = ((int)rr.GetRecordType()) + 1;
            bytes.AppendIntAs2Bytes(rrTypeNum);

            var rrClassNum = ((int)rr.GetRecordClass()) + 1;
            bytes.AppendIntAs2Bytes(rrClassNum);

            bytes.AppendIntAs4Bytes(rr.TimeToLive);

            switch (rr)
            {
                case ARecord a:
                    bytes.AddRange(this.ToByteArray(a));
                    break;
                case CNameRecord c:
                    bytes.AddRange(this.ToByteArray(c));
                    break;
                case NSRecord ns:
                    bytes.AddRange(this.ToByteArray(ns));
                    break;
                default:
                    throw new NotImplementedException($"Serialization for recordType {rr.GetRecordType()} is not yet implemented.");
            }

            return bytes.ToArray();
        }

        private byte[] ToByteArray(ARecord a)
        {
            var addressBytes = a.Address.GetAddressBytes();

            var bytes = new List<byte>();
            bytes.AppendIntAs2Bytes(ARecord.Length);
            bytes.AddRange(addressBytes);
            return bytes.ToArray();
        }

        private byte[] ToByteArray(CNameRecord rr)
        {
            var bytes = new List<byte>();
            var qBytes = new DnsQuestionBinarySerializer().SerializeQName(rr.CName);

            bytes.AppendIntAs2Bytes(qBytes.Count);
            bytes.AddRange(qBytes);

            return bytes.ToArray();
        }

        private byte[] ToByteArray(NSRecord rr)
        {
            var bytes = new List<byte>();
            var qBytes = new DnsQuestionBinarySerializer().SerializeQName(rr.DName);

            bytes.AppendIntAs2Bytes(qBytes.Count);
            bytes.AddRange(qBytes);

            return bytes.ToArray();
        }

        public ResourceRecord FromBytes(byte[] bytes, int start, out int totalBytesRead)
        {
            var name = this.dnsSerializer.ParseQuestionName(bytes, start, out var bytesRead);
            totalBytesRead = bytesRead;

            var recordType = DeserializeRecordType(bytes, start + totalBytesRead);
            totalBytesRead += 2;

            // count the record class bytes.
            totalBytesRead += 2;

            var timeToLive = Read4BytesAsInt(bytes, start + totalBytesRead);
            totalBytesRead += 4;

            var length = DnsQuestionBinarySerializer.Read2BytesAsInt(bytes, start + totalBytesRead);
            totalBytesRead += 2;

            switch (recordType)
            {
                case RecordType.A:
                    var a = new ARecord()
                    {
                        Name = name,
                        TimeToLive = timeToLive,
                        Address = new System.Net.IPAddress(new ReadOnlySpan<byte>(bytes, start + totalBytesRead, 4)),
                    };

                    totalBytesRead += 4;
                    return a;
                case RecordType.CNAME:
                    var c = new CNameRecord()
                    {
                        Name = name,
                        TimeToLive = timeToLive,
                        CName = this.dnsSerializer.ParseQuestionName(bytes, start + totalBytesRead, out var cnameBytesRead),
                    };

                    totalBytesRead += cnameBytesRead;
                    return c;
                case RecordType.NS:
                    var ns = new NSRecord()
                    {
                        Name = name,
                        TimeToLive = timeToLive,
                        DName = this.dnsSerializer.ParseQuestionName(bytes, start + totalBytesRead, out var dnameBytesRead),
                    };
                    totalBytesRead += dnameBytesRead;
                    return ns;
                default:
                    throw new NotImplementedException($"Deserialization of recordType {recordType} is not yet implemented.");
            }
        }

        public static RecordType DeserializeRecordType(byte[] bytes, int start)
        {
            int val = DnsQuestionBinarySerializer.Read2BytesAsInt(bytes, start);
            val--;
            return (RecordType)val;
        }

        private static int Read4BytesAsInt(byte[] bytes, int start)
        {
            if (start < 0 || bytes.Length < start + 4)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            int val = bytes[start + 3];
            val |= (bytes[start + 2]);
            val |= (bytes[start + 1] << 16);
            val |= (bytes[start] << 24);

            return val;
        }
    }
}
