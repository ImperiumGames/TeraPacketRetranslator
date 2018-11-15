using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeraPacketRetranslator.Messages
{
    public class S_CHAT : ParsedMessage
    {
        [PropertyReplacer(Key = "channel")]
        public ChatChannelEnum Channel
        {
            get
            {
                try { return (ChatChannelEnum)Content["channel"]; }
                catch { return default(ChatChannelEnum); }
            }
        }
    }
}
