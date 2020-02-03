using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;
using System.Text;
using Bns.StubResolver.Dns.Serialization;

namespace Bns.StubResolver.Dns
{
    public class Header
    {
        private const int sizeInBytes = 12;

        private IJsonSerializer jsonSerializer;

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

        public Header()
        {
            this.jsonSerializer = new DnsJsonSerializer();
        }

        public static Header Parse(byte[] buffer)
        {
            if (buffer.Length < sizeInBytes)
            {
                throw new Exception("Header is too small");
            }

            var header = new Header();

            header.Id = (ushort)(buffer[0] << 8);
            header.Id |= buffer[1];

            header.IsResponse = (buffer[2] & 0x80) != 0;

            var opcodeNum = (buffer[2] & 0x78) >> 3; //01111000
            header.Opcode = (HeaderOpCode)opcodeNum;

            header.IsAuthoritativeAnswer = (buffer[2] & 0x04) != 0;
            header.IsTruncated = (buffer[2] & 0x02) != 0;
            header.RecursionDesired = (buffer[2] & 0x01) != 0;
            header.RecursionAvailable = (buffer[3] & 0x80) != 0;
            header.Z = (byte)((buffer[3] & 0x70) >> 4);

            var rcodeNum = (buffer[3] & 0x0F);
            header.Rcode = (ResponseCode)rcodeNum;

            header.QueryCount = (ushort)(buffer[4] << 8);
            header.QueryCount |= buffer[5];
            header.AnswerCount = (ushort)(buffer[6] << 8);
            header.AnswerCount |= buffer[7];
            header.AuthorityCount = (ushort)(buffer[8] << 8);
            header.AuthorityCount |= buffer[9];
            header.AddtlCount = (ushort)(buffer[10] << 8);
            header.AddtlCount |= buffer[11];

            return header;
        }

        public byte[] ToByteArray()
        {
            var buffer = new byte[sizeInBytes];

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
