using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeraPacketRetranslator.Config;
using TeraPacketRetranslator.Messages;

namespace TeraPacketRetranslator.Processing.Processors
{
    public abstract class PacketProcessor
    {
        PacketFilterConfig m_Filter;
        protected PacketProcessor(ProcessorConfig config)
        {
            m_Filter = config.Filter;
        }

        protected bool CheckFilter(int code)
        {
            return (m_Filter == null) ? true : m_Filter.Check(code);
        }
    }

    public abstract class PPInputRaw : PacketProcessor
    {
        protected PPInputRaw(ProcessorConfig config)
            : base(config)
        { }
        public abstract RawMessage Process();

    }

    public abstract class PPRawTranslated : PacketProcessor
    {
        protected PPRawTranslated(ProcessorConfig config)
            : base(config)
        { }
        public abstract ParsedMessage Process(RawMessage raw);

        public virtual bool CheckFilter(RawMessage raw) => CheckFilter(raw.OpCode);
    }

    public abstract class PPRawOutput: PacketProcessor
    {
        protected PPRawOutput(ProcessorConfig config)
            : base(config)
        { }
        public abstract void Process(RawMessage raw);

        public virtual bool CheckFilter(RawMessage raw) => CheckFilter(raw.OpCode);
    }

    public abstract class PPTranslatedTranslated : PacketProcessor
    {
        protected PPTranslatedTranslated(ProcessorConfig config)
            : base(config)
        { }
        public abstract ParsedMessage Process(ParsedMessage msg);

        public virtual bool CheckFilter(ParsedMessage msg) => CheckFilter(msg.OpCode);
    }

    public abstract class PPTranslatedOutput : PacketProcessor
    {
        protected PPTranslatedOutput(ProcessorConfig config)
            : base(config)
        { }
        public abstract void Process(ParsedMessage msg);

        public virtual bool CheckFilter(ParsedMessage msg) => CheckFilter(msg.OpCode);
    }
}
