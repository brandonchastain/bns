using Bns.Dns.ResourceRecords;
using Dns.Serialization.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bns.Dns.Serialization
{
    public class DnsQuestionBinarySerializer
    {

        public byte[] SerializeQuestion(Question q)
        {
            if (q == null)
            {
                return null;
            }

            var wordBytes = QNameSerializer.SerializeQName(q.QName);

            wordBytes.AppendIntAs2Bytes((int)q.QType);
            wordBytes.AppendIntAs2Bytes((int)q.QClass);

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
            var sb = new StringBuilder();
            int wordSizeIdx = start;
            bytesRead = 0;

            // Bounds check for initial position
            if (wordSizeIdx >= buffer.Length)
            {
                throw new ArgumentException($"Start position {start} is beyond buffer length {buffer.Length}", nameof(start));
            }

            int wordStart = wordSizeIdx + 1;
            var wordSize = (int)buffer[wordSizeIdx];
            bool isPointer = (wordSize & 0xc0) != 0; // a pointer to another label will have the first two bits set.

            // Prevent infinite loops with a maximum label count
            int maxLabels = 255;
            int labelCount = 0;

            while (wordSize > 0 && !isPointer)
            {
                // Check for too many labels (prevent infinite loops)
                if (++labelCount > maxLabels)
                {
                    throw new InvalidOperationException("DNS name has too many labels (possible infinite loop or malformed packet)");
                }

                // Validate label length (DNS labels must be 1-63 bytes)
                if (wordSize > 63)
                {
                    throw new InvalidOperationException($"Invalid DNS label length: {wordSize}. Labels must be 1-63 bytes.");
                }

                // Check if we can read the entire label
                if (wordStart + wordSize > buffer.Length)
                {
                    throw new ArgumentException($"Label extends beyond buffer: need {wordSize} bytes at position {wordStart}, but buffer ends at {buffer.Length}");
                }

                var wordBytes = new byte[wordSize];
                Array.Copy(buffer, wordStart, wordBytes, 0, wordSize);
                sb.Append($"{Encoding.ASCII.GetString(wordBytes)}.");

                bytesRead += wordSize + 1;
                wordSizeIdx += (wordSize + 1);

                // Check if we can read the next label size byte
                if (wordSizeIdx >= buffer.Length)
                {
                    throw new ArgumentException($"Missing null terminator or next label: position {wordSizeIdx} is beyond buffer length {buffer.Length}");
                }

                wordSize = (int)buffer[wordSizeIdx];
                wordStart = wordSizeIdx + 1;

                isPointer = (wordSize & (0xc0)) != 0;
            }


            if (isPointer)
            {
                // Check if we can read the 2-byte pointer
                if (wordSizeIdx + 1 >= buffer.Length)
                {
                    throw new ArgumentException($"Pointer extends beyond buffer: need 2 bytes at position {wordSizeIdx}, but buffer length is {buffer.Length}");
                }

                var pointer = Read2BytesAsInt(buffer, wordSizeIdx);
                pointer &= ~(0xc0 << 8);

                // Validate pointer doesn't point beyond buffer or to invalid location
                if (pointer >= buffer.Length)
                {
                    throw new ArgumentException($"Pointer value {pointer} points beyond buffer length {buffer.Length}");
                }

                // Prevent pointer loops by ensuring pointer points backwards
                if (pointer >= start)
                {
                    throw new InvalidOperationException($"Invalid pointer: {pointer} points forward or to current position (start: {start}), potential infinite loop");
                }

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
            val |= bytes[start] << 8;

            return val;
        }

        private RecordType ParseQueryType(byte[] buffer, int index)
        {
            int result = buffer[index + 1];
            result |= (buffer[index] << 8);
            //result -= 1;
            return (RecordType)result;
        }

        private RecordClass ParseQueryClass(byte[] buffer, int index)
        {
            int result = buffer[index + 1];
            result |= (buffer[index] << 8);
            //result -= 1;
            return (RecordClass)result;
        }
    }
}
