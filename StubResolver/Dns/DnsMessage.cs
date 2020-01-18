using Dns.RecordData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    public class DnsMessage
    {
        private Header Header { get; set; }
        private Question Question { get; set; }
        private List<ResourceRecord> Answers { get; set; }
        private List<ResourceRecord> Authority { get; set; }
        private List<ResourceRecord> Additional { get; set; }

        private DnsMessage()
        {
            this.Answers = new List<ResourceRecord>();
        }

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
            var b = new DnsQuestionSerializer().SerializeQuestion(result.Question);
            HexPrinter.PrintBufferHex(b, b.Length);
            return result;
        }

        public void AddAnswer(ResourceRecord record)
        {
            this.Answers.Add(record);
            this.Header.AnswerCount++;
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
