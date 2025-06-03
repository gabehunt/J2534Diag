using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2534Diag
{
    public class Mode6Test
    {
        public byte Tid { get; set; }
        public byte Cid { get; set; }
        public byte SubId { get; set; }
        public string Label { get; set; }
        public string Units { get; set; }
        public Func<byte[], ushort> DecodeValue { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
