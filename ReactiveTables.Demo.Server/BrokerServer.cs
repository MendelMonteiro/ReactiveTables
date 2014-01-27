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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Comms;
using ReactiveTables.Framework.Filters;
using ReactiveTables.Framework.Joins;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Protobuf;
using ReactiveTables.Framework.Synchronisation;

namespace ReactiveTables.Demo.Server
{
    /// <summary>
    /// Simulates a broker aggregation server which receives many updates for different brokers
    /// and sends the updates to a client.  Will also aggregate on the server side to select the best bid/asks
    /// </summary>
    class BrokerServer : IServer
    {
        private readonly ManualResetEventSlim _finished = new ManualResetEventSlim();

        public void Start()
        {
            Console.WriteLine("Starting broker service");
            _finished.Reset();

            var outputTable = new ReactiveTable();
            BrokerTableDefinition.SetupFeedTable(outputTable);
            var feeds = new ReactiveBatchedPassThroughTable(outputTable, new DefaultThreadMarshaller());


            var clientsTable = new ReactiveTable();
            BrokerTableDefinition.SetupClientFeedTable(clientsTable);

            SetupTcpServer(outputTable, feeds, clientsTable);

            StartBrokerClient(clientsTable);

            StartBrokerFeeds(feeds);
        }

        private static void StartBrokerClient(ReactiveTable clientsTable)
        {
            Task.Run(() =>
                         {
                             // Used non-batched pass through as we won't be receiving much data.
                             var clientTable = new ReactivePassThroughTable(clientsTable, new DefaultThreadMarshaller());
                             var client = new ReactiveTableTcpClient(clientTable, BrokerTableDefinition.ClientColumnsToFieldIds,
                                                                     (int) ServerPorts.BrokerFeedClients);
                             client.Start();
                         });
        }

        private void SetupTcpServer(IReactiveTable feeds, ReactiveBatchedPassThroughTable passThroughTable, ReactiveTable clientsTable)
        {
            var server = new ReactiveTableTcpServer(new ProtobufTableEncoder(),
                                                    new IPEndPoint(IPAddress.Loopback, (int) ServerPorts.BrokerFeed),
                                                    _finished,
                                                    s => GetClientTable(s, feeds, clientsTable),
                                                    () => UpdateClients(passThroughTable));

            // Start the server in a new thread
            Task.Run(() => server.Start(feeds, new ProtobufEncoderState {ColumnsToFieldIds = BrokerTableDefinition.ColumnsToFieldIds}));
        }

        private IReactiveTable GetClientTable(ReactiveClientSession reactiveClientSession, IReactiveTable feeds, ReactiveTable clientsTable)
        {
            var joined = new JoinedTable(clientsTable, feeds,
                                         new Join<string>(clientsTable, BrokerTableDefinition.BrokerClientColumns.ClientCcyPairColumn,
                                                          feeds, BrokerTableDefinition.BrokerColumns.CcyPairColumn, JoinType.Inner));
            var filtered = new FilteredTable(joined,
                                             new DelegatePredicate1<string>(
                                                 BrokerTableDefinition.BrokerClientColumns.ClientIpColumn,
                                                 ip => ip == reactiveClientSession.RemoteEndPoint.Address.ToString()));

            return filtered;
        }

        /// <summary>
        /// Push all the changes to the clients
        /// </summary>
        /// <param name="passThroughTable"></param>
        private void UpdateClients(ReactiveBatchedPassThroughTable passThroughTable)
        {
            passThroughTable.SynchroniseChanges();
        }

        private static void StartBrokerFeeds(IWritableReactiveTable feeds)
        {
            var feeders = new List<BrokerFeed>
                              {
                                  new BrokerFeed("Tullet", feeds),
                                  new BrokerFeed("VolBroker", feeds),
                                  new BrokerFeed("Broker1", feeds)
                              };

            foreach (var brokerFeed in feeders)
            {
                brokerFeed.Start();
            }
        }

        public void Stop()
        {
            Console.WriteLine("Stopping service");
            _finished.Set();
        }
    }

    class BrokerFeed
    {
        private readonly IWritableReactiveTable _table;
        private readonly Random _random = new Random();
        private readonly int[] _rowIndeces = new int[15];
        private readonly string[] _maturities = new[] { "ON", "1W", "2W", "3W", "1M", "2M", "3M", "4M", "5M", "6M", "1Y", "2Y", "3Y", "5Y", "10Y" };

        public BrokerFeed(string name, IWritableReactiveTable table)
        {
            Name = name;
            _table = table;
        }

        public string Name { get; private set; }

        /// <summary>
        /// All Broker feeds must be started in same thread so that AddRow is called on same thread.
        /// </summary>
        public void Start()
        {
            for (int i = 0; i < _maturities.Length; i++)
            {
                int rowIndex = _table.AddRow();
                _rowIndeces[i] = rowIndex;
                _table.SetValue(BrokerTableDefinition.BrokerColumns.MaturityColumn, rowIndex, _maturities[i]);
                _table.SetValue(BrokerTableDefinition.BrokerColumns.CcyPairColumn, rowIndex, "EUR/USD");
                _table.SetValue(BrokerTableDefinition.BrokerColumns.BrokerNameColumn, rowIndex, Name);
            }

            Task.Run(() => FeedBrokerData());
        }

        private void FeedBrokerData()
        {
            while (true)
            {
                for (int i = 0; i < _rowIndeces.Length; i++)
                {
                    _table.SetValue(BrokerTableDefinition.BrokerColumns.BidColumn, _rowIndeces[i], _random.NextDouble());
                    _table.SetValue(BrokerTableDefinition.BrokerColumns.AskColumn, _rowIndeces[i], _random.NextDouble());
                }
                Thread.Sleep(50);
            }
        }
    }
}
