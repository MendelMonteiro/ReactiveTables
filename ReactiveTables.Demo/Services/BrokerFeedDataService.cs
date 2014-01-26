using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Threading;
using ReactiveTables.Demo.Server;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Protobuf;
using ReactiveTables.Framework.Synchronisation;
using ReactiveTables.Utils;

namespace ReactiveTables.Demo.Services
{
    class BrokerFeedDataService : IBrokerFeedDataService
    {
        private readonly ReactiveTable _feeds = new ReactiveTable();
        private readonly TimeSpan _synchroniseTablesDelay = TimeSpan.FromMilliseconds(50);
        private readonly List<TcpClient> _clients = new List<TcpClient>();
        private readonly List<ProtobufTableDecoder> _tableWriters = new List<ProtobufTableDecoder>();

        public BrokerFeedDataService()
        {
            BrokerFeedTableDefinition.SetupFeedTable(_feeds);
        }

        public IReactiveTable Feeds
        {
            get { return _feeds; }
        }

        public void Start(Dispatcher dispatcher)
        {
            var feedsWire = new ReactiveBatchedPassThroughTable(_feeds,
                                                                new WpfThreadMarshaller(dispatcher),
                                                                _synchroniseTablesDelay);
            Task.Run(() => StartReceiving(feedsWire,
                                          BrokerFeedTableDefinition.ColumnsToFieldIds,
                                          (int) ServerPorts.BrokerFeed));
        }

        private void StartReceiving(ReactiveBatchedPassThroughTable wireTable, Dictionary<string, int> columnsToFieldIds, int port)
        {
            var client = new TcpClient();
            _clients.Add(client);
            // TODO: Handle disconnections
            client.Connect(IPAddress.Loopback, port);
            using (var stream = client.GetStream())
            {
//                FileStream file = new FileStream("broker-output.bin", FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
//                stream.CopyTo(file);

                var fieldIdsToColumns = columnsToFieldIds.InverseUniqueDictionary();
                var tableDecoder = new ProtobufTableDecoder(wireTable, fieldIdsToColumns, stream);
                _tableWriters.Add(tableDecoder);
                tableDecoder.Start();
            }
            //_client.Close();
        }

        public void Stop()
        {
            foreach (var writer in _tableWriters)
            {
                writer.Stop();
            }
            foreach (var client in _clients)
            {
                client.Close();
            }
        }
    }
}