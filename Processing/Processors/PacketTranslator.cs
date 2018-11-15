using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeraPacketRetranslator.Config;
using TeraPacketRetranslator.Messages;
using TeraPacketRetranslator.Parser;

namespace TeraPacketRetranslator.Processing.Processors
{
    public class PacketTranslator : PPRawTranslated
    {
        ProtocolDatabase m_ProtocolDatabase;

        public PacketTranslator(ProcessorConfig config)
            : base(config)
        {
            m_ProtocolDatabase = Database.GetProtocols(config.Version);
        }
        public override ParsedMessage Process(RawMessage raw)
        {
            ProtocolDefinition def = m_ProtocolDatabase.GetProtocol(raw.OpCode);
            return def != null ? MessageFactory.Parse(raw, def) : null;
        }
    }
}
