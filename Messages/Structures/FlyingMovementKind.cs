using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeraPacketRetranslator.Messages
{
    public enum FlyingMovementKind : uint
    {
        Forward = 2,
        Backward = 3,
        BoostForward = 4,
        TakeOff = 5,
        Ascend = 6,
        Descend = 7,
        Land = 8
    }
}


