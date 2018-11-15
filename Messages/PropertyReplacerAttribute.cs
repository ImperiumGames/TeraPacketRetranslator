using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeraPacketRetranslator.Messages
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyReplacerAttribute : Attribute
    {
        public string Key;
    }
}
