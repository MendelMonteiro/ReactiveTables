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
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Threading;
using ReactiveTables.Demo.Server;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Comms;
using ReactiveTables.Framework.Filters;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.SimpleBinaryEncoding;
using ReactiveTables.Framework.Synchronisation;
using ReactiveTables.Framework.Utils;

namespace ReactiveTables.Demo.Services
{
    public interface IBrokerFeedDataService
    {
        IReactiveTable Feeds { get; }
        IWritableReactiveTable CurrencyPairs { get; }
        void Start(Dispatcher dispatcher);
        void Stop();
    }

    class BrokerFeedDataService : IBrokerFeedDataService
    {
        private readonly ReactiveTable _feeds = new ReactiveTable();
        private readonly TimeSpan _synchroniseTablesDelay = TimeSpan.FromMilliseconds(50);
        private readonly ReactiveTable _currencyPairs = new ReactiveTable();
        private ReactiveTableTcpClient<IReactiveTable> _ccyPairSubscriptionClient;
        private ReactiveTableTcpClient<IWritableReactiveTable> _feedClient;

        public BrokerFeedDataService()
        {
            BrokerTableDefinition.SetupFeedTable(_feeds);
            BrokerTableDefinition.SetupClientFeedTable(_currencyPairs);
            _currencyPairs.AddColumn(new ReactiveColumn<bool>(BrokerTableDefinition.BrokerClientColumns.ClientSide.Selected));
        }

        public IReactiveTable Feeds
        {
            get { return _feeds; }
        }

        public IWritableReactiveTable CurrencyPairs
        {
            get { return _currencyPairs; }
        }

        public void Start(Dispatcher dispatcher)
        {
            // Connect up to receive the feeds
            var feedsWire = new ReactiveBatchedPassThroughTable(_feeds,
                                                                new WpfThreadMarshaller(dispatcher),
                                                                _synchroniseTablesDelay);
            var tableDecoder = GetTableDecoder();
            Task.Run(() => StartReceiving(feedsWire, BrokerTableDefinition.ColumnsToFieldIds, (int)ServerPorts.BrokerFeed, tableDecoder));

            // Subscribe to the feeds we want
            var currencyPairsWire = _currencyPairs;// new ReactivePassThroughTable(_currencyPairs, new ThreadPoolThreadMarshaller());
            var tableEncoder = GetTableEncoder();
            Task.Run(() => SetupFeedSubscription(currencyPairsWire, tableEncoder));
        }

        private static IReactiveTableProcessor<IReactiveTable> GetTableEncoder()
        {
#if SBE
            return new SbeTableEncoder();
#else
            return new ProtobufTableEncoder();
#endif
        }

        private static IReactiveTableProcessor<IWritableReactiveTable> GetTableDecoder()
        {
#if SBE
            return new SbeTableDecoder();
#else
            return new ProtobufTableDecoder();
#endif
        }

        private static object GetEncoderState()
        {
#if SBE
            return new SbeTableEncoderState{ColumnsToFieldIds = BrokerTableDefinition.ClientColumnsToFieldIds};
#else
            return new ProtobufEncoderState(BrokerTableDefinition.ClientColumnsToFieldIds);
#endif
        }

        private static object GetDecoderState(Dictionary<string, int> columnsToFieldIds)
        {
#if SBE
            return new SbeTableDecoderState{FieldIdsToColumns = columnsToFieldIds.InverseUniqueDictionary()};
#else
            return new ProtobufDecoderState(columnsToFieldIds.InverseUniqueDictionary());
#endif
        }

        private void SetupFeedSubscription(IReactiveTable currenciesTable, IReactiveTableProcessor<IReactiveTable> encoder)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, (int)ServerPorts.BrokerFeedClients);
            var selectedCurrencies = new FilteredTable(
                currenciesTable,
                new DelegatePredicate1<bool>(BrokerTableDefinition.BrokerClientColumns.ClientSide.Selected, selected => selected));

            var client = new ReactiveTableTcpClient<IReactiveTable>(encoder,
                                                                    selectedCurrencies,
                                                                    GetEncoderState(),
                                                                    endPoint);
            _ccyPairSubscriptionClient = client;
            try
            {
                client.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void StartReceiving(IWritableReactiveTable wireTable, Dictionary<string, int> columnsToFieldIds, int port, IReactiveTableProcessor<IWritableReactiveTable> decoder)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port);
            var client = new ReactiveTableTcpClient<IWritableReactiveTable>(decoder,
                                                                            wireTable,
                                                                            GetDecoderState(columnsToFieldIds),
                                                                            endPoint);
            _feedClient = client;
            try
            {
                client.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void Stop()
        {
            if (_feedClient != null) _feedClient.Dispose();
            if (_ccyPairSubscriptionClient != null) _ccyPairSubscriptionClient.Dispose();
        }
    }
}