using Bns.Dns.Serialization;
using System;

namespace Bns.Dns
{
    public class Question
    {
        private const int MaxQnameSize = 255; // rfc 1035
        private IJsonSerializer jsonSerializer;

        public String QName { get; set; }
        public RecordType QType { get; set; }
        public RecordClass QClass { get; set; }

        public Question()
        {
            jsonSerializer = new DnsJsonSerializer();
        }

        public string ToJson()
        {
            return this.jsonSerializer.ToJson(this);
        }

        public override string ToString()
        {
            return this.jsonSerializer.PrettyPrint(this.ToJson());
        }

    }
}
