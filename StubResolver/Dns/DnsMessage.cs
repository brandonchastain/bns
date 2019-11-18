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
            return result;
        }

        public override string ToString()
        {
            return this.header.ToString();
        }
    }
}
