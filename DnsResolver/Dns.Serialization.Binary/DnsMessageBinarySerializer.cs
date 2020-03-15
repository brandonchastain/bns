using Bns.Dns.ResourceRecords;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.Dns.Serialization
{
    public class DnsMessageBinarySerializer : IDnsMsgBinSerializer
    {
        private DnsQuestionBinarySerializer qSerializer;
        private ResourceRecordBinarySerializer rrSerializer;

        public DnsMessageBinarySerializer(ResourceRecordBinarySerializer rrSer, DnsQuestionBinarySerializer qSerializer)
        {
            this.rrSerializer = rrSer ?? throw new ArgumentNullException(nameof(rrSer));
            this.qSerializer = qSerializer ?? throw new ArgumentNullException(nameof(qSerializer));
        }

        public DnsMessage Deserialize(byte[] buffer)
        {
            var result = new DnsMessage();
            result.Header = DeserializeHeader(buffer);
            int start = 12;

            result.Question = this.qSerializer.DeserializeBytes(buffer, start, out var questionBytesRead);
            start += questionBytesRead;

            var rrBytesRead = 0;

            result.Answers = new List<ResourceRecord>();
            for (int i = 0; i < result.Header.AnswerCount; i++)
            {
                var answerRecord = rrSerializer.FromBytes(buffer, start + rrBytesRead, out var rrBytes);
                rrBytesRead += rrBytes;

                if (answerRecord != null)
                {
                    result.AddAnswer(answerRecord);
                }
            }

            result.Authority = new List<ResourceRecord>();
            for (int i = 0; i < result.Header.AuthorityCount; i++)
            {
                var rrSer = new ResourceRecordBinarySerializer(new DnsQuestionBinarySerializer());
                var authRec = rrSer.FromBytes(buffer, start + rrBytesRead, out var rrBytes);
                rrBytesRead += rrBytes;

                if (authRec != null)
                {
                    result.AddAuthority(authRec);
                }
            }

            result.Additional = new List<ResourceRecord>();
            for (int i = 0; i < result.Header.AddtlCount; i++)
            {
                var rrSer = new ResourceRecordBinarySerializer(new DnsQuestionBinarySerializer());
                var addtlRec = rrSer.FromBytes(buffer, start + rrBytesRead, out var rrBytes);
                rrBytesRead += rrBytes;

                if (addtlRec != null)
                {
                    result.AddAuthority(addtlRec);
                }
            }


            // var b = new DnsQuestionSerializer().SerializeQuestion(result.Question);
            // HexPrinter.PrintBufferHex(b, b.Length);

            return result;
        }

        public byte[] Serialize(DnsMessage dnsMessage)
        {
            var header = dnsMessage.Header.ToByteArray();
            var body = this.qSerializer.SerializeQuestion(dnsMessage.Question);

            var resultBytes = new List<byte>(header);
            resultBytes.AddRange(body);

            foreach (var ans in dnsMessage.Answers)
            {
                resultBytes.AddRange(this.rrSerializer.ToByteArray(ans));
            }

            foreach (var auth in dnsMessage.Authority)
            {
                resultBytes.AddRange(this.rrSerializer.ToByteArray(auth));
            }

            foreach (var addtl in dnsMessage.Additional)
            {
                resultBytes.AddRange(this.rrSerializer.ToByteArray(addtl));
            }

            return resultBytes.ToArray();
        }


        public static Header DeserializeHeader(byte[] buffer)
        {
            if (buffer.Length < Header.MaxSizeInBytes)
            {
                throw new Exception("Header is too small");
            }

            var header = new Header();

            header.Id = (ushort)(buffer[0] << 8);
            header.Id |= buffer[1];

            header.IsResponse = (buffer[2] & 0x80) != 0;

            var opcodeNum = (buffer[2] & 0x78) >> 3; //01111000
            header.Opcode = (HeaderOpCode)opcodeNum;

            header.IsAuthoritativeAnswer = (buffer[2] & 0x04) != 0;
            header.IsTruncated = (buffer[2] & 0x02) != 0;
            header.RecursionDesired = (buffer[2] & 0x01) != 0;
            header.RecursionAvailable = (buffer[3] & 0x80) != 0;
            header.Z = (byte)((buffer[3] & 0x70) >> 4);

            var rcodeNum = (buffer[3] & 0x0F);
            header.Rcode = (ResponseCode)rcodeNum;

            header.QueryCount = (ushort)(buffer[4] << 8);
            header.QueryCount |= buffer[5];
            header.AnswerCount = (ushort)(buffer[6] << 8);
            header.AnswerCount |= buffer[7];
            header.AuthorityCount = (ushort)(buffer[8] << 8);
            header.AuthorityCount |= buffer[9];
            header.AddtlCount = (ushort)(buffer[10] << 8);
            header.AddtlCount |= buffer[11];

            return header;
        }

    }
}
