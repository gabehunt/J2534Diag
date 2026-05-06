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
                    ArbId = new byte[] { 0x18, 0xDB, 0x33, 0xF1 },
                    Length = 0x03,
                    Service = 0x22,
                    DidHighByte = 0xF4,
                    DidLowByte = 0x06,
                    Name = "STFT Bank 1",
                    Unit = "%",
                    Formula = data => (data[0] - 128) * 100.0 / 128.0,
                    Enabled = true
                },
                new UdsPid {
                    ArbId = new byte[] { 0x18, 0xDB, 0x33, 0xF1 },
                    Length = 0x03,
                    Service = 0x22,
                    DidHighByte = 0xF4,
                    DidLowByte = 0x07,
                    Name = "LTFT Bank 1",
                    Unit = "%",
                    Formula = data => (data[0] - 128) * 100.0 / 128.0,
                    Enabled = true
                },
                new UdsPid {
                    ArbId = new byte[] { 0x18, 0xDB, 0x33, 0xF1 },
                    Length = 0x03,
                    Service = 0x22,
                    DidHighByte = 0xF4,
                    DidLowByte = 0x08,
                    Name = "STFT Bank 2",
                    Unit = "%",
                    Formula = data => (data[0] - 128) * 100.0 / 128.0,
                    Enabled = true
                },
                new UdsPid {
                    ArbId = new byte[] { 0x18, 0xDB, 0x33, 0xF1 },
                    Length = 0x03,
                    Service = 0x22,
                    DidHighByte = 0xF4,
                    DidLowByte = 0x09,
                    Name = "LTFT Bank 2",
                    Unit = "%",
                    Formula = data => (data[0] - 128) * 100.0 / 128.0,
                    Enabled = true
                },
                new UdsPid {
                    ArbId = new byte[] { 0x18, 0xDB, 0x33, 0xF1 },
                    Length = 0x03,
                    Service = 0x22,
                    DidHighByte = 0xF4,
                    DidLowByte = 0x0C,
                    Name = "Engine RPM",
                    Unit = "rpm",
                    Formula = data => ((data[0] << 8) | data[1]) / 4.0,
                    Enabled = true
                },
                new UdsPid {
                    ArbId = new byte[] { 0x18, 0xDB, 0x33, 0xF1 },
                    Length = 0x03,
                    Service = 0x22,
                    DidHighByte = 0xF4,
                    DidLowByte = 0x11,
                    Name = "Throttle Position",
                    Unit = "%",
                    Formula = data => data[0] * 100.0 / 255.0,
                    Enabled = true
                },
            };
        }
    }
}
