using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TeraPacketRetranslator.Config
{
    public static class Database
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static Dictionary<int, ProtocolDatabase> m_ProtocolDatabaseCache =  new Dictionary<int, ProtocolDatabase>();

        public static List<Server> Servers
        {
            get
            {
                if (m_ServerDatabase == null)
                {
                    InitializeServers();
                }
                return m_ServerDatabase;
            }
        }
        static List<Server> m_ServerDatabase;

        #region init

        static void InitializeServers()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            m_ServerDatabase = File.ReadAllLines(Path.Combine(directory, "servers.txt"))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Split(new[] { ' ' }, 4))
                .Select(
                    parts =>
                        new Server(parts[3], parts[1], parts[0],
                            !string.IsNullOrEmpty(parts[2]) ? uint.Parse(parts[2]) : uint.MaxValue))
                .Where(x => x.ServerId != uint.MaxValue).ToList();
            m_ServerDatabase.Add(new Server("VPN", "Unknown", "127.0.0.1"));
        }

        #endregion

        public static ProtocolDatabase GetProtocols(int version)
        {
            if (!m_ProtocolDatabaseCache.ContainsKey(version))
            {
                m_ProtocolDatabaseCache.Add(version, new ProtocolDatabase(version));
            }
            return m_ProtocolDatabaseCache[version];
        }
    }
}
