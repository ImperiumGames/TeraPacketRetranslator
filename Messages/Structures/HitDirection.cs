using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeraPacketRetranslator.Messages
{
    [Flags]
    public enum HitDirection
    {
        Back = 1,
        Left = 2,
        Right = 4,
        Side = 8,
        Front = 16,
        Dot = 32,
        Pet = 64
    }
}
