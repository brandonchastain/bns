using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    class Header
    {
        private const int sizeInBytes = 12;

        private ushort id;
        private bool isResponse;
        private HeaderOpCode opcode; // last 4 bits only
        private bool isAuthoritativeAnswer;
        private bool isTruncated;
        private bool recursionDesired;
        private bool recursionAvailable;
        private byte z;
        private ResponseCode rcode;
        private ushort queryCount;
        private ushort answerCount;
        private ushort authorityCount;
        private ushort addtlCount;

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

            header.id = (ushort)(buffer[0] << 8);
            header.id |= buffer[1];

            header.isResponse = (buffer[2] & 0x80) != 0;

            var opcodeNum = (buffer[2] & 0x78) >> 3; //01111000
            header.opcode = (HeaderOpCode)opcodeNum;

            header.isAuthoritativeAnswer = (buffer[2] & 0x04) != 0;
            header.isTruncated = (buffer[2] & 0x02) != 0;
            header.recursionDesired = (buffer[2] & 0x01) != 0;
            header.recursionAvailable = (buffer[3] & 0x80) != 0;
            header.z = (byte)((buffer[3] & 0x70) >> 4);

            var rcodeNum = (buffer[3] & 0x0F);
            header.rcode = (ResponseCode)rcodeNum;

            header.queryCount = (ushort)(buffer[4] << 8);
            header.queryCount |= buffer[5];
            header.answerCount = (ushort)(buffer[6] << 8);
            header.answerCount |= buffer[7];
            header.authorityCount = (ushort)(buffer[8] << 8);
            header.authorityCount |= buffer[9];
            header.addtlCount = (ushort)(buffer[10] << 8);
            header.addtlCount |= buffer[11];

            return header;
        }

        public byte[] ToByteArray()
        {
            var buffer = new byte[sizeInBytes];

            buffer[0] = (byte)(this.id >> 8);
            buffer[1] = (byte)this.id;

            // Set is response flag to 1
            buffer[2] |= 0x80;

            var opcodeNum = (ushort)this.opcode;
            buffer[2] |= (byte)(opcodeNum << 3);

            // set aa flag to 0 since this is a recursive answer
            buffer[2] &= 0xfb; //11111011

            // set isTruncated flag to 0
            buffer[2] &= 0xfd; //11111101

            // set rd flag
            if (this.recursionDesired)
            {
                buffer[2] &= 0xfe;
            }

            // set recursionAvailable flag to 1
            buffer[3] |= 0x80;

            return buffer;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Header:");
            sb.AppendLine($"[id] : {this.id}");
            sb.AppendLine($"[isResponse] : {this.isResponse}");
            sb.AppendLine($"[isAuthoritative] : {this.isAuthoritativeAnswer}");
            sb.AppendLine($"[isTruncated] : {this.isTruncated}");
            sb.AppendLine($"[recursionDesired] : {this.recursionDesired}");
            sb.AppendLine($"[recursionAvailable] : {this.recursionAvailable}");
            sb.AppendLine($"[z] : {this.z}");
            sb.AppendLine($"[rcode] : {this.rcode}");
            sb.AppendLine($"[queryCount] : {this.queryCount}");
            sb.AppendLine($"[answerCount] : {this.answerCount}");
            sb.AppendLine($"[authorityCount] : {this.authorityCount}");
            sb.AppendLine($"[addtlCount] : {this.addtlCount}");
            return sb.ToString();
        }
    }
}
