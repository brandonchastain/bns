using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    public class Question
    {
        private const int MaxQnameSize = 255; // rfc 1035

        private static DnsQuestionSerializer serializer = new DnsQuestionSerializer();

        public String QName { get; set; }
        public RecordType QType { get; set; }
        public RecordClass QClass { get; set; }

        public Question() { }

        public static Question FromBytes(byte[] buffer, out int bytesRead)
        {
            var q = serializer.DeserializeBytes(buffer, out int questionBytesRead);
            bytesRead = questionBytesRead;
            return q;
        }

        public override string ToString()
        {
            return this.QName + "\n" +
                   this.QType.ToString() + "\n" +
                   this.QClass.ToString();
        }

        public byte[] ToByteArray()
        {
            var buffer = serializer.SerializeQuestion(this);
            return buffer;
        }
    }
}
