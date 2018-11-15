// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using TeraPacketRetranslator.Extractor;
using TeraPacketRetranslator.Messages;

namespace TeraPacketRetranslator.PacketLog
{
    public class PacketLogWriter : IDisposable
    {
        private readonly bool _ownsStream;
        private Stream _stream;
        private DateTime _time;

        public PacketLogWriter(string filename, LogHeader header)
            : this(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read), true, header)
        {
        }

        public PacketLogWriter(Stream stream, bool ownsStream, LogHeader header)
        {
            _stream = stream;
            _ownsStream = ownsStream;
            StreamHelper.WriteBlock(_stream, BlockType.MagicBytes,
                new ArraySegment<byte>(LogHelper.Encoding.GetBytes(LogHelper.MagicBytes)));
            StreamHelper.WriteBlock(_stream, BlockType.Region,
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(header.Region)));
            StreamHelper.WriteBlock(_stream, BlockType.Start, new ArraySegment<byte>(new byte[0]));
        }

        public void Dispose()
        {
            if (_ownsStream)
            {
                _stream.Dispose();
            }
            _stream = null;
        }

        public void Append(RawMessage message)
        {
            if (_stream == null)
                throw new ObjectDisposedException("LogWriter");

            if (message.Time != _time)
            {
                StreamHelper.WriteBlock(_stream, BlockType.Timestamp,
                    new ArraySegment<byte>(LogHelper.DateTimeToBytes(message.Time)));
                _time = message.Time;
            }

            var blockType = message.Direction == MessageDirection.ClientToServer ? BlockType.Client : BlockType.Server;
            StreamHelper.WriteBlock(_stream, blockType, message.Data);

            _stream.Flush();
        }
    }
}