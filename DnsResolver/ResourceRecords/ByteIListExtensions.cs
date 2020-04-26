using BinaryReader;
using System;
using System.Collections.Generic;

namespace Bns.Dns.Serialization
{
    internal static class ByteIListExtensions
    {
        public static void AppendIntAs2Bytes(this IList<byte> bytes, int val)
        {
            BinaryIntReader.AppendIntAs2Bytes(bytes, val);
        }

        public static void AppendIntAs4Bytes(this IList<byte> bytes, int val)
        {
            BinaryIntReader.AppendIntAs4Bytes(bytes, val);
        }
    }
}
