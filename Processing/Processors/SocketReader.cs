using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeraPacketRetranslator.Config;
using TeraPacketRetranslator.Extractor;
using TeraPacketRetranslator.Messages;

namespace TeraPacketRetranslator.Processing.Processors
{
    public class SocketReader : PPInputRaw
    {
        MessageExtractor m_Extractor;
        public SocketReader(ProcessorConfig config)
            : base(config)
        {
            m_Extractor = new MessageExtractor();
        }
        public override RawMessage Process()
        {
            RawMessage msg;
            if (m_Extractor.Packets.TryDequeue(out msg))
                return msg;
            return null;
        }
    }
}
