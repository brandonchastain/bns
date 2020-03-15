using System;
using System.Text;

namespace Bns.StubResolver.Common
{
    public static class HexPrinter
    {
        public static string ToHexString(byte[] bytes, int size)
        {
            var result = new StringBuilder(bytes.Length * 2);
            var hexAlphabet = "0123456789ABCDEF";
            var numChars = 0;
            for (int i = 0; i < size; i++)
            {
                var b = bytes[i];
                result.Append(hexAlphabet[b >> 4]);
                result.Append(hexAlphabet[b & 0x0F]);

                numChars += 2;
                
                if (numChars % 8 == 0)
                {
                    result.Append(" ");
                }
            }

            return result.ToString();
        }

        public static void PrintBufferHex(byte[] bytes, int size)
        {
            Console.WriteLine(ToHexString(bytes, size));
        }
    }
}
