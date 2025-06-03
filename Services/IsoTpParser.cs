using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2534Diag
{
    public class IsoTpParser
    {
        private List<byte> _buffer = new List<byte>();
        private int _expectedLength = 0;
        private bool _collecting = false;

        public event EventHandler<IsoTpFrameEventArgs> FrameReceived;

        public void ProcessMessage(byte[] data)
        {
            if (data == null || data.Length < 5) return;

            var canId = data.Take(4).ToArray();
            var payload = data.Skip(4).ToArray();
            if (payload.Length == 0) return;
            //Debug.WriteLine($"Processing CAN ID: {BitConverter.ToString(canId)} Payload: {BitConverter.ToString(payload)}");
            byte pci = payload[0];
            int frameType = (pci & 0xF0) >> 4;

            switch (frameType)
            {
                case 0x0: // Single Frame
                    {
                        int sfLen = pci & 0x0F;
                        if (payload.Length >= 1 + sfLen)
                        {
                            var msg = payload.Skip(1).Take(sfLen).ToArray();
                            FrameReceived?.Invoke(this, new IsoTpFrameEventArgs(IsoTpFrameType.SingleFrame, canId, msg, true));
                        }
                        break;
                    }
                case 0x1: // First Frame
                    {
                        if (payload.Length >= 2)
                        {
                            _expectedLength = ((pci & 0x0F) << 8) + payload[1];
                            _buffer.Clear();
                            _buffer.AddRange(payload.Skip(2));
                            _collecting = true;
                            FrameReceived?.Invoke(this, new IsoTpFrameEventArgs(IsoTpFrameType.FirstFrame, canId, _buffer.ToArray(), false));
                        }
                        break;
                    }
                case 0x2: // Consecutive Frame
                    {
                        if (payload.Length > 1 && _collecting)
                        {
                            _buffer.AddRange(payload.Skip(1));
                            FrameReceived?.Invoke(this, new IsoTpFrameEventArgs(IsoTpFrameType.ConsecutiveFrame, canId, _buffer.ToArray(), false));
                            if (_buffer.Count >= _expectedLength)
                            {
                                _collecting = false;
                                FrameReceived?.Invoke(this, new IsoTpFrameEventArgs(IsoTpFrameType.ConsecutiveFrame, canId, _buffer.Take(_expectedLength).ToArray(), true));
                            }
                        }
                        break;
                    }
                case 0x3: // Flow Control
                    FrameReceived?.Invoke(this, new IsoTpFrameEventArgs(IsoTpFrameType.FlowControl, canId, payload, false));
                    break;
                default:
                    FrameReceived?.Invoke(this, new IsoTpFrameEventArgs(IsoTpFrameType.Unknown, canId, payload, false));
                    break;
            }
        }
    }

}
