using Bns.StubResolver.Dns;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Dns.Test
{
    public class HeaderParseTests
    {

        [Test]
        public void TestDeserializeHeader()
        {
            var b = new byte[] { 0xdf, 0xa8, 0x80, 0xa0, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 };
            var header = Header.Parse(b);
            Assert.AreEqual(header.Id, 57256);
            Assert.IsTrue(header.IsResponse);
            Assert.AreEqual(HeaderOpCode.StandardQuery, header.Opcode);
            Assert.IsFalse(header.IsAuthoritativeAnswer);
            Assert.IsFalse(header.IsTruncated);
            Assert.IsFalse(header.RecursionDesired);
            Assert.IsTrue(header.RecursionAvailable);
            Assert.AreEqual(2, header.Z);
            Assert.AreEqual(ResponseCode.NoError, header.Rcode);
            Assert.AreEqual(1, header.QueryCount);
            Assert.AreEqual(1, header.AnswerCount);
            Assert.AreEqual(0, header.AuthorityCount);
            Assert.AreEqual(0, header.AddtlCount);
        }

        [Test]
        public void TestSerializeHeader()
        {
            var b = new byte[] { 0xdf, 0xa8, 0x80, 0xa0, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 };
            var header = Header.Parse(b);
            var newB = header.ToByteArray();
            Assert.AreEqual(b, newB);
        }
    }
}