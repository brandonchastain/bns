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

            // Each serialized RR should begin with 2 bytes containing the length of the rdata.
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
                case MXRecord mx:
                    bytes.AddRange(this.ToByteArray(mx));
                    break;
                case PTRRecord ptr:
                    bytes.AddRange(this.ToByteArray(ptr));
                    break;
                case TXTRecord txt:
                    bytes.AddRange(this.ToByteArray(txt));
                    break;
                case SOARecord soa:
                    bytes.AddRange(this.ToByteArray(soa));
                    break;
                default:
                    throw new NotImplementedException($"Serialization for recordType {rr.GetRecordType()} is not yet implemented.");
            }

            return bytes.ToArray();
        }

        private byte[] ToByteArray(MXRecord mx)
        {
            var bytes = new List<byte>();
            var dnameBytes = this.dnsSerializer.SerializeQName(mx.ExchangeDName);
            var numBytes = dnameBytes.Count + 2;
            bytes.AppendIntAs2Bytes(numBytes);
            bytes.AppendIntAs2Bytes(mx.Preference);
            bytes.AddRange(dnameBytes);

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
            var qBytes = this.dnsSerializer.SerializeQName(rr.CName);

            bytes.AppendIntAs2Bytes(qBytes.Count);
            bytes.AddRange(qBytes);

            return bytes.ToArray();
        }

        private byte[] ToByteArray(NSRecord rr)
        {
            var bytes = new List<byte>();
            var qBytes = this.dnsSerializer.SerializeQName(rr.DName);

            bytes.AppendIntAs2Bytes(qBytes.Count);
            bytes.AddRange(qBytes);

            return bytes.ToArray();
        }

        private byte[] ToByteArray(PTRRecord rr)
        {
            var bytes = new List<byte>();
            var qnameBytes = this.dnsSerializer.SerializeQName(rr.PtrDName);
            var numBytes = qnameBytes.Count;
            bytes.AppendIntAs2Bytes(numBytes);
            bytes.AddRange(qnameBytes);
            return bytes.ToArray();
        }

        private byte[] ToByteArray(TXTRecord rr)
        {
            var bytes = new List<byte>();
            var textDataBytes = Encoding.ASCII.GetBytes(rr.TextData);

            if (rr.Length != textDataBytes.Length)
            {
                Console.WriteLine("TXT record length mismatch detected during serialization.");
            }

            bytes.AppendIntAs2Bytes(rr.Length);
            bytes.AddRange(textDataBytes);

            return bytes.ToArray();
        }

        private byte[] ToByteArray(SOARecord rr)
        {
            var bytes = new List<byte>();
            var mnameBytes = this.dnsSerializer.SerializeQName(rr.MName);
            var rnameBytes = this.dnsSerializer.SerializeQName(rr.RName);
            
            var length = mnameBytes.Count + rnameBytes.Count + 20;
            bytes.AppendIntAs2Bytes(length);

            bytes.AddRange(mnameBytes);
            bytes.AddRange(rnameBytes);
            bytes.AppendIntAs4Bytes((int)rr.Serial);
            bytes.AppendIntAs4Bytes(rr.RefreshInterval);
            bytes.AppendIntAs4Bytes(rr.RetryInterval);
            bytes.AppendIntAs4Bytes(rr.ExpireInterval);
            bytes.AppendIntAs4Bytes((int)rr.Minimum);

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
