using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NetworkSniffer;
using System.Diagnostics;
using System.Threading;
using System.IO;
using TeraPacketRetranslator.Messages;
using TeraPacketRetranslator.Config;

namespace TeraPacketRetranslator.Extractor
{
    public class MessageExtractor
    {
        private readonly ConcurrentDictionary<TcpConnection, byte> _isNew = new ConcurrentDictionary<TcpConnection, byte>();
        private TcpConnection _clientToServer;
        private ConnectionDecrypter _decrypter;
        private MessageSplitter _messageSplitter;
        private TcpConnection _serverToClient;
        public int ClientProxyOverhead;
        private bool _connected;
        
        public bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                _isNew.Keys.ToList().ForEach(x => x.RemoveCallback());
                _isNew.Clear();
            }
        }

        public ConcurrentQueue<RawMessage> Packets = new ConcurrentQueue<RawMessage>();

        public int ServerProxyOverhead;

        public MessageExtractor()
        {
            var ipSniffer = new IpSnifferRawSocketMultipleInterfaces();

            var tcpSniffer = new TcpSniffer(ipSniffer);
            tcpSniffer.NewConnection += HandleNewConnection;
            tcpSniffer.EndConnection += HandleEndConnection;
            ipSniffer.Enabled = true;
        }


        public event Action<Server> NewConnection;
        public event Action<RawMessage> MessageReceived;
        public event Action EndConnection;
        public event Action<string> Warning;

        protected virtual void OnNewConnection(Server server)
        {
            if (NewConnection != null)
                NewConnection(server);
        }

        protected virtual void OnEndConnection()
        {
            if (EndConnection != null)
                EndConnection();
        }

        protected virtual void OnMessageReceived(RawMessage message)
        {
            Packets.Enqueue(message);
            if (MessageReceived != null)
                MessageReceived(message);
        }

        protected virtual void OnWarning(string obj)
        {
            if (Warning != null)
                Warning(obj);
        }


        private void HandleEndConnection(TcpConnection connection)
        {
            if (connection == _clientToServer || connection == _serverToClient)
            {
                _clientToServer?.RemoveCallback();
                _serverToClient?.RemoveCallback();
                Connected = false;
                OnEndConnection();
            }
            else connection.RemoveCallback();
            connection.DataReceived -= HandleTcpDataReceived;
        }

        private void HandleNewConnection(TcpConnection connection)
        {
            if (Connected || 
                !Database.Servers.Any(s => s.Ip.CompareTo(connection.Source.Address.ToString()) == 0) &&
                !Database.Servers.Any(s => s.Ip.CompareTo(connection.Destination.Address.ToString()) == 0))
            {
                return;
            }
            _isNew.TryAdd(connection, 1);
            connection.DataReceived += HandleTcpDataReceived;
        }

        private void HandleTcpDataReceived(TcpConnection connection, byte[] data, int needToSkip)
        {
            if (data.Length == 0)
            {
                if (needToSkip == 0 || !(connection == _clientToServer || connection == _serverToClient)) { return; }
                _decrypter?.Skip(connection == _clientToServer ? MessageDirection.ClientToServer : MessageDirection.ServerToClient, needToSkip);
                return;
            }
            if (!Connected && _isNew.ContainsKey(connection))
            {
                
                if (Database.Servers.Any(s => s.Ip.CompareTo(connection.Source.Address.ToString()) == 0) && data.Take(4).SequenceEqual(new byte[] { 1, 0, 0, 0 }))
                {
                    byte q;
                    _isNew.TryRemove(connection, out q);
                    var server = Database.Servers.First(s => s.Ip.CompareTo(connection.Source.Address.ToString()) == 0);
                    _serverToClient = connection;
                    _clientToServer = null;

                    ServerProxyOverhead = (int)connection.BytesReceived;
                    _decrypter = new ConnectionDecrypter(server.Region);
                    _decrypter.ClientToServerDecrypted += HandleClientToServerDecrypted;
                    _decrypter.ServerToClientDecrypted += HandleServerToClientDecrypted;

                    _messageSplitter = new MessageSplitter();
                    _messageSplitter.MessageReceived += HandleMessageReceived;
                    _messageSplitter.Resync += OnResync;
                }
                if (_serverToClient != null && _clientToServer == null && _serverToClient.Destination.Equals(connection.Source) &&
                    _serverToClient.Source.Equals(connection.Destination))
                {
                    ClientProxyOverhead = (int)connection.BytesReceived;
                    byte q;
                    _isNew.TryRemove(connection, out q);
                    _clientToServer = connection;
                    var server = Database.Servers.First(s => s.Ip.CompareTo(connection.Destination.Address.ToString()) == 0);
                    _isNew.Clear();
                    OnNewConnection(server);
                }
                if (connection.BytesReceived > 0x10000) //if received more bytes but still not recognized - not interesting.
                {
                    byte q;
                    _isNew.TryRemove(connection, out q);
                    connection.DataReceived -= HandleTcpDataReceived;
                    connection.RemoveCallback();
                }
            }

            if (!(connection == _clientToServer || connection == _serverToClient)) { return; }
            if (_decrypter == null) { return; }
            if (connection == _clientToServer) { _decrypter.ClientToServer(data, needToSkip); }
            else { _decrypter.ServerToClient(data, needToSkip); }

        }

        private void OnResync(MessageDirection direction, int skipped, int size)
        {
        }

        private void HandleMessageReceived(RawMessage message)
        {
            OnMessageReceived(message);
        }

        private void HandleServerToClientDecrypted(byte[] data)
        {
            _messageSplitter.ServerToClient(DateTime.UtcNow, data);
        }

        private void HandleClientToServerDecrypted(byte[] data)
        {
            _messageSplitter.ClientToServer(DateTime.UtcNow, data);
        }
    }
}
