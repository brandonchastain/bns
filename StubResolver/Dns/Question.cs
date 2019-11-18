using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    class Question
    {
        string qname; // max length 255 octets
        RecordType qtype;
        RecordClass qclass;
    }
}
