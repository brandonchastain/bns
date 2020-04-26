using Bns.Dns.ResourceRecords;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.Dns.Serialization
{
    public class ResourceRecordBinarySerializer
    {
        private readonly DnsQuestionBinarySerializer dnsSerializer;

        public ResourceRecordBinarySerializer(DnsQuestionBinarySerializer dnsSerializer)
        {
            this.dnsSerializer = dnsSerializer;
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

            ResourceRecord result = null;

            switch (recordType)
            {
                case RecordType.A:
                    result = new ARecord()
                    {
                        Name = name,
                        TimeToLive = timeToLive,
                        Address = new System.Net.IPAddress(new ReadOnlySpan<byte>(bytes, start + totalBytesRead, 4)),
                    };
                    break;
                case RecordType.CNAME:
                    result = new CNameRecord()
                    {
                        Name = name,
                        TimeToLive = timeToLive,
                        CName = this.dnsSerializer.ParseQuestionName(bytes, start + totalBytesRead, out var cnameBytesRead),
                    };
                    break;
                case RecordType.NS:
                    result = new NSRecord()
                    {
                        Name = name,
                        TimeToLive = timeToLive,
                        DName = this.dnsSerializer.ParseQuestionName(bytes, start + totalBytesRead, out var dnameBytesRead),
                    };
                    break;
                case RecordType.MX:
                    result = this.DeserializeMxRr(name, timeToLive, bytes, start + totalBytesRead, out var rdataSize);
                    break;
                case RecordType.PTR:
                    result = this.DeserializePtrRr(name, timeToLive, bytes, start + totalBytesRead, out var ptrSize);
                    break;
                case RecordType.TXT:
                    result = this.DeserializeTxtRr(name, timeToLive, bytes, start + totalBytesRead, length, out var txtSize);
                    break;
                case RecordType.SOA:
                    result = this.DeserializeSoaRr(name, timeToLive, bytes, start + totalBytesRead, out var soaSize);
                    break;
                default:
                    Console.WriteLine($"Deserialization of recordType {recordType} is not yet implemented.");
                    result = null;
                    break;
            }

            totalBytesRead += length;
            return result;
        }

        private MXRecord DeserializeMxRr(string name, int ttl, byte[] bytes, int start, out int bytesRead)
        {
            var mx = new MXRecord()
            {
                Name = name,
                TimeToLive = ttl,
            };

            mx.ExchangeDName = this.dnsSerializer.ParseQuestionName(bytes, start, out bytesRead);
            bytesRead += 2; // preference

            return mx;
        }


        private PTRRecord DeserializePtrRr(string name, int ttl, byte[] bytes, int start, out int bytesRead)
        {
            var ptr = new PTRRecord()
            {
                Name = name,
                TimeToLive = ttl,
            };

            ptr.PtrDName = this.dnsSerializer.ParseQuestionName(bytes, start, out bytesRead);

            return ptr;
        }

        private TXTRecord DeserializeTxtRr(string name, int ttl, byte[] bytes, int start, int length, out int bytesRead)
        {
            var txt = new TXTRecord()
            {
                Name = name,
                TimeToLive = ttl,
                Length = length
            };

            var wordBytes = new byte[length];
            Array.Copy(bytes, start, wordBytes, 0, length);
            txt.TextData = Encoding.ASCII.GetString(wordBytes);

            bytesRead = length;

            return txt;
        }

        private SOARecord DeserializeSoaRr(string name, int ttl, byte[] bytes, int start, out int bytesRead)
        {
            var soa = new SOARecord()
            {
                Name = name,
                TimeToLive = ttl,
            };

            soa.MName = this.dnsSerializer.ParseQuestionName(bytes, start, out bytesRead);

            soa.RName = this.dnsSerializer.ParseQuestionName(bytes, start + bytesRead, out var rNameBytesRead);
            bytesRead += rNameBytesRead;

            soa.Serial = (uint)Read4BytesAsInt(bytes, bytesRead);
            soa.RefreshInterval = Read4BytesAsInt(bytes, bytesRead + 4);
            soa.RetryInterval = Read4BytesAsInt(bytes, bytesRead + 8);
            soa.ExpireInterval = Read4BytesAsInt(bytes, bytesRead + 12);
            soa.Minimum = (uint)Read4BytesAsInt(bytes, bytesRead + 16);

            bytesRead += 20;

            return soa;
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
