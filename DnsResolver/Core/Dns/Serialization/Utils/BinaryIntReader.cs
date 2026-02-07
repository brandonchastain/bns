using System;
using System.Collections.Generic;

namespace BinaryReader
{
    public class BinaryIntReader
    {
        public static void AppendIntAs2Bytes(IList<byte> bytes, int val)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            bytes.Add((byte)(val >> 8));
            bytes.Add((byte)val);
        }

        public static void AppendIntAs4Bytes(IList<byte> bytes, int val)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            bytes.Add((byte)(val >> 24));
            bytes.Add((byte)(val >> 16));
            bytes.Add((byte)(val >> 8));
            bytes.Add((byte)val);
        }
    }
}
