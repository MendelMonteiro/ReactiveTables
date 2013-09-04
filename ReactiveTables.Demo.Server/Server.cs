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
using System.Threading.Tasks;
using ProtoBuf;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Demo.Server
{
    public static class FxColumns
    {
        public static class CurrencyPair
        {
            public static readonly string Id = "Ccy.Id";
            public static readonly string Ccy1 = "Ccy.Ccy1";
            public static readonly string Ccy2 = "Ccy.Ccy2";
            public static string CcyPair = "Ccy.CcyPairId";
        }

        public static class FxRates
        {
            public static readonly string CcyPairId = "Fx.CcyPairId";
            public static readonly string Bid = "Fx.Bid";
            public static readonly string Ask = "Fx.Ask";
            public static readonly string Time = "Fx.Time";
        }
    }

    internal class Server
    {
        private static void Main(string[] args)
        {
            Server server = new Server();
            server.Start();
        }

        private readonly Random _random = new Random();

        private void Start()
        {
            // Create data tables
            var currencies = GetCurrenciesTable();

            ReactiveTable fxRates = new ReactiveTable();
            fxRates.AddColumn(new ReactiveColumn<string>(FxColumns.FxRates.CcyPairId));
            fxRates.AddColumn(new ReactiveColumn<double>(FxColumns.FxRates.Bid));
            fxRates.AddColumn(new ReactiveColumn<double>(FxColumns.FxRates.Ask));
            fxRates.AddColumn(new ReactiveColumn<DateTime>(FxColumns.FxRates.Time));

            // Start threads for each one
            Task currenciesTask = Task.Factory.StartNew(StreamCurrencies, currencies);
            Task ratesTask = Task.Factory.StartNew(StreamRates, fxRates);

            currenciesTask.Wait();
            ratesTask.Wait();
        }

        private static ReactiveTable GetCurrenciesTable()
        {
            ReactiveTable currencies = new ReactiveTable();
            currencies.AddColumn(new ReactiveColumn<int>(FxColumns.CurrencyPair.Id));
            currencies.AddColumn(new ReactiveColumn<string>(FxColumns.CurrencyPair.CcyPair));
            currencies.AddColumn(new ReactiveColumn<string>(FxColumns.CurrencyPair.Ccy1));
            currencies.AddColumn(new ReactiveColumn<string>(FxColumns.CurrencyPair.Ccy2));
            return currencies;
        }

        private readonly string[] currencyList = new[] { "EUR", "GBP", "USD", "AUD", "CAD", "CHF", "NZD" };

        private void StreamCurrencies(object o)
        {
            ReactiveTable currencies = (ReactiveTable)o;

            var observerStream = new MemoryStream();
            Dictionary<string, int> columnsToFieldIds = new Dictionary<string, int>
                                                            {
                                                                {FxColumns.CurrencyPair.Id, 1},
                                                                {FxColumns.CurrencyPair.CcyPair, 2},
                                                                {FxColumns.CurrencyPair.Ccy1, 3},
                                                                {FxColumns.CurrencyPair.Ccy2, 4},
                                                            };
            using (var protoWriter = new ProtoWriter(observerStream, null, null))
            {
                currencies.Subscribe(new ProtobufWriterObserver(currencies, protoWriter, columnsToFieldIds));

                int ccyPairId = 1;
                for (int i = 0; i < currencyList.Length; i++)
                {
                    for (int j = i + 1; j < currencyList.Length; j++)
                    {
                        var ccy1 = currencyList[i];
                        var ccy2 = currencyList[j];
                        var rowId = currencies.AddRow();
                        currencies.SetValue(FxColumns.CurrencyPair.Id, rowId, ccyPairId++);
                        currencies.SetValue(FxColumns.CurrencyPair.CcyPair, rowId, ccy1 + ccy2);
                        currencies.SetValue(FxColumns.CurrencyPair.Ccy1, rowId, ccy1);
                        currencies.SetValue(FxColumns.CurrencyPair.Ccy2, rowId, ccy2);
                    }
                }
            }

            ResetStream(observerStream);
            //            ReadProtobuf(observerStream);
            ReactiveTable output = GetCurrenciesTable();
            ProtobufTableWriter tableWriter = new ProtobufTableWriter(output, InverseUniqueDictionary(columnsToFieldIds));
            tableWriter.WriteStream(observerStream);
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

        private void StreamRates(object o)
        {
            ReactiveTable fxRates = (ReactiveTable)o;

            for (int i = 0; i < currencyList.Length; i++)
            {
                for (int j = i + 1; j < currencyList.Length; j++)
                {
                    var ccy1 = currencyList[i];
                    var ccy2 = currencyList[j];

                    var rowId = fxRates.AddRow();
                    fxRates.SetValue(FxColumns.FxRates.CcyPairId, rowId, ccy1 + ccy2);
                    fxRates.SetValue(FxColumns.FxRates.Bid, rowId, GetRandomBidAsk());
                    fxRates.SetValue(FxColumns.FxRates.Ask, rowId, GetRandomBidAsk());
                    fxRates.SetValue(FxColumns.FxRates.Time, rowId, DateTime.UtcNow);
                }
            }
        }

        private double GetRandomBidAsk()
        {
            return _random.Next(1, 1000) / 500d;
        }
    }
}