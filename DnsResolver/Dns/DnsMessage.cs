using Bns.StubResolver.Udp.Contracts;
using System.Collections.Generic;
using System;
using Bns.Dns.ResourceRecords;

namespace Bns.Dns
{
    public class DnsMessage
    {
        public DnsMessage()
        {
            this.Answers = new List<ResourceRecord>();
            this.Authority = new List<ResourceRecord>();
            this.Additional = new List<ResourceRecord>();
        }

        public Header Header { get; set; }
        public Question Question { get; set; }
        public List<ResourceRecord> Answers { get; set; }
        public List<ResourceRecord> Authority { get; set; }
        public List<ResourceRecord> Additional { get; set; }

        public void AddAnswer(ResourceRecord rec)
        {
            this.Answers.Add(rec);
        }

        public void AddAuthority(ResourceRecord rec)
        {
            this.Authority.Add(rec);
        }

        public void AddAddtl(ResourceRecord rec)
        {
            this.Additional.Add(rec);
        }

        public void AddAnswersAndIncrementCount(List<ResourceRecord> records)
        {
            foreach(var rec in records)
            {
                this.AddAnswer(rec);
                this.Header.AnswerCount++;
            }
        }

        public void AddAuthorityAndIncrementCount(List<ResourceRecord> records)
        {
            foreach (var rec in records)
            {
                this.AddAuthority(rec);
                this.Header.AuthorityCount++;
            }
        }

        public void AddAdditionalAndIncrementCount(List<ResourceRecord> records)
        {
            foreach (var rec in records)
            {
                this.AddAddtl(rec);
                this.Header.AddtlCount++;
            }
        }

        public override string ToString()
        {
            return this.Header.ToString() + "\n" + this.Question.ToString();
        }
    }
}
