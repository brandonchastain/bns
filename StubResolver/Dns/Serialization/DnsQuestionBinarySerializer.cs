using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.StubResolver.Dns.Serialization
{
    public class DnsQuestionBinarySerializer
    {
        public List<byte> SerializeQName(string qname)
        {
            var wordBytes = new List<byte>();
            int wordStart = 0;

            int wordEnd = qname.IndexOf('.', wordStart);

            int wordSize = wordEnd - wordStart;
            while (wordSize > 0)
            {
                wordBytes.Add((byte)wordSize);
                for (int c = 0; c < wordSize; c++)
                {
                    wordBytes.Add((byte)(qname[wordStart + c]));
                }

                wordStart = wordEnd + 1;
                wordEnd = qname.IndexOf('.', wordStart);
                wordSize = wordEnd - wordStart;
            }

            wordBytes.Add(0);

            return wordBytes;
        }

        public byte[] SerializeQuestion(Question q)
        {
            if (q == null)
            {
                return null;
            }

            var wordBytes = SerializeQName(q.QName);
            var qTypeToWrite = (int)(q.QType + 1);
            wordBytes.Add((byte)(qTypeToWrite >> 8));
            wordBytes.Add((byte)qTypeToWrite);

            var qClassToWrite = (int)(q.QClass + 1);
            wordBytes.Add((byte)(qClassToWrite >> 8));
            wordBytes.Add((byte)qClassToWrite);

            return wordBytes.ToArray();
        }

        public Question DeserializeBytes(byte[] buffer, int start, out int bytesRead)
        {
            buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

            Question q = new Question();
            q.QName = ParseQuestionName(buffer, start, out int qNameBytesRead);
            bytesRead = qNameBytesRead;

            q.QType = ParseQueryType(buffer, start + qNameBytesRead);
            bytesRead += 2;

            q.QClass = ParseQueryClass(buffer, start + qNameBytesRead + 2);
            bytesRead += 2;

            return q;
        }

        public string ParseQuestionName(byte[] buffer, int start, out int bytesRead)
        {
            // TODO: Improve out of bounds handling
            var sb = new StringBuilder();
            int wordSizeIdx = start;
            int wordStart = wordSizeIdx + 1;
            var wordSize = (int)buffer[wordSizeIdx];
            bool isPointer = (wordSize & 0xc0) != 0; // a pointer to another label will have the first two bits set.
            bytesRead = 0;

            while (wordSize > 0 && !isPointer)
            {
                var wordBytes = new byte[wordSize];
                Array.Copy(buffer, wordStart, wordBytes, 0, wordSize);
                sb.Append($"{Encoding.ASCII.GetString(wordBytes)}.");

                bytesRead += wordSize + 1;
                wordSizeIdx += (wordSize + 1);
                wordSize = (int)buffer[wordSizeIdx];
                wordStart = wordSizeIdx + 1;

                isPointer = (wordSize & (0xc0)) != 0;
            }


            if (isPointer)
            {
                var pointer = Read2BytesAsInt(buffer, wordSizeIdx);
                pointer &= ~(0xc0 << 8);
                sb.Append(this.ParseQuestionName(buffer, pointer, out _));

                // count the 2 bytes
                bytesRead += 2;
            }
            else
            {
                // count the final null byte as a byte that was read.
                bytesRead += 1;
            }

            return sb.ToString();
        }

        public static int Read2BytesAsInt(byte[] bytes, int start)
        {
            if (start < 0 || bytes.Length < start + 2)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            int val = bytes[start + 1];
            val |= (bytes[start] << 8);

            return val;
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
