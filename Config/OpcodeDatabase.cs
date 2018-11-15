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
    public class OpcodeDatabase
    {
        static OpcodeDatabase m_Instance;

        public Dictionary<string, int> NamesToOpcodes { get; private set; }

        public static OpcodeDatabase Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new OpcodeDatabase();
                }
                return m_Instance;
            }
        }

        private OpcodeDatabase()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var opcodesPath = Path.Combine(directory, ConfigurationManager.AppSettings["OpcodeMap"]);

            NamesToOpcodes = File.ReadAllLines(opcodesPath)
                .Select(s => s.Replace(" = ", " ").Split(' '))
                .ToDictionary(parts => parts[0], parts => int.Parse(parts[1]));
        }

        public int GetOpcodeByName(string name)
        {
            int result;
            if (NamesToOpcodes.TryGetValue(name, out result))
                return result;
            return 0;
        }

        public string GetNameByOpcode(int code)
        {
            var found = NamesToOpcodes.FirstOrDefault(kvp => kvp.Value == code);
            if (!found.Equals(default(KeyValuePair<int, ProtocolDefinition>)))
                return found.Key;
            return null;
        }
    }
}
