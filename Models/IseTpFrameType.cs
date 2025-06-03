using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2534Diag
{
    public enum IsoTpFrameType
    {
        SingleFrame,
        FirstFrame,
        ConsecutiveFrame,
        FlowControl,
        Unknown
    }
}
