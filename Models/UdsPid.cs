using System;

namespace J2534Diag
{

    public class UdsPid
    {
        public byte[] ArbId { get; set; }
        public byte Length { get; set; }
        public byte Service { get; set; }
        public byte DidHighByte { get; set; }
        public byte DidLowByte { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public Func<byte[], double> Formula { get; set; }
        public bool Enabled { get; set; } = true;
    }
}