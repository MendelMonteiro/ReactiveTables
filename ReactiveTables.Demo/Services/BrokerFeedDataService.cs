// This file is part of ReactiveTables.
// 
// ReactiveTables is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReactiveTables is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Threading;
using ReactiveTables.Demo.Server;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Protobuf;
using ReactiveTables.Framework.Synchronisation;

namespace ReactiveTables.Demo.Services
{
    public interface IBrokerFeedDataService
    {
        IReactiveTable Feeds { get; }
        void Start(Dispatcher dispatcher);
        void Stop();
    }

    class BrokerFeedDataService : IBrokerFeedDataService
    {
        private readonly ReactiveTable _feeds = new ReactiveTable();
        private readonly TimeSpan _synchroniseTablesDelay = TimeSpan.FromMilliseconds(50);
        private readonly List<TcpClient> _clients = new List<TcpClient>();
        private readonly List<ProtobufTableDecoder> _tableWriters = new List<ProtobufTableDecoder>();

        public BrokerFeedDataService()
        {
            BrokerTableDefinition.SetupFeedTable(_feeds);
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
                                          BrokerTableDefinition.ColumnsToFieldIds,
                                          (int) ServerPorts.BrokerFeed));
        }

        private void StartReceiving(IWritableReactiveTable wireTable, Dictionary<string, int> columnsToFieldIds, int port)
        {
            var client = new ReactiveTableTcpClient(wireTable, columnsToFieldIds, port);
            client.Start();
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