using Bns.StubResolver.Dns.Serialization;
using System;
namespace Bns.StubResolver.Dns
{
    public class Question
    {
        private const int MaxQnameSize = 255; // rfc 1035

        private static DnsQuestionBinarySerializer serializer = new DnsQuestionBinarySerializer();
        private IJsonSerializer jsonSerializer;

        public String QName { get; set; }
        public RecordType QType { get; set; }
        public RecordClass QClass { get; set; }

        public Question()
        {
            jsonSerializer = new DnsJsonSerializer();
        }

        public static Question FromBytes(byte[] buffer, out int bytesRead)
        {
            var q = serializer.DeserializeBytes(buffer, out int questionBytesRead);
            bytesRead = questionBytesRead;
            return q;
        }

        public byte[] ToByteArray()
        {
            var buffer = serializer.SerializeQuestion(this);
            return buffer;
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
