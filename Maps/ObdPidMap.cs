using System.Collections.Generic;

namespace J2534Diag
{
    public static class ObdPidMap
    {
        public static List<ObdPid> GetObdPids(bool useImperialUnits)
        {
            return new List<ObdPid>
            {
                new ObdPid {
                    ArbId = new byte[] { 0x00, 0x00, 0x07, 0xDF },
                    Sid = 0x02,
                    Mode = 0x01,
                    Pid = 0x0C,
                    Name = "Engine RPM",
                    Unit = "rpm",
                    Formula = data => ((data[0] << 8) | data[1]) / 4.0,
                    Enabled = true
                },

                new ObdPid {
                    ArbId = new byte[] { 0x00, 0x00, 0x07, 0xDF },
                    Sid = 0x02,
                    Mode = 0x01,
                    Pid = 0x0D,
                    Name = "Vehicle Speed",
                    Unit = useImperialUnits ? "mph" : "km/h",
                    Formula = data => useImperialUnits ? data[0] * 0.621371 : data[0],
                    Enabled = false
                },          
                new ObdPid {
                    ArbId = new byte[] { 0x00, 0x00, 0x07, 0xDF },
                    Sid = 0x02,
                    Mode = 0x01,
                    Pid = 0x06,
                    Name = "STFT Bank 1",
                    Unit = "%",
                    Formula = data => (data[0] - 128) * 100.0 / 128.0,
                    Enabled = true
                },
                new ObdPid {
                    ArbId = new byte[] { 0x00, 0x00, 0x07, 0xDF },
                    Sid = 0x02,
                    Mode = 0x01,
                    Pid = 0x07,
                    Name = "LTFT Bank 1",
                    Unit = "%",
                    Formula = data => (data[0] - 128) * 100.0 / 128.0,
                    Enabled = true
                },
                new ObdPid {
                    ArbId = new byte[] { 0x00, 0x00, 0x07, 0xDF },
                    Sid = 0x02,
                    Mode = 0x01,
                    Pid = 0x08,
                    Name = "STFT Bank 2",
                    Unit = "%",
                    Formula = data => (data[0] - 128) * 100.0 / 128.0,
                    Enabled = true
                },
                new ObdPid {
                    ArbId = new byte[] { 0x00, 0x00, 0x07, 0xDF },
                    Sid = 0x02,
                    Mode = 0x01,
                    Pid = 0x09,
                    Name = "LTFT Bank 2",
                    Unit = "%",
                    Formula = data => (data[0] - 128) * 100.0 / 128.0,
                    Enabled = true
                },
                new ObdPid {
                    ArbId = new byte[] { 0x00, 0x00, 0x07, 0xDF },
                    Sid = 0x02,
                    Mode = 0x01,
                    Pid = 0x11,
                    Name = "Throttle Position",
                    Unit = "%",
                    Formula = data => data[0] * 100.0 / 255.0,
                    Enabled = true
                },
                new ObdPid {
                    ArbId = new byte[] { 0x00, 0x00, 0x07, 0xDF },
                    Sid = 0x02,
                    Mode = 0x01,
                    Pid = 0x05,
                    Name = "Coolant Temp",
                    Unit = useImperialUnits ? "°F" : "°C",
                    Formula = data => useImperialUnits ? ((data[0] - 40) * 9.0 / 5.0) + 32 : data[0] - 40,
                    Enabled = false
                },
            };
        }
    }
}
