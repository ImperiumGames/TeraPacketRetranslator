using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TeraPacketRetranslator.Config
{
    public enum ProtocolTerm
    {
        STRING,
        INT32,
        UINT32,
        INT64,
        UINT64,
        INT16,
        UINT16,
        BYTE,
        BYTES,
        BOOL,
        FLOAT,
        VEC3,
        VEC3FA,
        ANGLE,
        SKILLID,
        ARRAY
    }

    public class ProtocolLine
    {
        public int Indent { get; private set; }
        public ProtocolTerm Term { get; private set; }
        public string Name { get; private set; }

        public ProtocolLine(int indent, ProtocolTerm term, string name)
        {
            Indent = indent;
            Term = term;
            Name = name;
        }
    }

    public class ProtocolDefinition
    {
        public string Name { get; private set; }
        public List<ProtocolLine> Lines { get; private set; }  = new List<ProtocolLine>(); 

        public ProtocolDefinition(string name)
        {
            Name = name;
        }
        public static ProtocolDefinition ParseFromStream(Stream stream, string name, int currentVersion)
        {
            ProtocolDefinition result = new ProtocolDefinition(name);

            var termsDictionary = Enum.GetValues(typeof(ProtocolTerm)).Cast<ProtocolTerm>().ToDictionary(e => e.ToString().ToLowerInvariant(), e=>e);

            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (line.StartsWith("#"))
                        {
                            if (line.StartsWith("# majorPatchVersion >= "))
                            {
                                int v = 0;
                                if (Int32.TryParse(line.Substring(23), out v) && v > currentVersion)
                                {
                                    return null;
                                }
                            }
                            
                            continue;
                        }
                        line = line.TrimStart();
                        line = line.Replace("-", "- ");
                        string[] words = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int i = 0;
                        while (words[i].CompareTo("-") == 0)
                            i++;
                        ProtocolTerm term;
                        if (words.Length >= i+2 && termsDictionary.TryGetValue(words[i], out term))
                        {
                            result.Lines.Add(new ProtocolLine(i, term, words[i + 1]));
                        }
                        else
                        {
                            
                        }

                    }
                }
            }
            return result;
        }

    }
}
