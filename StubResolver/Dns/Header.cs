using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    class Header
    {
        private const int sizeInBytes = 12;

        ushort id;
        bool isResponse;
        HeaderOpCode opcode; // last 4 bits only
        bool isAuthoritativeAnswer;
        bool isTruncated;
        bool recursionDesired;
        bool recursionAvailable;
        byte z; // always 0
        ResponseCode rcode;
        ushort queryCount;
        ushort answerCount;
        ushort authorityCount;
        ushort addtlCount;

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

            var opcodeNum = (buffer[2] & 0x78) >> 3;
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
