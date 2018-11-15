using System.IO;
using System.Text;
using TeraPacketRetranslator.Messages;

namespace TeraPacketRetranslator.Parser
{
    public class MessageReader : BinaryReader
    {
        public MessageReader(RawMessage message)
            : base(GetStream(message), Encoding.Unicode)
        {
            Message = message;
        }

        public RawMessage Message { get; private set; }

        private static MemoryStream GetStream(RawMessage message)
        {
            return new MemoryStream(message.Payload.Array, message.Payload.Offset, message.Payload.Count, false, true);
        }

        public Vector3f ReadVector3f()
        {
            Vector3f result;
            result.X = ReadSingle();
            result.Y = ReadSingle();
            result.Z = ReadSingle();
            return result;
        }

        public Angle ReadAngle()
        {
            return new Angle(ReadInt16());
        }

        public void Skip(int count)
        {
            ReadBytes(count);
        }

        public string ReadTeraString()
        {
            var builder = new StringBuilder();
            try
            {
                while (true)
                {
                    var c = ReadChar();
                    if (c == 0) return builder.ToString();
                    builder.Append(c);
                }
            }
            catch { return builder.ToString(); } 
        }
    }
}