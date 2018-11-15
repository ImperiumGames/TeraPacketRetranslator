// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace TeraPacketRetranslator.Extractor
{
    internal static class StreamHelper
    {
        private static void ReadBytes(Stream stream, byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                var bytesRead = stream.Read(buffer, offset, count);
                if (bytesRead == 0)
                    throw new IOException("Unexpected end of stream");
                count -= bytesRead;
                offset += bytesRead;
            }
        }

        public static byte[] ReadBytes(this Stream stream, int count)
        {
            var result = new byte[count];
            ReadBytes(stream, result, 0, result.Length);
            return result;
        }
        public static void WriteBlock(Stream stream, BlockType blockType, ArraySegment<byte> data)
        {
            stream.WriteByte((byte)blockType);
            WriteRawBlock(stream, data);
        }

        public static void WriteRawBlock(Stream stream, ArraySegment<byte> data)
        {
            var size = data.Count + 2;
            if (size > ushort.MaxValue)
                throw new ArgumentException("data.Count is too big");
            var header = new byte[2];
            header[0] = unchecked((byte)size);
            header[1] = (byte)(size >> 8);
            stream.Write(header, 0, header.Length);
            stream.Write(data.Array, data.Offset, data.Count);
        }

        public static void ReadBlock(Stream stream, out BlockType blockType, out byte[] data)
        {
            blockType = (BlockType)stream.ReadByte();
            var sizeBuffer = stream.ReadBytes(2);
            var size = (ushort)(sizeBuffer[0] | sizeBuffer[1] << 8);
            data = stream.ReadBytes(size - 2);
        }

    }
}