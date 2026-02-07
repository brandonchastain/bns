using Bns.Dns;
using Bns.Dns.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace Dns.Test
{
    [TestClass]
    public class HeaderParseTests
    {
        private DnsMessageBinarySerializer serializer;

        [TestInitialize]
        public void ClassSetup()
        {
            var qSer = new DnsQuestionBinarySerializer();
            var rrSer = new ResourceRecordBinarySerializer(qSer);
            this.serializer = new DnsMessageBinarySerializer(rrSer, qSer);
        }

        [TestMethod]
        public void TestDeserializeHeader()
        {
            var b = new byte[] { 0xdf, 0xa8, 0x80, 0xa0, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 };
            var header = serializer.DeserializeHeader(b);
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

        [TestMethod]
        public void TestSerializeHeader()
        {
            var b = new byte[] { 0xdf, 0xa8, 0x80, 0xa0, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 };
            var header = serializer.DeserializeHeader(b);
            var newB = header.ToByteArray();
            CollectionAssert.AreEqual(b, newB);
        }
    }
}