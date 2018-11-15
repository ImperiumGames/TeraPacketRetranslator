using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeraPacketRetranslator.Config;
using TeraPacketRetranslator.Messages;
using TeraPacketRetranslator.PacketLog;

namespace TeraPacketRetranslator.Processing.Processors
{
    public class DumpReader : PPInputRaw
    {
        PacketLogReader m_Reader;
        public DumpReader(ProcessorConfig config)
            : base(config)
        {
            m_Reader = new PacketLogReader(new FileStream(config.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }
        public override RawMessage Process()
        {
            return m_Reader.ReadMessage();
        }
    }
}
