using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml.Linq;

namespace TeraPacketRetranslator.Config
{
    public static class Config
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static List<ProcessorConfig> Processors { get; private set; }

        static Config()
        {
            Reload();
        }

        static void Reload()
        {
            string fileName = ConfigurationManager.AppSettings["PublicConfig"];
            var xml = XDocument.Load(fileName);

            try
            {
                Processors = xml.Element("config").Elements("processor").Select(elem => new ProcessorConfig(elem)).ToList();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }

        }
    }
}
