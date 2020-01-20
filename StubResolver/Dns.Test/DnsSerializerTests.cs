using Bns.StubResolver.Common;
using Bns.StubResolver.Dns;
using Bns.StubResolver.Dns.Serialization;
using NUnit.Framework;
using System;

namespace Dns.Test
{
    public class DnsSerializerTests
    {

        [Test]
        public void TestSerializeQuestion()
        {
            var q = new Question();
            q.QClass = RecordClass.IN;
            q.QType = RecordType.A;
            q.QName = "www.";
            var serializedQuestion = new DnsQuestionSerializer().SerializeQuestion(q);

            var b = new byte[] { 0x03, 0x77, 0x77, 0x77, 0x00, 0x00, 0x01, 0x00, 0x01 };
            var bHex = HexPrinter.ToHexString(b, b.Length);
            var sHex = HexPrinter.ToHexString(serializedQuestion, serializedQuestion.Length);
            Assert.AreEqual(bHex, sHex);
            Assert.AreEqual(b.Length, serializedQuestion.Length);
        }

        [Test]
        public void TestSerializeQuestionMultiPart()
        {
            var q = new Question();
            q.QClass = RecordClass.IN;
            q.QType = RecordType.A;
            q.QName = "www.microsoft.";
            var serializedQuestion = new DnsQuestionSerializer().SerializeQuestion(q);

            var b = new byte[] { 0x03, 0x77, 0x77, 0x77, 0x09, 0x6d, 0x69, 0x63, 0x72, 0x6f, 0x73, 0x6f, 0x66, 0x74, 0x00, 0x00, 0x01, 0x00, 0x01 };
            var bHex = HexPrinter.ToHexString(b, b.Length);
            var sHex = HexPrinter.ToHexString(serializedQuestion, serializedQuestion.Length);
            Assert.AreEqual(bHex, sHex);
            Assert.AreEqual(b.Length, serializedQuestion.Length);
        }

        [Test]
        public void TestSerializeQuestionThreePart()
        {
            var q = new Question();
            q.QClass = RecordClass.IN;
            q.QType = RecordType.A;
            q.QName = "www.microsoft.com.";
            var serializedQuestion = new DnsQuestionSerializer().SerializeQuestion(q);

            var b = new byte[] { 0x03, 0x77, 0x77, 0x77, 0x09, 0x6d, 0x69, 0x63, 0x72, 0x6f, 0x73, 0x6f, 0x66, 0x74, 0x03, 0x63, 0x6f, 0x6d, 0x00, 0x00, 0x01, 0x00, 0x01 };
            var bHex = HexPrinter.ToHexString(b, b.Length);
            var sHex = HexPrinter.ToHexString(serializedQuestion, serializedQuestion.Length);
            Assert.AreEqual(bHex, sHex);
            Assert.AreEqual(b.Length, serializedQuestion.Length);
        }

        [Test]
        public void TestParseQName()
        {
            var buffer = new byte[] { 0x03, 0x77, 0x77, 0x77, 0x00 };
            var qname = new DnsQuestionSerializer().ParseQuestionName(buffer, out var bytesRead);
            Assert.AreEqual("www.", qname);
            Assert.AreEqual(5, bytesRead); // 3 chars, 1 for size, and 1 for 00.
        }

        [Test]
        public void TestDeserializeQuestion()
        {
            var b = new byte[] { 0x03, 0x77, 0x77, 0x77, 0x00, 0x00, 0x01, 0x00, 0x01 };
            var q = new DnsQuestionSerializer().DeserializeBytes(b, out var bytesRead);

            Assert.AreEqual(RecordClass.IN, q.QClass);
            Assert.AreEqual(RecordType.A, q.QType);
            Assert.AreEqual("www.", q.QName);
            Assert.AreEqual(b.Length, bytesRead);
        }

        [Test]
        public void TestDeserializeQuestionTwoParts()
        {
            var b = new byte[] { 0x03, 0x77, 0x77, 0x77, 0x09, 0x6d, 0x69, 0x63, 0x72, 0x6f, 0x73, 0x6f, 0x66, 0x74, 0x00, 0x00, 0x01, 0x00, 0x01 };
            var q = new DnsQuestionSerializer().DeserializeBytes(b, out var bytesRead);

            Assert.AreEqual(RecordClass.IN, q.QClass);
            Assert.AreEqual(RecordType.A, q.QType);
            Assert.AreEqual("www.microsoft.", q.QName);
            Assert.AreEqual(b.Length, bytesRead);
        }
    }
}