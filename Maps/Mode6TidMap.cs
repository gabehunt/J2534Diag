using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace J2534Diag
{
    public static class Mode6TidMap
    {
        public static readonly List<Mode6Test> MisfireTests = new List<Mode6Test>()
        {
            new Mode6Test { Tid = 0xA2, Cid = 0x0B, SubId = 0x24, Label = "Cyl 1 History", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA2, Cid = 0x0C, SubId = 0x24, Label = "Cyl 1 Current", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA3, Cid = 0x0B, SubId = 0x24, Label = "Cyl 2 History", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA3, Cid = 0x0C, SubId = 0x24, Label = "Cyl 2 Current", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA4, Cid = 0x0B, SubId = 0x24, Label = "Cyl 3 History", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA4, Cid = 0x0C, SubId = 0x24, Label = "Cyl 3 Current", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA5, Cid = 0x0B, SubId = 0x24, Label = "Cyl 4 History", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA5, Cid = 0x0C, SubId = 0x24, Label = "Cyl 4 Current", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA6, Cid = 0x0B, SubId = 0x24, Label = "Cyl 5 History", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA6, Cid = 0x0C, SubId = 0x24, Label = "Cyl 5 Current", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA7, Cid = 0x0B, SubId = 0x24, Label = "Cyl 6 History", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA7, Cid = 0x0C, SubId = 0x24, Label = "Cyl 6 Current", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA8, Cid = 0x0B, SubId = 0x24, Label = "Cyl 7 History", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA8, Cid = 0x0C, SubId = 0x24, Label = "Cyl 7 Current", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA9, Cid = 0x0B, SubId = 0x24, Label = "Cyl 8 History", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
            new Mode6Test { Tid = 0xA9, Cid = 0x0C, SubId = 0x24, Label = "Cyl 8 Current", Units = "counts", DecodeValue = d => (ushort)((d[0] << 8) | d[1]) },
        };
    }

}
