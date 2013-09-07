using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ReactiveTables.Demo.Server;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Synchronisation;

namespace ReactiveTables.Demo.Client
{
    class FxDataService
    {
        private readonly ReactiveTable _currencies;
        private readonly List<ProtobufTableWriter> _tableWriters = new List<ProtobufTableWriter>();
        private readonly TimeSpan _synchroniseTablesDelay = TimeSpan.FromMilliseconds(500);
        private readonly List<TcpClient> _clients = new List<TcpClient>();
        private readonly ReactiveTable _fxRates;

        public FxDataService()
        {
            _currencies = GetCurrenciesTable();
            _fxRates = GetRatesTable();
        }

        public ReactiveTable FxRates
        {
            get { return _fxRates; }
        }

        public ReactiveTable Currencies
        {
            get { return _currencies; }
        }

        public void Start(Dispatcher dispatcher)
        {
            var currenciesWire = new ReactiveBatchedPassThroughTable(_currencies, new WpfThreadMarshaller(dispatcher), _synchroniseTablesDelay);
            Task.Run(() => StartReceiving(currenciesWire, new Dictionary<string, int>
                                                              {
                                                                  {FxTableDefinitions.CurrencyPair.Id, 101},
                                                                  {FxTableDefinitions.CurrencyPair.CcyPair, 102},
                                                                  {FxTableDefinitions.CurrencyPair.Ccy1, 103},
                                                                  {FxTableDefinitions.CurrencyPair.Ccy2, 104},
                                                              }, 1337));

            var ratesWire = new ReactiveBatchedPassThroughTable(_fxRates, new WpfThreadMarshaller(dispatcher), _synchroniseTablesDelay);
            Task.Run(() => StartReceiving(ratesWire, new Dictionary<string, int>
                                                         {
                                                             {FxTableDefinitions.FxRates.CcyPairId, 101},
                                                             {FxTableDefinitions.FxRates.Bid, 102},
                                                             {FxTableDefinitions.FxRates.Ask, 103},
                                                             {FxTableDefinitions.FxRates.Time, 104},
                                                         }, 1338));
        }

        private static ReactiveTable GetCurrenciesTable()
        {
            ReactiveTable currencies = new ReactiveTable();
            currencies.AddColumn(new ReactiveColumn<int>(FxTableDefinitions.CurrencyPair.Id));
            currencies.AddColumn(new ReactiveColumn<string>(FxTableDefinitions.CurrencyPair.CcyPair));
            currencies.AddColumn(new ReactiveColumn<string>(FxTableDefinitions.CurrencyPair.Ccy1));
            currencies.AddColumn(new ReactiveColumn<string>(FxTableDefinitions.CurrencyPair.Ccy2));
            return currencies;
        }

        private static ReactiveTable GetRatesTable()
        {
            ReactiveTable fxRates = new ReactiveTable();
            fxRates.AddColumn(new ReactiveColumn<string>(FxTableDefinitions.FxRates.CcyPairId));
            fxRates.AddColumn(new ReactiveColumn<double>(FxTableDefinitions.FxRates.Bid));
            fxRates.AddColumn(new ReactiveColumn<double>(FxTableDefinitions.FxRates.Ask));
            fxRates.AddColumn(new ReactiveColumn<DateTime>(FxTableDefinitions.FxRates.Time));
            return fxRates;
        }

        private void StartReceiving(IWritableReactiveTable currenciesWire, Dictionary<string, int> columnsToFieldIds, int port)
        {
            var client = new TcpClient();
            _clients.Add(client);
            // TODO: Handle disconnections
            client.Connect(IPAddress.Loopback, port);
            using (var stream = client.GetStream())
            {
                var fieldIdsToColumns = InverseUniqueDictionary(columnsToFieldIds);
                var tableWriter = new ProtobufTableWriter(currenciesWire, fieldIdsToColumns, stream);
                _tableWriters.Add(tableWriter);
                tableWriter.Start();
            }
            //_client.Close();
        }

        private static Dictionary<TValue, TKey> InverseUniqueDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            var inverse = new Dictionary<TValue, TKey>(dictionary.Count);
            foreach (var value in dictionary)
            {
                inverse.Add(value.Value, value.Key);
            }
            return inverse;
        }

        public void Stop()
        {
            if (_tableWriters != null)
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
}
