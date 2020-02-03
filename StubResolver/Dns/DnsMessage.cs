using Bns.StubResolver.Udp.Contracts;
using Bns.StubResolver.Dns.ResourceRecords;
using System.Collections.Generic;

namespace Bns.StubResolver.Dns
{
    public class DnsMessage : IByteSerializable
    {

        public DnsMessage()
        {
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

            var msgNoHeader = new List<byte>(); 
            for (int i = 12; i < buffer.Length; i++)
            {
                msgNoHeader.Add(buffer[i]);
            }

            result.Question = Question.FromBytes(msgNoHeader.ToArray(), out var _);
            
            // var b = new DnsQuestionSerializer().SerializeQuestion(result.Question);
            // HexPrinter.PrintBufferHex(b, b.Length);

            return result;
        }

        public void AddAnswers(List<ResourceRecord> records)
        {
            foreach(var rec in records)
            {
                this.Answers.Add(rec);
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
                all.AddRange(ans.ToByteArray());
            }

            return all.ToArray();
        }
    }
}
