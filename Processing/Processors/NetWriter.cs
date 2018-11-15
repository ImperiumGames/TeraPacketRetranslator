using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using TeraPacketRetranslator.Config;
using TeraPacketRetranslator.Messages;

namespace TeraPacketRetranslator.Processing.Processors
{
    public class NetWriter : PPTranslatedOutput
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string m_Url;
        public NetWriter(ProcessorConfig config)
            : base(config)
        {
            m_Url = config.Path;
        }

        public override void Process(ParsedMessage msg)
        {
            var dictionary = msg.GetContentWithPropertiesIncluded();
            dictionary.Add("opcode", msg.OpCodeName);

            var serializer = new JavaScriptSerializer();
            var str = serializer.Serialize(dictionary);
            SendAsync(str);

        }

        async void SendAsync(string msg)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage responce = await client.PostAsync(m_Url, new StringContent(msg, Encoding.UTF8, "application/json"));
                    if (!responce.IsSuccessStatusCode)
                    {
                        log.Debug($"Error sending message\n{msg}\n{responce.ReasonPhrase}({responce.StatusCode})");
                    }
                }

            }
            catch (Exception e)
            {
                log.Debug(e.ToString());
            }
        }
    }
}