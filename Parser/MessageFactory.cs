using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeraPacketRetranslator.Config;
using TeraPacketRetranslator.Messages;
using static TeraPacketRetranslator.Helpers;

namespace TeraPacketRetranslator.Parser
{
    public class MessageFactory
    {
        static Dictionary<string, Type> m_SpecializedTypes =
            Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(ParsedMessage))).ToDictionary(t => t.Name, t => t);

        public static ParsedMessage Parse(RawMessage message, ProtocolDefinition proto)
        {
            ParsedMessage msg = (m_SpecializedTypes.ContainsKey(proto.Name)) ?
                                (ParsedMessage)Activator.CreateInstance(m_SpecializedTypes[proto.Name]) :
                                new ParsedMessage();

            msg.OpCode = message.OpCode;
            msg.Time = message.Time;
            using (MessageReader reader = new MessageReader(message))
            {
                msg.Content = ReadRecursive(proto.Lines, reader);
            }
            return msg;
        }

        static Dictionary<string,object> ReadRecursive(List<ProtocolLine> lines, MessageReader reader)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            Dictionary<string, ushort> offsets = new Dictionary<string, ushort>();
            Dictionary<string, ushort> counts = new Dictionary<string, ushort>();

            ProtocolLine prevProtoLine = null;

            if (lines.Count == 0) { return result; }
            int baseIndent = lines[0].Indent;

            //Read meta information
            for (int lineCounter = 0; lineCounter < lines.Count; lineCounter++)
            {
                if (lines[lineCounter].Indent != baseIndent) { continue; }
                ushort offset, count;
                switch (lines[lineCounter].Term)
                {
                    case ProtocolTerm.STRING:
                        offset = reader.ReadUInt16();
                        offsets.Add(lines[lineCounter].Name, offset);
                        break;
                    case ProtocolTerm.ARRAY:
                        count = reader.ReadUInt16();
                        counts.Add(lines[lineCounter].Name, count);
                        offset = reader.ReadUInt16();
                        offsets.Add(lines[lineCounter].Name, offset);
                        break;
                    case ProtocolTerm.BYTES:
                        offset = reader.ReadUInt16();
                        offsets.Add(lines[lineCounter].Name, offset);
                        count = reader.ReadUInt16();
                        counts.Add(lines[lineCounter].Name, count);
                        break;
                }
            }

            //Read content
            for (int lineCounter = 0; lineCounter < lines.Count; lineCounter++)
            {
                object value = null;
                ProtocolLine line = lines[lineCounter];
                switch (line.Term)
                {
                    case ProtocolTerm.INT32:
                        value = reader.ReadInt32();
                        break;
                    case ProtocolTerm.UINT32:
                        value = reader.ReadUInt32();
                        break;
                    case ProtocolTerm.INT64:
                        value = reader.ReadInt64();
                        break;
                    case ProtocolTerm.UINT64:
                        value = reader.ReadUInt64();
                        break;
                    case ProtocolTerm.INT16:
                        value = reader.ReadInt16();
                        break;
                    case ProtocolTerm.UINT16:
                        value = reader.ReadUInt16();
                        break;
                    case ProtocolTerm.FLOAT:
                        value = reader.ReadSingle();
                        break;
                    case ProtocolTerm.BYTE:
                        value = reader.ReadByte();
                        break;
                    case ProtocolTerm.BOOL:
                        value = reader.ReadBoolean();
                        break;
                    case ProtocolTerm.SKILLID:
                        value = new SkillId(reader.ReadUInt64());
                        break;
                    case ProtocolTerm.VEC3:
                    case ProtocolTerm.VEC3FA:
                        value = reader.ReadVector3f();
                        break;
                    case ProtocolTerm.ANGLE:
                        value = reader.ReadAngle();
                        break;
                    case ProtocolTerm.STRING:
                        ushort soffset;
                        if (!offsets.TryGetValue(line.Name, out soffset))
                        {
                            throw new FormatException();
                        }
                        if (soffset == 0) { continue; }
                        reader.BaseStream.Position = soffset - 4;
                        value = reader.ReadTeraString();
                        break;
                    case ProtocolTerm.BYTES:
                        ushort boffset;
                        ushort bcount;
                        if (!offsets.TryGetValue(line.Name, out boffset) || !counts.TryGetValue(line.Name, out bcount))
                        {
                            throw new FormatException();
                        }
                        if (boffset == 0 || bcount == 0) { continue; }
                        reader.BaseStream.Position = boffset - 4;
                        value = reader.ReadBytes(bcount);
                        break;
                    case ProtocolTerm.ARRAY:
                        ushort aoffset = 0;
                        ushort acount = 0;
                        if (!offsets.TryGetValue(line.Name, out aoffset) || !counts.TryGetValue(line.Name, out acount))
                        {
                            throw new FormatException();
                        }
                        var innerBlock = lines.Skip(lineCounter + 1).TakeWhile(l => l.Indent > line.Indent).ToList();
                        lineCounter += innerBlock.Count;

                        if (aoffset == 0 || acount == 0) { continue; }

                        value = new List<object>();

                        for (var arrayCounter = 0; arrayCounter < acount; arrayCounter++)
                        {
                            reader.BaseStream.Position = aoffset - 4;
                            var pointer = reader.ReadUInt16();
                            if (pointer != aoffset)
                            {
                                throw new FormatException();
                            }

                            ushort nextOffset = reader.ReadUInt16();

                            (value as List<object>).Add(ReadRecursive(innerBlock, reader));
                            //////
                            aoffset = nextOffset;
                        }


                        break;

                }
                if (value != null)
                    result.Add(line.Name, value);
            }
            return result;
        }

    }
}
