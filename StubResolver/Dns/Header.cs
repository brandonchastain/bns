using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    class Header
    {
        private const int sizeInBytes = 12;

        private ushort Id { get; set; }
        private bool IsResponse { get; set; }
        private HeaderOpCode Opcode { get; set; } // last 4 bits only
        private bool IsAuthoritativeAnswer { get; set; }
        private bool IsTruncated { get; set; }
        private bool RecursionDesired { get; set; }
        private bool RecursionAvailable { get; set; }
        private byte Z { get; set; }
        private ResponseCode Rcode { get; set; }
        private ushort QueryCount { get; set; }
        private ushort AnswerCount { get; set; }
        private ushort AuthorityCount { get; set; }
        private ushort AddtlCount { get; set; }

        private Header()
        {

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

            // Set is response flag to 1
            buffer[2] |= 0x80;

            var opcodeNum = (ushort)this.Opcode;
            buffer[2] |= (byte)(opcodeNum << 3);

            // set aa flag to 0 since this is a recursive answer
            buffer[2] &= 0xfb; //11111011

            // set isTruncated flag to 0
            buffer[2] &= 0xfd; //11111101

            // set rd flag
            if (this.RecursionDesired)
            {
                buffer[2] &= 0xfe;
            }

            // set recursionAvailable flag to 1
            buffer[3] |= 0x80;

            // persist Z
            buffer[3] |= (byte)(this.Z << 4);

            // RCode
            var rcodeNum = (byte)this.Rcode;
            buffer[3] |= rcodeNum;

            this.QueryCount = 1;
            this.AnswerCount = 1;

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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Header:");
            sb.AppendLine($"[id] : {this.Id}");
            sb.AppendLine($"[isResponse] : {this.IsResponse}");
            sb.AppendLine($"[isAuthoritative] : {this.IsAuthoritativeAnswer}");
            sb.AppendLine($"[isTruncated] : {this.IsTruncated}");
            sb.AppendLine($"[recursionDesired] : {this.RecursionDesired}");
            sb.AppendLine($"[recursionAvailable] : {this.RecursionAvailable}");
            sb.AppendLine($"[z] : {this.Z}");
            sb.AppendLine($"[rcode] : {this.Rcode}");
            sb.AppendLine($"[queryCount] : {this.QueryCount}");
            sb.AppendLine($"[answerCount] : {this.AnswerCount}");
            sb.AppendLine($"[authorityCount] : {this.AuthorityCount}");
            sb.AppendLine($"[addtlCount] : {this.AddtlCount}");
            return sb.ToString();
        }
    }
}
