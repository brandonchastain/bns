using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.Dns
{
    public enum RecordClass
    {
        IN = 1,      // Internet (default, most common) - using 0-based for your code
        CS = 2,      // CSNET (obsolete)
        CH = 3,      // Chaos 
        HS = 4,      // Hesiod
        NONE = 254,  // None (for dynamic updates)
        ANY = 255    // Wildcard query
    }
}
