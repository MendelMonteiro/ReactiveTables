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
        private readonly ReactiveTable _rates = new ReactiveTable();
        private readonly ReactiveTable _currencies;
        private ProtobufTableWriter _tableWriter;
        private readonly TimeSpan _synchroniseTablesDelay = TimeSpan.FromMilliseconds(500);

        public FxDataService()
        {
            _currencies = GetCurrenciesTable();
        }

        public ReactiveTable Rates
        {
            get { return _rates; }
        }

        public ReactiveTable Currencies
        {
            get { return _currencies; }
        }

        public void Start(Dispatcher dispatcher)
        {
            var currenciesWire = new ReactiveBatchedPassThroughTable(_currencies, new WpfThreadMarshaller(dispatcher), _synchroniseTablesDelay);
            Task.Run(() => StartReceiving(currenciesWire));
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

        private void StartReceiving(IWritableReactiveTable currenciesWire)
        {
            using (var client = new TcpClient())
            {
                client.Connect(IPAddress.Loopback, 1337);
                using (var stream = client.GetStream())
                {
                    var columnsToFieldIds = new Dictionary<string, int>
                                                {
                                                    {FxTableDefinitions.CurrencyPair.Id, 1},
                                                    {FxTableDefinitions.CurrencyPair.CcyPair, 2},
                                                    {FxTableDefinitions.CurrencyPair.Ccy1, 3},
                                                    {FxTableDefinitions.CurrencyPair.Ccy2, 4},
                                                };

                    var fieldIdsToColumns = InverseUniqueDictionary(columnsToFieldIds);
                    _tableWriter = new ProtobufTableWriter(currenciesWire, fieldIdsToColumns, stream);
                    _tableWriter.Start();
                }
            }
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
            if (_tableWriter != null)
            {
                _tableWriter.Stop();
            }
        }
    }
}
