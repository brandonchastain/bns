using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    public class DnsMessage
    {
        private Header Header { get; set; }
        private Question Question { get; set; }
        private List<ResourceRecord> Answer { get; set; }
        private List<ResourceRecord> Authority { get; set; }
        private List<ResourceRecord> Additional { get; set; }

        private DnsMessage()
        {

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

            return all.ToArray();
        }
    }
}
