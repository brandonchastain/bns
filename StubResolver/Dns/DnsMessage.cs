using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    public class DnsMessage
    {
        Header header;
        Question question;
        List<ResourceRecord> answer;
        List<ResourceRecord> authority;
        List<ResourceRecord> additional;

        private DnsMessage()
        {

        }

        public static DnsMessage Parse(byte[] buffer)
        {
            var result = new DnsMessage();
            result.header = Header.Parse(buffer);

            var msgNoHeader = new List<byte>(); 
            for (int i = 12; i < buffer.Length; i++)
            {
                msgNoHeader.Add(buffer[i]);
            }

            result.question = Question.Parse(msgNoHeader.ToArray());
            return result;
        }

        public override string ToString()
        {
            return this.header.ToString();
        }

        public byte[] ToByteArray()
        {
            return this.header.ToByteArray();
        }
    }
}
