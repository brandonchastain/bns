using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    public class DnsQuestionSerializer
    {
        public byte[] SerializeQuestion(Question q)
        {
            if (q == null)
            {
                return null;
            }

            var wordBytes = new List<byte>();
            int wordStart = 0;

            int wordEnd = q.QName.IndexOf('.', wordStart);

            int wordSize = wordEnd - wordStart;
            while (wordSize > 0)
            {
                wordBytes.Add((byte)wordSize);
                for (int c = 0; c < wordSize; c++)
                {
                    wordBytes.Add((byte)(q.QName[wordStart + c]));
                }

                wordStart = wordEnd + 1;
                wordEnd = q.QName.IndexOf('.', wordStart); 
                wordSize = wordEnd - wordStart;
            }

            wordBytes.Add(0);

            var qTypeToWrite = (int)(q.QType + 1);
            wordBytes.Add((byte)(qTypeToWrite >> 8));
            wordBytes.Add((byte)qTypeToWrite);

            var qClassToWrite = (int)(q.QClass + 1);
            wordBytes.Add((byte)(qClassToWrite >> 8));
            wordBytes.Add((byte)qClassToWrite);

            return wordBytes.ToArray();
        }

        public Question DeserializeBytes(byte[] buffer, out int bytesRead)
        {
            buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

            Question q = new Question();
            q.QName = ParseQuestionName(buffer, out int qNameBytesRead);
            q.QType = ParseQueryType(buffer, qNameBytesRead);
            q.QClass = ParseQueryClass(buffer, qNameBytesRead + 2);
            bytesRead = qNameBytesRead + 4;
            return q;
        }

        public string ParseQuestionName(byte[] buffer, out int bytesRead)
        {
            var sb = new StringBuilder();
            int wordSizeIdx = 0;
            int wordStart = wordSizeIdx + 1;
            var wordSize = (int)buffer[wordSizeIdx];
            bytesRead = 0;

            while (wordSize > 0)
            {
                var wordBytes = new byte[wordSize];
                Array.Copy(buffer, wordStart, wordBytes, 0, wordSize);
                sb.Append($"{Encoding.ASCII.GetString(wordBytes)}.");

                bytesRead += wordSize + 1;
                wordSizeIdx += (wordSize + 1);
                wordSize = (int)buffer[wordSizeIdx];
                wordStart = wordSizeIdx + 1;
            }

            bytesRead += 1;

            return sb.ToString();
        }

        private RecordType ParseQueryType(byte[] buffer, int index)
        {
            int result = buffer[index + 1];
            result |= (buffer[index] << 8);
            result -= 1;
            return (RecordType)result;
        }

        private RecordClass ParseQueryClass(byte[] buffer, int index)
        {
            int result = buffer[index + 1];
            result |= (buffer[index] << 8);
            result -= 1;
            return (RecordClass)result;
        }
    }
}
