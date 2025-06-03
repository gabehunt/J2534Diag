using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2534Diag
{
    public class IsoTpFrameEventArgs : EventArgs
    {
        public IsoTpFrameType FrameType { get; }
        public byte[] CanId { get; }
        public byte[] Payload { get; }
        public bool IsComplete { get; }

        public IsoTpFrameEventArgs(IsoTpFrameType frameType, byte[] canId, byte[] payload, bool isComplete = false)
        {
            FrameType = frameType;
            CanId = canId;
            Payload = payload;
            IsComplete = isComplete;
        }
    }
}
