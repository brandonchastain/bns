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
            AppendIntAs2Bytes(rrTypeNum, bytes);

            var rrClassNum = ((int)rr.GetRecordClass()) + 1;
            AppendIntAs2Bytes(rrClassNum, bytes);

            AppendIntAs4Bytes(rr.TimeToLive, bytes);

            if (rr is ARecord a)
            {
                bytes.AddRange(this.ToByteArray(a));
            }
            else if (rr is CNameRecord c)
            {
                bytes.AddRange(this.ToByteArray(c));
            }
            else
            {
                throw new NotImplementedException($"Serialization for recordType {rr.GetRecordType()} is not yet implemented.");
            }

            return bytes.ToArray();
        }

        private byte[] ToByteArray(ARecord a)
        {
            var addressBytes = a.Address.GetAddressBytes();

            var bytes = new List<byte>();
            AppendIntAs2Bytes(ARecord.Length, bytes);
            bytes.AddRange(addressBytes);
            return bytes.ToArray();
        }

        private byte[] ToByteArray(CNameRecord rr)
        {
            var bytes = new List<byte>();
            var qBytes = new DnsQuestionBinarySerializer().SerializeQName(rr.CName);

            bytes.Add((byte)(qBytes.Count >> 8));
            bytes.Add((byte)qBytes.Count);
            bytes.AddRange(qBytes);

            return bytes.ToArray();
        }

        public ResourceRecord FromBytes(byte[] bytes, int start, out int totalBytesRead)
        {
            var name = this.dnsSerializer.ParseQuestionName(bytes, start, out var bytesRead);
            totalBytesRead = bytesRead;

            var recordType = DeserializeRecordType(bytes, start + totalBytesRead);
            totalBytesRead += 2;

            var timeToLive = Read4BytesAsInt(bytes, start + totalBytesRead);
            totalBytesRead += 4;

            var length = Read2BytesAsInt(bytes, start + totalBytesRead);
            totalBytesRead += 2;

            switch (recordType)
            {
                case RecordType.A:
                    var a = new ARecord()
                    {
                        Name = name,
                        TimeToLive = timeToLive,
                        Address = new System.Net.IPAddress(new ReadOnlySpan<byte>(bytes, bytesRead, 4)),
                    };

                    totalBytesRead += 4;
                    return a;
                case RecordType.CNAME:
                    var c = new CNameRecord()
                    {
                        Name = name,
                        TimeToLive = timeToLive,
                        CName = this.dnsSerializer.ParseQuestionName(bytes, bytesRead, out var cnameBytesRead),
                    };

                    totalBytesRead += cnameBytesRead;
                    return c;
                default:
                    throw new NotImplementedException($"Deserialization of recordType {recordType} is not yet implemented.");
            }
        }

        public static RecordType DeserializeRecordType(byte[] bytes, int start)
        {
            int val = Read2BytesAsInt(bytes, start);
            val--;
            return (RecordType)val;
        }

        private static void AppendIntAs2Bytes(int val, IList<byte> bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            bytes.Add((byte)(val >> 8));
            bytes.Add((byte)val);
        }

        private static void AppendIntAs4Bytes(int val, IList<byte> bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            bytes.Add((byte)(val >> 24));
            bytes.Add((byte)(val >> 16));
            bytes.Add((byte)(val >> 8));
            bytes.Add((byte)val);
        }

        private static int Read2BytesAsInt(byte[] bytes, int start)
        {
            if (start < 0 || bytes.Length < start + 2)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            int val = bytes[start + 1];
            val |= (bytes[start] << 8);

            return val;
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
