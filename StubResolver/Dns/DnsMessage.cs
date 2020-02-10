using Bns.StubResolver.Udp.Contracts;
using Bns.StubResolver.Dns.ResourceRecords;
using System.Collections.Generic;
using Bns.StubResolver.Dns.Serialization;

namespace Bns.StubResolver.Dns
{
    public class DnsMessage : IByteSerializable
    {
        private DnsQuestionBinarySerializer dnsQSerializer;
        private ResourceRecordBinarySerializer rrSerializer;

        public DnsMessage()
        {
            dnsQSerializer = new DnsQuestionBinarySerializer();
            rrSerializer = new ResourceRecordBinarySerializer(dnsQSerializer);
            this.Answers = new List<ResourceRecord>();
            this.Authority = new List<ResourceRecord>();
            this.Additional = new List<ResourceRecord>();
        }

        public Header Header { get; set; }
        public Question Question { get; set; }
        public List<ResourceRecord> Answers { get; set; }
        private List<ResourceRecord> Authority { get; set; }
        private List<ResourceRecord> Additional { get; set; }

        public static DnsMessage Parse(byte[] buffer)
        {
            var result = new DnsMessage();
            result.Header = Header.Parse(buffer);
            int start = 12;

            result.Question = Question.FromBytes(buffer, start, out var questionBytesRead);
            start += questionBytesRead;

            result.Answers = new List<ResourceRecord>();

            var rrBytesRead = 0;

            for (int i = 0; i < result.Header.AnswerCount; i++)
            {
                var rrSer = new ResourceRecordBinarySerializer(new DnsQuestionBinarySerializer());
                var answerRecord = rrSer.FromBytes(buffer, start + rrBytesRead, out var rrBytes);
                rrBytesRead += rrBytes;

                result.AddAnswer(answerRecord);
            }


            // TODO: read the rest of the answers.

            // var b = new DnsQuestionSerializer().SerializeQuestion(result.Question);
            // HexPrinter.PrintBufferHex(b, b.Length);

            return result;
        }

        public void AddAnswer(ResourceRecord rec)
        {
            this.Answers.Add(rec);
        }

        public void AddAnswersAndIncrementCount(List<ResourceRecord> records)
        {
            foreach(var rec in records)
            {
                this.AddAnswer(rec);
                this.Header.AnswerCount++;
            }
        }

        public override string ToString()
        {
            return this.Header.ToString() + "\n" + this.Question.ToString();
        }

        public byte[] ToByteArray()
        {
            var header = this.Header.ToByteArray();
            var body = this.Question.ToByteArray();
            var all = new List<byte>(header);
            all.AddRange(body);

            foreach (var ans in this.Answers)
            {
                all.AddRange(this.rrSerializer.ToByteArray(ans));
            }

            return all.ToArray();
        }
    }
}
