using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    class Question
    {
        string qname; // max length 255 octets
        RecordType qtype;
        RecordClass qclass;

        private Question()
        {

        }

        public static Question Parse(byte[] buffer)
        {
            buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

            Question q = new Question();
            q.qname = ParseQuestionName(buffer, out int bytesRead);

            int qtypeNum = (buffer[bytesRead] << 8);
            qtypeNum |= buffer[bytesRead + 1];
            q.qtype = (RecordType)qtypeNum;

            int qclassNum = (buffer[bytesRead + 2] << 8);
            qclassNum |= buffer[bytesRead + 3];
            q.qclass = (RecordClass)qclassNum;

            return q;
        }

        private static string ParseQuestionName(byte[] buffer, out int bytesRead)
        {
            var sb = new StringBuilder();

            int i = 0;
            int labelSize = buffer[i];

            while (labelSize != 0)
            {
                int start = i + 1;
                var chars = new List<char>();
                for (int j = 0; j < labelSize; j++)
                {
                    sb.Append((char)buffer[start + j]);
                    Console.WriteLine(sb.ToString());
                }

                sb.Append('.');

                i = (i + labelSize);
                labelSize = buffer[i + 1];
            }

            bytesRead = i;
            return sb.ToString();
        }
    }
}
