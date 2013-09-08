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
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Threading;
using ReactiveTables.Demo.Server;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Columns.Calculated;
using ReactiveTables.Framework.Comms.Protobuf;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Synchronisation;

namespace ReactiveTables.Demo.Client
{
    internal class FxDataService
    {
        private readonly ReactiveTable _currencies;
        private readonly List<ProtobufTableWriter> _tableWriters = new List<ProtobufTableWriter>();
        private readonly TimeSpan _synchroniseTablesDelay = TimeSpan.FromMilliseconds(300);
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

        public static class CalculateColumns
        {
            public static class FxRates
            {
                public const string LongTime = "FxRates.LongTime";
            } 
        }

        public void Start(Dispatcher dispatcher)
        {
            var currenciesWire = new ReactiveBatchedPassThroughTable(_currencies, new WpfThreadMarshaller(dispatcher),
                                                                     _synchroniseTablesDelay);
            Task.Run(() => StartReceiving(currenciesWire, FxTableDefinitions.CurrencyPair.ColumnsToFieldIds, 1337));

            var ratesWire = new ReactiveBatchedPassThroughTable(_fxRates, new WpfThreadMarshaller(dispatcher), _synchroniseTablesDelay);
            Task.Run(() => StartReceiving(ratesWire, FxTableDefinitions.FxRates.ColumnsToFieldIds, 1338));
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
            fxRates.AddColumn(new ReactiveColumn<double>(FxTableDefinitions.FxRates.Open));
            fxRates.AddColumn(new ReactiveColumn<double>(FxTableDefinitions.FxRates.Close));
            fxRates.AddColumn(new ReactiveColumn<double>(FxTableDefinitions.FxRates.YearRangeStart));
            fxRates.AddColumn(new ReactiveColumn<double>(FxTableDefinitions.FxRates.YearRangeEnd));
            fxRates.AddColumn(new ReactiveColumn<double>(FxTableDefinitions.FxRates.Change));
            var timeColumn = new ReactiveColumn<DateTime>(FxTableDefinitions.FxRates.Time);
            fxRates.AddColumn(timeColumn);
            fxRates.AddColumn(new ReactiveCalculatedColumn1<string, DateTime>(CalculateColumns.FxRates.LongTime,
                                                                              timeColumn,
                                                                              time => time.ToString("HH:mm:ss:fffff")));
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