using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeraPacketRetranslator.Config;
using TeraPacketRetranslator.Messages;
using TeraPacketRetranslator.PacketLog;

namespace TeraPacketRetranslator.Processing.Processors
{
    public class DumpWriter : PPRawOutput
    {
        PacketLogWriter m_Writer;
        public DumpWriter(ProcessorConfig config)
            : base(config)
        {
            m_Writer = new PacketLogWriter(config.Path, new LogHeader() { Region = string.Empty });
        }

        public override void Process(RawMessage raw)
        {
            m_Writer.Append(raw);
        }
    }
}
