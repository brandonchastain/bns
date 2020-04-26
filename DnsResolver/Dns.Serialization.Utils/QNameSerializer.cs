using System;
using System.Collections.Generic;

namespace Dns.Serialization.Utils
{
    public static class QNameSerializer
    {
        public static IList<byte> SerializeQName(string qname)
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
    }
}
