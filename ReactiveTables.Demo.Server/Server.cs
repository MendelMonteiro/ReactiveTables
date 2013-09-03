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
                var stream = new MemoryStream();
                using (var writer = new ProtoWriter(stream, null, null))
                {
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

                            ProtoWriter.WriteFieldHeader(1, WireType.StartGroup, writer);
                            var token = ProtoWriter.StartSubItem(rowId, writer);

                            ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
                            ProtoWriter.WriteInt32(currencies.GetValue<int>(FxColumns.CurrencyPair.Id, rowId), writer);

                            ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
                            ProtoWriter.WriteString(currencies.GetValue<string>(FxColumns.CurrencyPair.CcyPair, rowId), writer);

                            ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
                            ProtoWriter.WriteString(currencies.GetValue<string>(FxColumns.CurrencyPair.Ccy1, rowId), writer);

                            ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
                            ProtoWriter.WriteString(currencies.GetValue<string>(FxColumns.CurrencyPair.Ccy2, rowId), writer);

                            ProtoWriter.EndSubItem(token, writer);
                        }
                    }
                }

                ResetStream(stream);
                //                ReadProtobuf(stream);
            }

            ResetStream(observerStream);
            //            ReadProtobuf(observerStream);
            ReactiveTable output = GetCurrenciesTable();
            WriteStreamToTable(observerStream, output, InverseUniqueDictionary(columnsToFieldIds));
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

        private void WriteStreamToTable(Stream stream, ReactiveTable table, Dictionary<int, string> fieldIdsToColumns)
        {
            Dictionary<int, int> remoteToLocalRowIds = new Dictionary<int, int>();
            using (var reader = new ProtoReader(stream, null, null))
            {
                int fieldId;
                while ((fieldId = reader.ReadFieldHeader()) != 0)
                {
                    if (fieldId == ProtobufOperationTypes.Add)
                    {
                        var rowId = ReadRowId(reader);
                        if (rowId >= 0)
                        {
                            remoteToLocalRowIds.Add(rowId, table.AddRow());
                        }
                    }
                    if (fieldId == ProtobufOperationTypes.Update)
                    {
                        var token = ProtoReader.StartSubItem(reader);

                        fieldId = reader.ReadFieldHeader();
                        if (fieldId == ProtobufFieldIds.RowId) // Check for row id
                        {
                            var rowId = reader.ReadInt32();

                            WriteFieldsToTable(table, fieldIdsToColumns, reader, remoteToLocalRowIds[rowId]);
                        }

                        ProtoReader.EndSubItem(token, reader);
                    }
                    else if (fieldId == ProtobufOperationTypes.Delete)
                    {
                        var rowId = ReadRowId(reader);
                        if (rowId >= 0)
                        {
                            table.DeleteRow(remoteToLocalRowIds[rowId]);
                            remoteToLocalRowIds.Remove(rowId);
                        }
                    }
                }
            }
        }

        private static int ReadRowId(ProtoReader reader)
        {
            var token = ProtoReader.StartSubItem(reader);
            var fieldId = reader.ReadFieldHeader();
            if (fieldId != ProtobufFieldIds.RowId) return -1;

            var rowId = reader.ReadInt32();
            fieldId = reader.ReadFieldHeader();
            ProtoReader.EndSubItem(token, reader);
            return rowId;
        }

        private void WriteFieldsToTable(ReactiveTable table, Dictionary<int, string> fieldIdsToColumns, 
                                        ProtoReader reader, int rowId)
        {
            int fieldId;
            while ((fieldId = reader.ReadFieldHeader()) != 0)
            {
                var columnId = fieldIdsToColumns[fieldId];
                var column = table.Columns[columnId];

                if (column.Type == typeof (int))
                {
                    table.SetValue(columnId, rowId, reader.ReadInt32());
                }
                else if (column.Type == typeof (short))
                {
                    table.SetValue(columnId, rowId, reader.ReadInt16());
                }
                else if (column.Type == typeof (string))
                {
                    table.SetValue(columnId, rowId, reader.ReadString());
                }
                else if (column.Type == typeof (bool))
                {
                    table.SetValue(columnId, rowId, reader.ReadBoolean());
                }
                else if (column.Type == typeof (double))
                {
                    table.SetValue(columnId, rowId, reader.ReadDouble());
                }
                else if (column.Type == typeof (long))
                {
                    table.SetValue(columnId, rowId, reader.ReadInt64());
                }
                else if (column.Type == typeof (decimal))
                {
                    table.SetValue(columnId, rowId, BclHelpers.ReadDecimal(reader));
                }
            }
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

    public static class ProtobufOperationTypes
    {
        public const int Update = 1;
        public const int Add = 2;
        public const int Delete = 3;
    }

    public static class ProtobufFieldIds
    {
        public const int RowId = short.MaxValue;
    }

    internal class ProtobufWriterObserver : IObserver<TableUpdate>
    {
        private readonly Dictionary<string, int> _columnsToFieldIds;
        private readonly ProtoWriter _writer;
        private readonly IReactiveTable _table;

        public ProtobufWriterObserver(IReactiveTable table, ProtoWriter writer, Dictionary<string, int> columnsToFieldIds)
        {
            _table = table;
            _writer = writer;
            _columnsToFieldIds = columnsToFieldIds;
        }

        public void OnNext(TableUpdate value)
        {
            switch (value.Action)
            {
                case TableUpdate.TableUpdateAction.Add:
                    WriteAdd(value);
                    break;
                case TableUpdate.TableUpdateAction.Update:
                    WriteUpdate(value);
                    break;
                case TableUpdate.TableUpdateAction.Delete:
//                    WriteDelete(value);
                    break;
            }
        }

        private void WriteAdd(TableUpdate value)
        {
            WriteRowId(value, ProtobufOperationTypes.Add);
        }

        private void WriteDelete(TableUpdate value)
        {
            WriteRowId(value, ProtobufOperationTypes.Delete);
        }

        private void WriteUpdate(TableUpdate value)
        {
            ProtoWriter.WriteFieldHeader(ProtobufOperationTypes.Update, WireType.StartGroup, _writer);
            var token = ProtoWriter.StartSubItem(value.RowIndex, _writer);

            var rowId = value.RowIndex;

            // Send the row id so that it can be matched against the local row id at the other end.
            ProtoWriter.WriteFieldHeader(ProtobufFieldIds.RowId, WireType.Variant, _writer);
            ProtoWriter.WriteInt32(rowId, _writer);

            foreach (var column in value.Columns)
            {
                var fieldId = _columnsToFieldIds[column.ColumnId];

                WriteColumn(column, fieldId, rowId);
            }

            ProtoWriter.EndSubItem(token, _writer);
        }

        private void WriteColumn(IReactiveColumn column, int fieldId, int rowId)
        {
            if (column.Type == typeof (int))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Variant, _writer);
                ProtoWriter.WriteInt32(_table.GetValue<int>(column.ColumnId, rowId), _writer);
            }
            else if (column.Type == typeof (short))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Variant, _writer);
                ProtoWriter.WriteInt16(_table.GetValue<short>(column.ColumnId, rowId), _writer);
            }
            else if (column.Type == typeof (string))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.String, _writer);
                ProtoWriter.WriteString(_table.GetValue<string>(column.ColumnId, rowId), _writer);
            }
            else if (column.Type == typeof (bool))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Variant, _writer);
                ProtoWriter.WriteBoolean(_table.GetValue<bool>(column.ColumnId, rowId), _writer);
            }
            else if (column.Type == typeof (double))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Variant, _writer);
                ProtoWriter.WriteDouble(_table.GetValue<double>(column.ColumnId, rowId), _writer);
            }
            else if (column.Type == typeof (long))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Variant, _writer);
                ProtoWriter.WriteInt64(_table.GetValue<long>(column.ColumnId, rowId), _writer);
            }
            else if (column.Type == typeof (decimal))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.StartGroup, _writer);
                BclHelpers.WriteDecimal(_table.GetValue<decimal>(column.ColumnId, rowId), _writer);
            }
        }

        private void WriteRowId(TableUpdate value, int operationType)
        {
            ProtoWriter.WriteFieldHeader(operationType, WireType.StartGroup, _writer);
            var token = ProtoWriter.StartSubItem(value.RowIndex, _writer);
            ProtoWriter.WriteFieldHeader(ProtobufFieldIds.RowId, WireType.Variant, _writer);
            ProtoWriter.WriteInt32(value.RowIndex, _writer);
            ProtoWriter.EndSubItem(token, _writer);
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }
}