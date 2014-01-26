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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Comms;
using ReactiveTables.Framework.Protobuf;

namespace ReactiveTables.Demo.Server
{
    internal interface IServer
    {
        void Stop();
        void Start();
    }

    /// <summary>
    /// Demo server streams two tables to a client when it connects
    /// </summary>
    internal class Server : IServer
    {
        private static void Main(string[] args)
        {
            var servers = new List<IServer> {new Server(), new BrokerServer()};
            servers.ForEach(s => s.Start());
            Console.CancelKeyPress += (sender, eventArgs) => servers.ForEach(s => s.Stop());
            Console.WriteLine("Press Enter to stop server");
            Console.ReadKey();
            servers.ForEach(s => s.Stop());
        }

        public void Stop()
        {
            Console.WriteLine("Stopping service");
            _finished.Set();
        }

        private readonly string[] _currencyList = new[] { "EUR", "GBP", "USD", "AUD", "CAD", "CHF", "NZD", "CNY", "ZAR", "BRL", "RUB", "JPY", "INR", "DKK", "NOK", "PLN" };
        private readonly ManualResetEventSlim _finished = new ManualResetEventSlim();
        private readonly Random _random = new Random();
        private readonly DateTime _start = DateTime.Today;

        public void Start()
        {
            _finished.Reset();

            // Create data tables
            var currencies = GetCurrenciesTable();
            var fxRates = GetRatesTable();

            // Start threads for each one
            Task.Run(() => StreamCurrencies(currencies));
            Task.Run(() => StreamRates(fxRates));
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
            fxRates.AddColumn(new ReactiveColumn<DateTime>(FxTableDefinitions.FxRates.Time));
//            fxRates.AddColumn(new ReactiveColumn<long>(FxTableDefinitions.FxRates.Ticks));
            return fxRates;
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

        private void StreamCurrencies(object o)
        {
            ReactiveTable currencies = (ReactiveTable)o;

            int ccyPairId = 1;
            for (int i = 0; i < _currencyList.Length; i++)
            {
                for (int j = i + 1; j < _currencyList.Length; j++)
                {
                    var ccy1 = _currencyList[i];
                    var ccy2 = _currencyList[j];
                    var rowId = currencies.AddRow();
                    currencies.SetValue(FxTableDefinitions.CurrencyPair.Id, rowId, ccyPairId++);
                    currencies.SetValue(FxTableDefinitions.CurrencyPair.CcyPair, rowId, ccy1 + ccy2);
                    currencies.SetValue(FxTableDefinitions.CurrencyPair.Ccy1, rowId, ccy1);
                    currencies.SetValue(FxTableDefinitions.CurrencyPair.Ccy2, rowId, ccy2);
                }
            }

            ReactiveTableTcpServer server = new ReactiveTableTcpServer(new ProtobufTableEncoder(),
                                                                       new IPEndPoint(IPAddress.Loopback, (int)ServerPorts.Currencies), _finished);

            server.Start(currencies, new ProtobufEncoderState
                                         {
                                             ColumnsToFieldIds = new Dictionary<string, int>
                                                                     {
                                                                         {FxTableDefinitions.CurrencyPair.Id, 101},
                                                                         {FxTableDefinitions.CurrencyPair.CcyPair, 102},
                                                                         {FxTableDefinitions.CurrencyPair.Ccy1, 103},
                                                                         {FxTableDefinitions.CurrencyPair.Ccy2, 104},
                                                                     }
                                         });
        }

        private void StreamRates(object o)
        {
            ReactiveTable fxRates = (ReactiveTable)o;

            Dictionary<string, int> ccyPairsToRowIds = new Dictionary<string, int>();
            AddRates(fxRates, ccyPairsToRowIds);
            UpdateRates(ccyPairsToRowIds, fxRates);

            Action updateTable = () => UpdateRates(ccyPairsToRowIds, fxRates, false);
            ReactiveTableTcpServer server = new ReactiveTableTcpServer(new ProtobufTableEncoder(),
                                                                       new IPEndPoint(IPAddress.Loopback, (int)ServerPorts.FxRates), _finished, updateTable);
            server.Start(fxRates, new ProtobufEncoderState
                                      {
                                          ColumnsToFieldIds = FxTableDefinitions.FxRates.ColumnsToFieldIds
                                      });
        }
        
        private void AddRates(ReactiveTable fxRates, Dictionary<string, int> ccyPairsToRowIds)
        {
            for (int i = 0; i < _currencyList.Length; i++)
            {
                for (int j = i + 1; j < _currencyList.Length; j++)
                {
                    var ccy1 = _currencyList[i];
                    var ccy2 = _currencyList[j];

                    var rowId = fxRates.AddRow();
                    var ccyPair = ccy1 + ccy2;
                    ccyPairsToRowIds[ccyPair] = rowId;
                }
            }
        }

        private void UpdateRates(Dictionary<string, int> ccyPairsToRowIds, IWritableReactiveTable fxRates, bool full = true)
        {
            for (int i = 0; i < _currencyList.Length; i++)
            {
                for (int j = i + 1; j < _currencyList.Length; j++)
                {
                    var ccy1 = _currencyList[i];
                    var ccy2 = _currencyList[j];

                    var ccyPair = ccy1 + ccy2;
                    var rowId = ccyPairsToRowIds[ccyPair];
                    if (full) fxRates.SetValue(FxTableDefinitions.FxRates.CcyPairId, rowId, ccyPair);
                    fxRates.SetValue(FxTableDefinitions.FxRates.Bid, rowId, GetRandomBidAsk());
                    fxRates.SetValue(FxTableDefinitions.FxRates.Ask, rowId, GetRandomBidAsk());
                    fxRates.SetValue(FxTableDefinitions.FxRates.Open, rowId, GetRandomBidAsk());
                    fxRates.SetValue(FxTableDefinitions.FxRates.Close, rowId, GetRandomBidAsk());
                    fxRates.SetValue(FxTableDefinitions.FxRates.YearRangeStart, rowId, GetRandomBidAsk());
                    fxRates.SetValue(FxTableDefinitions.FxRates.YearRangeEnd, rowId, GetRandomBidAsk());
                    fxRates.SetValue(FxTableDefinitions.FxRates.Change, rowId, GetRandomBidAsk());
                    fxRates.SetValue(FxTableDefinitions.FxRates.Time, rowId, DateTime.UtcNow);
//                    var ticksElapsed = DateTime.Now.Ticks - _start.Ticks;
//                    fxRates.SetValue(FxTableDefinitions.FxRates.Ticks, rowId, ticksElapsed);
                }
            }
        }

        private double GetRandomBidAsk()
        {
            return _random.Next(1, 1000) / 500d;
        }
    }

    internal class ReplayStream : Stream
    {
        private readonly List<Stream> _streams = new List<Stream>();

        public void AddStream(Stream stream)
        {
            lock (_streams)
            {
                _streams.Add(stream);
            }
        }

        public void RemoveStream(Stream stream)
        {
            lock (_streams)
            {
                _streams.Remove(stream);
            }
        }

        public override void Flush()
        {
            if (_streams.Count == 0) return;
            Stream[] copy;
            lock (_streams)
            {
                copy = _streams.ToArray();
            }
            foreach (var stream in copy)
            {
                stream.Flush();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_streams.Count == 0) return;
            Stream[] copy;
            lock (_streams)
            {
                copy = _streams.ToArray();
            }
            foreach (var stream in copy)
            {
                stream.Write(buffer, offset, count);
            }
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position { get; set; }
    }
}