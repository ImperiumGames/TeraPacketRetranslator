using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TeraPacketRetranslator.Config
{
    public class PacketFilterConfig
    {
        enum FilterType
        {
            None,
            Blacklist,
            Whitelist,
            Block
        }

        FilterType m_FilterType;

        List<string> m_Entries;

        public PacketFilterConfig(XElement elem)
        {
            string typeStr = elem.Attribute("type").Value;
            m_FilterType = (FilterType)Enum.Parse(typeof(FilterType), typeStr, true);
            m_Entries = (m_FilterType == FilterType.None || m_FilterType == FilterType.Block) ?
                new List<string>() :
                elem.Element(typeStr).Elements("opcode").Select(opcodeElem => opcodeElem.Value).ToList();
        }

        public bool Check(string name)
        {
            if (m_FilterType == FilterType.None) return true;
            if (m_FilterType == FilterType.Block) return false;

            bool contains = m_Entries.Contains(name);

            return (m_FilterType == FilterType.Whitelist) ? contains : !contains;
        }

        public bool Check(int code)
        {
            return Check(OpcodeDatabase.Instance.GetNameByOpcode(code));
        }
    }
}
