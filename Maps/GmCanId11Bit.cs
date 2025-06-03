using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2534Diag
{
    public static class GmCanId11Bit
    {
        public static readonly Dictionary<uint, string> KnownCanIds = new Dictionary<uint, string>
        {
            { 0x7DF, "TESTER_TO_ECU_BROADCAST" },
            { 0x7E0, "TESTER_TO_ECU_REQUEST" },
            { 0x7EB, "ECU_TO_TESTER_RESPONSE" },
        };

    }
}
