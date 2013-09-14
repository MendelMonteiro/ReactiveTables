﻿// This file is part of ReactiveTables.
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
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Protobuf;
using ReactiveTables.Framework.Synchronisation;
using ReactiveTables.Utils;

namespace ReactiveTables.Demo.Client
{
    internal class FxDataService
    {
        public static class CalculateColumns
        {
            public static class FxRates
            {
                public const string LongTime = "FxRates.LongTime";
                public const string Micros = "FxRates.Micros";
            } 
        }

        private readonly ReactiveTable _currencies;
        private readonly List<ProtobufTableDecoder> _tableWriters = new List<ProtobufTableDecoder>();
        private readonly TimeSpan _synchroniseTablesDelay = TimeSpan.FromMilliseconds(200);
        private readonly List<TcpClient> _clients = new List<TcpClient>();
        private readonly ReactiveTable _fxRates;
        private static readonly DateTime _start = DateTime.Today;

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
//            var tickColumn = new ReactiveColumn<long>(FxTableDefinitions.FxRates.Ticks);
//            fxRates.AddColumn(tickColumn);
            fxRates.AddColumn(new ReactiveCalculatedColumn1<double, DateTime>(CalculateColumns.FxRates.LongTime,
                                                                              timeColumn,
                                                                              time => (DateTime.UtcNow - time).TotalMilliseconds));
            /*fxRates.AddColumn(new ReactiveCalculatedColumn1<long, long>(CalculateColumns.FxRates.Micros,
                                                                        tickColumn,
                                                                        tickStart =>
                                                                        {
                                                                            var ticksElapsed = DateTime.Now.Ticks - _start.Ticks;
                                                                            return (ticksElapsed - tickStart) * 10;
                                                                        }));*/
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
                var fieldIdsToColumns = columnsToFieldIds.InverseUniqueDictionary();
                var tableDecoder = new ProtobufTableDecoder(currenciesWire, fieldIdsToColumns, stream);
                _tableWriters.Add(tableDecoder);
                tableDecoder.Start();
            }
            //_client.Close();
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