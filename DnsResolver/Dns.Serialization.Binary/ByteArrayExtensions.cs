using System;
using System.Collections.Generic;

namespace Bns.Dns.Serialization
{
    internal static class ByteArrayExtensions
    {
        public static void AppendIntAs2Bytes(this IList<byte> bytes, int val)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            bytes.Add((byte)(val >> 8));
            bytes.Add((byte)val);
        }

        public static void AppendIntAs4Bytes(this IList<byte> bytes, int val)
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
