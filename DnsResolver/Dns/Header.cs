using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;
using System.Text;
using Bns.Dns.Serialization;

namespace Bns.Dns
{
    public class Header
    {
        public const int MaxSizeInBytes = 12;

        private IJsonSerializer jsonSerializer;

        public Header()
        {
            this.jsonSerializer = new DnsJsonSerializer();
        }

        public ushort Id { get; set; }
        public bool IsResponse { get; set; }
        public HeaderOpCode Opcode { get; set; } // last 4 bits only
        public bool IsAuthoritativeAnswer { get; set; }
        public bool IsTruncated { get; set; }
        public bool RecursionDesired { get; set; }
        public bool RecursionAvailable { get; set; }
        public byte Z { get; set; }
        public ResponseCode Rcode { get; set; }
        public ushort QueryCount { get; set; }
        public ushort AnswerCount { get; set; }
        public ushort AuthorityCount { get; set; }
        public ushort AddtlCount { get; set; }


        public byte[] ToByteArray()
        {
            var buffer = new byte[MaxSizeInBytes];

            buffer[0] = (byte)(this.Id >> 8);
            buffer[1] = (byte)this.Id;

            if (this.IsResponse)
            {
                // Set is response flag to 1
                buffer[2] |= 0x80;
            }

            var opcodeNum = (ushort)this.Opcode;
            buffer[2] |= (byte)(opcodeNum << 3);

            // set aa flag to 0 since this is a recursive answer
            buffer[2] &= 0xfb; //11111011

            // set isTruncated flag to 0
            buffer[2] &= 0xfd; //11111101

            // set rd flag
            if (this.RecursionDesired)
            {
                buffer[2] |= 0x01;
            }

            if (this.RecursionAvailable)
            {
                // set recursionAvailable flag to 1
                buffer[3] |= 0x80;
            }

            // persist Z
            buffer[3] |= (byte)(this.Z << 4);

            // RCode
            var rcodeNum = (byte)this.Rcode;
            buffer[3] |= rcodeNum;

            // Counts of remaining sections
            buffer[4] = (byte)(this.QueryCount >> 8);
            buffer[5] = (byte)this.QueryCount;
            buffer[6] = (byte)(this.AnswerCount >> 8);
            buffer[7] = (byte)this.AnswerCount;
            buffer[8] = (byte)(this.AuthorityCount >> 8);
            buffer[9] = (byte)this.AuthorityCount;
            buffer[10] = (byte)(this.AddtlCount >> 8);
            buffer[11] = (byte)this.AddtlCount;

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
