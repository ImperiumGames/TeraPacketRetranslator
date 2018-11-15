using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using TeraPacketRetranslator.Config;

namespace TeraPacketRetranslator.Messages
{
    public class ParsedMessage
    {
        public string OpCodeName
        {
            get
            {
                string nameStr = OpcodeDatabase.Instance.GetNameByOpcode(OpCode);
                return (nameStr != null) ? nameStr : "UNKNOWN";
            }
        }

        public int OpCode { get; set; }

        public DateTime Time { get; set; }

        public Dictionary<string,object> Content = new Dictionary<string, object>();
        public Dictionary<string, object> GetContentWithPropertiesIncluded()
        {
            var props = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                    .Where(pi => pi.CanRead && pi.GetCustomAttributes(typeof(PropertyReplacerAttribute), true).Any());

            var keys = props.SelectMany(pi => pi.GetCustomAttributes(typeof(PropertyReplacerAttribute), true)).Select(o => (o as PropertyReplacerAttribute).Key);

            Dictionary<string, object> result = props.ToDictionary(pi => pi.Name, pi => pi.GetValue(this));

            Content.Where(kvp => !keys.Contains(kvp.Key)).ToList().ForEach(kvp => result.Add(kvp.Key, kvp.Value));

            return result;
        }

        public override string ToString()
        {
            var serializer = new JavaScriptSerializer();
            return Helpers.JsonPrettyPrint(serializer.Serialize(GetContentWithPropertiesIncluded()));
        }
    }
}