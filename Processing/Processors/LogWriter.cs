using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using TeraPacketRetranslator.Config;
using TeraPacketRetranslator.Messages;

namespace TeraPacketRetranslator.Processing.Processors
{
    public class LogWriter : PPTranslatedOutput
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        bool m_NoContent = false;
        public LogWriter(ProcessorConfig config)
            : base(config)
        {
            m_NoContent = config.Settings.Contains("nocontent");
        }

        public override void Process(ParsedMessage msg)
        {
            var str = msg.Time + ": " + msg.OpCodeName;

            if (!m_NoContent)
            {
                var serializer = new JavaScriptSerializer();
                str += ": " + Helpers.JsonPrettyPrint(serializer.Serialize(msg.GetContentWithPropertiesIncluded()));
            }

            log.Debug(str);
        }
    }
}
