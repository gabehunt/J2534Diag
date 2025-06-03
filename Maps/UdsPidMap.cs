using System.Collections.Generic;

namespace J2534Diag
{
    public static class UdsPidMap
    {
        public static List<UdsPid> GetUdsPids(bool useImperialUnits)
        {
            return new List<UdsPid>
            {
                new UdsPid {
                    ArbId = new byte[] { 0x14, 0xDA, 0x11, 0xF1 },
                    Length = 0x03,
                    Service = 0x22,
                    DidHighByte = 0xF4,
                    DidLowByte = 0x0C,
                    Name = "Engine RPM",
                    Unit = "rpm",
                    Formula = data => ((data[0] << 8) | data[1]) / 4.0,
                    Enabled = true
                },
            };
        }
    }
}
