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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Demo.Server
{
    internal class Server
    {
        private static void Main(string[] args)
        {
            Server server = new Server();
            server.Start();
            Console.CancelKeyPress += (sender, eventArgs) => server.Stop();
            Console.WriteLine("Press Enter to stop server");
            Console.ReadKey();
            server.Stop();
        }

        private void Stop()
        {
            Console.WriteLine("Stopping service");
            _finished.Set();
        }

        private readonly string[] currencyList = new[] { "EUR", "GBP", "USD", "AUD", "CAD", "CHF", "NZD" };
        private readonly ManualResetEventSlim _finished = new ManualResetEventSlim();
        private readonly Random _random = new Random();

        private void Start()
        {
            _finished.Reset();

            // Create data tables
            var currencies = GetCurrenciesTable();

            ReactiveTable fxRates = new ReactiveTable();
            fxRates.AddColumn(new ReactiveColumn<string>(FxTableDefinitions.FxRates.CcyPairId));
            fxRates.AddColumn(new ReactiveColumn<double>(FxTableDefinitions.FxRates.Bid));
            fxRates.AddColumn(new ReactiveColumn<double>(FxTableDefinitions.FxRates.Ask));
            fxRates.AddColumn(new ReactiveColumn<DateTime>(FxTableDefinitions.FxRates.Time));

            // Start threads for each one
            Task.Run(() => StreamCurrencies(currencies));
            Task.Run(() => StreamRates(fxRates));
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

            var observerStream = new MemoryStream();

            int ccyPairId = 1;
            for (int i = 0; i < currencyList.Length; i++)
            {
                for (int j = i + 1; j < currencyList.Length; j++)
                {
                    var ccy1 = currencyList[i];
                    var ccy2 = currencyList[j];
                    var rowId = currencies.AddRow();
                    currencies.SetValue(FxTableDefinitions.CurrencyPair.Id, rowId, ccyPairId++);
                    currencies.SetValue(FxTableDefinitions.CurrencyPair.CcyPair, rowId, ccy1 + ccy2);
                    currencies.SetValue(FxTableDefinitions.CurrencyPair.Ccy1, rowId, ccy1);
                    currencies.SetValue(FxTableDefinitions.CurrencyPair.Ccy2, rowId, ccy2);
                }
            }

            //ResetStream(observerStream);

            TcpListener listener = new TcpListener(IPAddress.Loopback, 1337);
            listener.Start();
            listener.BeginAcceptTcpClient(ClientAccepted,
                                          new ClientState
                                              {
                                                  Listener = listener,
                                                  Stream = observerStream,
                                                  Table = currencies
                                              });
            //            Console.WriteLine("Waiting for client connection");
            //            AcceptClientAndWriteTable(listener, observerStream, protoWriter);

            _finished.Wait();
            listener.Stop();
        }

        private static void AcceptClientAndWriteTable(TcpListener listener, MemoryStream observerStream, ProtoWriter protoWriter)
        {
            while (!listener.Pending())
            {
                Thread.Sleep(10);
            }

            using (var client = listener.AcceptTcpClient())
            using (var outputStream = client.GetStream())
            {
                observerStream.CopyTo(outputStream);

                outputStream.Close();
                client.Close();
            }

            protoWriter.Close();
        }

        private class ClientState
        {
            public TcpListener Listener { get; set; }
            public Stream Stream { get; set; }
            public IWritableReactiveTable Table { get; set; }
        }

        private void ClientAccepted(IAsyncResult ar)
        {
            var state = (ClientState)ar.AsyncState;
            Console.WriteLine("Client connection accepted");
            var listener = state.Listener;
            // TODO: Keep connection open and stream changes
            using (var client = listener.EndAcceptTcpClient(ar))
            using (var outputStream = client.GetStream())
            {
                var columnsToFieldIds = new Dictionary<string, int>
                                            {
                                                {FxTableDefinitions.CurrencyPair.Id, 101},
                                                {FxTableDefinitions.CurrencyPair.CcyPair, 102},
                                                {FxTableDefinitions.CurrencyPair.Ccy1, 103},
                                                {FxTableDefinitions.CurrencyPair.Ccy2, 104},
                                            };
                var protoWriter = new ProtoWriter(outputStream, null, null);
                var protobufWriterObserver = new ProtobufWriterObserver(state.Table, protoWriter, columnsToFieldIds);
                var token = state.Table.Subscribe(protobufWriterObserver);
                
                state.Table.ReplayRows(protobufWriterObserver);

                var last = state.Table.AddRow();
                state.Table.SetValue(FxTableDefinitions.CurrencyPair.CcyPair, last, "Test");

                protoWriter.Close();
                outputStream.Flush();
                while (client.Connected && !_finished.Wait(10)) { }

                token.Dispose();
                outputStream.Close();
                client.Close();
            }
        }

        private static void ResetStream(MemoryStream stream)
        {
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
        }

        private Dictionary<TValue, TKey> InverseUniqueDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            Dictionary<TValue, TKey> inverse = new Dictionary<TValue, TKey>(dictionary.Count);
            foreach (var value in dictionary)
            {
                inverse.Add(value.Value, value.Key);
            }
            return inverse;
        }

        private void ReadProtobuf(Stream stream)
        {
            using (ProtoReader reader = new ProtoReader(stream, null, null))
            {
                int field;
                while ((field = reader.ReadFieldHeader()) != 0)
                {
                    if (field == 1)
                    {
                        var token = ProtoReader.StartSubItem(reader);
                        while ((field = reader.ReadFieldHeader()) != 0)
                        {
                            object val;
                            switch (field)
                            {
                                case 1:
                                    val = reader.ReadInt32();
                                    break;
                                case 2:
                                    val = reader.ReadString();
                                    break;
                                case 3:
                                    val = reader.ReadString();
                                    break;
                                case 4:
                                    val = reader.ReadString();
                                    break;
                                default:
                                    val = null;
                                    reader.SkipField();
                                    break;
                            }
                            Console.WriteLine("Value is {0}", val);
                        }
                        ProtoReader.EndSubItem(token, reader);
                    }
                    else
                    {
                        reader.SkipField();
                    }
                }
            }
        }

        private async void StreamRates(object o)
        {
            ReactiveTable fxRates = (ReactiveTable)o;

            //            var observerStream = new ReplayStream();
            Dictionary<string, int> columnsToFieldIds = new Dictionary<string, int>
                                                            {
                                                                {FxTableDefinitions.FxRates.CcyPairId, 1},
                                                                {FxTableDefinitions.FxRates.Bid, 2},
                                                                {FxTableDefinitions.FxRates.Ask, 3},
                                                                {FxTableDefinitions.FxRates.Time, 4},
                                                            };
            //            var protoWriter = new ProtoWriter(observerStream, null, null);
            //            var protobufWriterObserver = new ProtobufWriterObserver(fxRates, protoWriter, columnsToFieldIds);
            //            fxRates.Subscribe(protobufWriterObserver);

            Dictionary<string, int> ccyPairsToRowIds = new Dictionary<string, int>();
            AddRates(fxRates, ccyPairsToRowIds);
            UpdateRates(ccyPairsToRowIds, fxRates);

            //ResetStream(observerStream);
            TcpListener listener = TcpListener.Create(1337);
            listener.Start();
            while (!_finished.Wait(10))
            {
                if (listener.Pending())
                {
                    var client = await listener.AcceptTcpClientAsync();
                    var outputStream = client.GetStream();

                    // TODO: Find way to re-utilise the proto writers
                    var protoWriter2 = new ProtoWriter(outputStream, null, null);
                    var replayer = new ProtobufWriterObserver(fxRates, protoWriter2, columnsToFieldIds);
                    // Replay state when new client connects
                    fxRates.ReplayRows(replayer);
                }

                UpdateRates(ccyPairsToRowIds, fxRates);
            }

            listener.Stop();
            //            protoWriter.Close();
        }

        public class ReplayStream : Stream
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

        private void AddRates(ReactiveTable fxRates, Dictionary<string, int> ccyPairsToRowIds)
        {
            for (int i = 0; i < currencyList.Length; i++)
            {
                for (int j = i + 1; j < currencyList.Length; j++)
                {
                    var ccy1 = currencyList[i];
                    var ccy2 = currencyList[j];

                    var rowId = fxRates.AddRow();
                    var ccyPair = ccy1 + ccy2;
                    ccyPairsToRowIds[ccyPair] = rowId;
                }
            }
        }

        private void UpdateRates(Dictionary<string, int> ccyPairsToRowIds, ReactiveTable fxRates)
        {
            for (int i = 0; i < currencyList.Length; i++)
            {
                for (int j = i + 1; j < currencyList.Length; j++)
                {
                    var ccy1 = currencyList[i];
                    var ccy2 = currencyList[j];

                    var ccyPair = ccy1 + ccy2;
                    var rowId = ccyPairsToRowIds[ccyPair];
                    fxRates.SetValue(FxTableDefinitions.FxRates.CcyPairId, rowId, ccyPair);
                    fxRates.SetValue(FxTableDefinitions.FxRates.Bid, rowId, GetRandomBidAsk());
                    fxRates.SetValue(FxTableDefinitions.FxRates.Ask, rowId, GetRandomBidAsk());
                    fxRates.SetValue(FxTableDefinitions.FxRates.Time, rowId, DateTime.UtcNow);
                }
            }
        }

        private double GetRandomBidAsk()
        {
            return _random.Next(1, 1000) / 500d;
        }
    }
}