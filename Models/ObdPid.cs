using System;

namespace J2534Diag
{

    public class ObdPid
    {
        public byte Mode { get; set; }
        public byte Pid { get; set; }
        public bool Enabled { get; set; } = false;
        public string Name { get; set; }
        public string Unit { get; set; }
        public Func<byte[], double> Formula { get; set; }
        public byte[] ArbId { get; internal set; }
        public byte Sid { get; internal set; }
    }
}