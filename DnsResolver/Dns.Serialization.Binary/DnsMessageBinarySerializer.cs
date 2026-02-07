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

        private DnsMessage result;
        private int start;

        public DnsMessageBinarySerializer(ResourceRecordBinarySerializer rrSer, DnsQuestionBinarySerializer qSerializer)
        {
            this.rrSerializer = rrSer ?? throw new ArgumentNullException(nameof(rrSer));
            this.qSerializer = qSerializer ?? throw new ArgumentNullException(nameof(qSerializer));
        }

        public DnsMessage Deserialize(byte[] buffer)
        {
            start = 0; // Reset position for each new message
            result = new DnsMessage();
            result.Header = DeserializeHeader(buffer);
            result.Question = DeserializeQuestion(buffer);
            result.Answers = DeserializeAnswers(buffer);
            result.Authority = DeserializeAuthorities(buffer);
            result.Additional = DeserializeAddtl(buffer);
            return result;
        }

        private Question DeserializeQuestion(byte[] buffer)
        {
            var question = this.qSerializer.DeserializeBytes(buffer, start, out var questionBytesRead);
            start += questionBytesRead;
            return question;
        }

        private List<ResourceRecord> DeserializeAnswers(byte[] buffer)
        {
            int count = result.Header.AnswerCount;
            return DeserializeResourceRecords(buffer, count);
        }

        private List<ResourceRecord> DeserializeAuthorities(byte[] buffer)
        {
            int count = result.Header.AuthorityCount;
            return DeserializeResourceRecords(buffer, count);
        }

        private List<ResourceRecord> DeserializeAddtl(byte[] buffer)
        {
            int count = result.Header.AddtlCount;
            return DeserializeResourceRecords(buffer, count);
        }

        private List<ResourceRecord> DeserializeResourceRecords(byte[] buffer, int count)
        {
            var rrBytesRead = 0;
            var answers = new List<ResourceRecord>();
            for (int i = 0; i < count; i++)
            {
                int rrStart = start + rrBytesRead;
                var resourceRecord = rrSerializer.FromBytes(buffer, rrStart, out var rrBytes);
                rrBytesRead += rrBytes;

                if (resourceRecord != null)
                {
                    answers.Add(resourceRecord);
                }
            }

            start += rrBytesRead;
            return answers;
        }

        public byte[] Serialize(DnsMessage dnsMessage)
        {
            var header = dnsMessage.Header.ToByteArray();
            var body = this.qSerializer.SerializeQuestion(dnsMessage.Question);

            var resultBytes = new List<byte>(header);
            resultBytes.AddRange(body);

            foreach (var ans in dnsMessage.Answers)
            {
                resultBytes.AddRange(ans.ToByteArray());
            }

            foreach (var auth in dnsMessage.Authority)
            {
                resultBytes.AddRange(auth.ToByteArray());
            }

            foreach (var addtl in dnsMessage.Additional)
            {
                resultBytes.AddRange(addtl.ToByteArray());
            }

            return resultBytes.ToArray();
        }


        public Header DeserializeHeader(byte[] buffer)
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

            start += 12;

            return header;
        }

    }
}
