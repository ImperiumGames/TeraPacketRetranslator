using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TeraPacketRetranslator.Config
{
    public class ProtocolDatabase
    {
        static Dictionary<int, ProtocolDefinition> m_Protocols;
        public ProtocolDatabase(int version)
        {
            m_Protocols = new Dictionary<int, ProtocolDefinition>();

            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var protoDir = Path.Combine(directory, ConfigurationManager.AppSettings["ProtocolFolder"]);
           


            var filenames = Directory.GetFiles(protoDir, "*.def").OrderByDescending(s => s);
            foreach (var filename in filenames)
            {
                string shortName = Path.GetFileName(filename).Split('.')[0];
                int code = OpcodeDatabase.Instance.GetOpcodeByName(shortName);
                if (code != 0 && !m_Protocols.ContainsKey(code))
                {
                    var stream = File.OpenRead(filename);
                    var definition = ProtocolDefinition.ParseFromStream(stream, shortName, version);
                    if (definition != null)
                        m_Protocols.Add(code, definition);
                }
            }

            OpcodeDatabase.Instance.NamesToOpcodes.Where(kvp => !m_Protocols.ContainsKey(kvp.Value))
                            .ToList()
                            .ForEach(kvp => m_Protocols.Add(kvp.Value, new ProtocolDefinition(kvp.Key)));
        }

        public ProtocolDefinition GetProtocol(int opcode)
        {
            if (m_Protocols.ContainsKey(opcode))
                return m_Protocols[opcode];
            else
                return null;
        }

        public string GetProtocolName(int opcode)
        {
            return GetProtocol(opcode)?.Name;
        }

        public ProtocolDefinition GetProtocol(string name)
        {
            var found = m_Protocols.FirstOrDefault(kvp => kvp.Value.Name.CompareTo(name) == 0);
            if (!found.Equals(default(KeyValuePair<int, ProtocolDefinition>)))
                return found.Value;
            else
                return null;
        }

    }
}
