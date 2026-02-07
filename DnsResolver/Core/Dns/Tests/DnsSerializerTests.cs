using Bns.Dns;
using Bns.Dns.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Dns.Test
{
    [TestClass]
    public class DnsSerializerTests
    {

        private readonly static List<byte> www = new List<byte> { 0x03, 0x77, 0x77, 0x77 };
        private readonly static List<byte> microsoft = new List<byte> { 0x09, 0x6d, 0x69, 0x63, 0x72, 0x6f, 0x73, 0x6f, 0x66, 0x74 };
        private readonly static List<byte> com = new List<byte> { 0x03, 0x63, 0x6f, 0x6d };
        private readonly static List<byte> qnameEnd = new List<byte> { 0x00, 0x00, 0x01, 0x00, 0x01 };

        private DnsQuestionBinarySerializer serializer;

        [TestInitialize]
        public void SetUp()
        {
            serializer = new DnsQuestionBinarySerializer();
        }

        [TestMethod]
        public void TestSerializeQuestion()
        {
            var question = GetAQuestion("www.");
            var serializedQuestion = serializer.SerializeQuestion(question);
            var expected = combine(www, qnameEnd);
            CollectionAssert.AreEqual(expected, serializedQuestion);
        }

        [TestMethod]
        public void TestSerializeQuestionMultiPart()
        {
            var twoPartQuestion = GetAQuestion("www.microsoft.");
            var serializedQuestion = serializer.SerializeQuestion(twoPartQuestion);
            var expected = combine(www, microsoft, qnameEnd);
            CollectionAssert.AreEqual(expected, serializedQuestion);
        }

        [TestMethod]
        public void TestSerializeQuestionThreePart()
        {
            var threePartQuestion = GetAQuestion("www.microsoft.com.");
            var serializedQuestion = serializer.SerializeQuestion(threePartQuestion);
            var expected = combine(www, microsoft, com, qnameEnd);
            CollectionAssert.AreEqual(expected, serializedQuestion);
        }

        private Question GetAQuestion(string qname)
        {
            var q = new Question();
            q.QClass = RecordClass.IN;
            q.QType = RecordType.A;
            q.QName = qname;
            return q;
        }

        [TestMethod]
        public void TestParseQName()
        {
            var buffer = new List<byte>(www);
            byte endOfQName = 0x00;
            buffer.Add(endOfQName);
            var qname = serializer.ParseQuestionName(buffer.ToArray(), 0, out var bytesRead);
            Assert.AreEqual("www.", qname);
            Assert.AreEqual(5, bytesRead); // 3 chars, 1 for size, and 1 for 00.
        }

        [TestMethod]
        public void TestDeserializeQuestion()
        {
            var buffer = combine(www, qnameEnd);
            var q = serializer.DeserializeBytes(buffer, 0, out var bytesRead);

            Assert.AreEqual(RecordClass.IN, q.QClass);
            Assert.AreEqual(RecordType.A, q.QType);
            Assert.AreEqual("www.", q.QName);
            Assert.AreEqual(buffer.Length, bytesRead);
        }

        [TestMethod]
        public void TestDeserializeQuestionTwoParts()
        {
            var buffer = combine(www, microsoft, qnameEnd);
            var q = serializer.DeserializeBytes(buffer, 0, out var bytesRead);

            Assert.AreEqual(RecordClass.IN, q.QClass);
            Assert.AreEqual(RecordType.A, q.QType);
            Assert.AreEqual("www.microsoft.", q.QName);
            Assert.AreEqual(buffer.Length, bytesRead);
        }

        private byte[] combine(params List<byte>[] byteLists)
        {
            var result = new List<byte>();
            foreach (var list in byteLists)
            {
                result.AddRange(list);
            }
            return result.ToArray();
        }
    }
}