using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Comms;
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
            Console.WriteLine("Starting service");
            _finished.Reset();

            var outputTable = new ReactiveTable();
            BrokerFeedTableDefinition.SetupFeedTable(outputTable);
            var feeds = new ReactiveBatchedPassThroughTable(outputTable,
                                                            new DefaultThreadMarshaller());

            SetupTcpServer(outputTable, feeds);

            StartBrokerFeeds(feeds);
        }

        private void SetupTcpServer(IWritableReactiveTable feeds, ReactiveBatchedPassThroughTable passThroughTable)
        {
            var server = new ReactiveTableTcpServer(new ProtobufTableEncoder(),
                                                    new IPEndPoint(IPAddress.Loopback, (int)ServerPorts.BrokerFeed),
                                                    _finished,
                                                    () => UpdateClients(passThroughTable));

            // Start the server in a new thread
            Task.Run(() =>
                     server.Start(feeds,
                                  new ProtobufEncoderState
                                      {
                                          ColumnsToFieldIds = BrokerFeedTableDefinition.ColumnsToFieldIds
                                      }));
        }

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
            /*_table = new ReactiveTable();
            _table.AddColumn(new ReactiveColumn<double>(BrokerColumns.BidColumn));
            _table.AddColumn(new ReactiveColumn<double>(BrokerColumns.AskColumn));
            _table.AddColumn(new ReactiveColumn<string>(BrokerColumns.MaturityColumn));
            _table.AddColumn(new ReactiveColumn<string>(BrokerColumns.BrokerNameColumn));*/

            for (int i = 0; i < _maturities.Length; i++)
            {
                int rowIndex = _table.AddRow();
                _rowIndeces[i] = rowIndex;
                _table.SetValue(BrokerFeedTableDefinition.BrokerColumns.MaturityColumn, rowIndex, _maturities[i]);
                _table.SetValue(BrokerFeedTableDefinition.BrokerColumns.CcyPairColumn, rowIndex, "EUR/USD");
                _table.SetValue(BrokerFeedTableDefinition.BrokerColumns.BrokerNameColumn, rowIndex, Name);
            }

            Task.Run(() => FeedBrokerData());
        }

        private void FeedBrokerData()
        {
            while (true)
            {
                for (int i = 0; i < _rowIndeces.Length; i++)
                {
                    _table.SetValue(BrokerFeedTableDefinition.BrokerColumns.BidColumn, _rowIndeces[i], _random.NextDouble());
                    _table.SetValue(BrokerFeedTableDefinition.BrokerColumns.AskColumn, _rowIndeces[i], _random.NextDouble());
                }
                Thread.Sleep(50);
            }
        }
    }
}
