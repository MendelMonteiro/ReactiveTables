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
using ProtoBuf;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Protobuf
{
    /// <summary>
    /// Observes an <see cref="IReactiveTable"/> and replicates the updates to the given ProtoWriter.
    /// </summary>
    class ProtobufWriterObserver : IObserver<TableUpdate>
    {
        private readonly Dictionary<string, int> _columnsToFieldIds;
        private readonly IReactiveTable _table;
        private readonly Stream _outputStream;
        private bool WithLengthPrefix = true;

        public ProtobufWriterObserver(IReactiveTable table, Stream outputStream, Dictionary<string, int> columnsToFieldIds)
        {
            _table = table;
            _outputStream = outputStream;
            _columnsToFieldIds = columnsToFieldIds;
        }

        public void OnNext(TableUpdate value)
        {
            // TODO: Find way to re-utilise the proto writers (object pool?)
            using (var writer = ProtoWriter.Create(_outputStream, null, null))
            {
                var outerToken = new SubItemToken();
                if (WithLengthPrefix)
                {
                    // Encode the length of the stream (protobuf-net will automatically calculate the length of the 'String' field)
                    ProtoWriter.WriteFieldHeader(ProtobufOperationTypes.MessageSize, WireType.String, writer);
                    outerToken = ProtoWriter.StartSubItem(ProtobufOperationTypes.MessageSize, writer);
                }
                switch (value.Action)
                {
                    case TableUpdateAction.Add:
                        WriteAdd(writer, value);
                        WriteUpdates(writer, _table.Columns, value.RowIndex);
                        break;
                    case TableUpdateAction.Update:
                        WriteUpdate(writer, value.Column, value.RowIndex);
                        break;
                    case TableUpdateAction.Delete:
                        WriteDelete(writer, value);
                        break;
                }
                if (WithLengthPrefix)
                {
                    ProtoWriter.EndSubItem(outerToken, writer);
                }
            }
        }

        private void WriteAdd(ProtoWriter writer, TableUpdate value)
        {
            WriteRowId(writer, value, ProtobufOperationTypes.Add);
        }

        private void WriteDelete(ProtoWriter writer, TableUpdate value)
        {
            WriteRowId(writer, value, ProtobufOperationTypes.Delete);
        }

        private void WriteUpdates(ProtoWriter writer, IEnumerable<IReactiveColumn> columns, int rowIndex)
        {
            int rowId;
            var token = WriteStartUpdate(writer, rowIndex, out rowId);

            foreach (var column in columns)
            {
                // Only write columns for which we have mappings defined (gives the consumer a way to filter which columns are written to stream)
                int fieldId;
                if (_columnsToFieldIds.TryGetValue(column.ColumnId, out fieldId))
                {
                    WriteColumn(writer, column, fieldId, rowId);
                }
            }

            WriteEndUpdate(writer, token);
        }

        private void WriteUpdate(ProtoWriter writer, IReactiveColumn column, int rowIndex)
        {
            int rowId;
            var token = WriteStartUpdate(writer, rowIndex, out rowId);

            // Only write columns for which we have mappings defined (gives the consumer a way to filter which columns are written to stream)
            int fieldId;
            if (_columnsToFieldIds.TryGetValue(column.ColumnId, out fieldId))
            {
                WriteColumn(writer, column, fieldId, rowId);
            }

            WriteEndUpdate(writer, token);
        }

        private static void WriteEndUpdate(ProtoWriter writer, SubItemToken token)
        {
            ProtoWriter.EndSubItem(token, writer);
        }

        private static SubItemToken WriteStartUpdate(ProtoWriter writer, int rowIndex, out int rowId)
        {
            // Start the row group
            ProtoWriter.WriteFieldHeader(ProtobufOperationTypes.Update, WireType.StartGroup, writer);
            var token = ProtoWriter.StartSubItem(rowIndex, writer);

            rowId = rowIndex;

            // Send the row id so that it can be matched against the local row id at the other end.
            ProtoWriter.WriteFieldHeader(ProtobufFieldIds.RowId, WireType.Variant, writer);
            ProtoWriter.WriteInt32(rowId, writer);
            return token;
        }

        private void WriteColumn(ProtoWriter writer, IReactiveColumn column, int fieldId, int rowId)
        {
            if (column.Type == typeof (int))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Variant, writer);
                ProtoWriter.WriteInt32(_table.GetValue<int>(column.ColumnId, rowId), writer);
            }
            else if (column.Type == typeof (short))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Variant, writer);
                ProtoWriter.WriteInt16(_table.GetValue<short>(column.ColumnId, rowId), writer);
            }
            else if (column.Type == typeof (string))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.String, writer);
                var value = _table.GetValue<string>(column.ColumnId, rowId);
//                Console.WriteLine("Writing string {0}", value);
                ProtoWriter.WriteString(value ?? string.Empty, writer);
            }
            else if (column.Type == typeof (bool))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Variant, writer);
                ProtoWriter.WriteBoolean(_table.GetValue<bool>(column.ColumnId, rowId), writer);
            }
            else if (column.Type == typeof (double))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Fixed64, writer);
                ProtoWriter.WriteDouble(_table.GetValue<double>(column.ColumnId, rowId), writer);
            }
            else if (column.Type == typeof (long))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Variant, writer);
                ProtoWriter.WriteInt64(_table.GetValue<long>(column.ColumnId, rowId), writer);
            }
            else if (column.Type == typeof (decimal))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.StartGroup, writer);
                BclHelpers.WriteDecimal(_table.GetValue<decimal>(column.ColumnId, rowId), writer);
            }
            else if (column.Type == typeof (DateTime))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.StartGroup, writer);
                BclHelpers.WriteDateTime(_table.GetValue<DateTime>(column.ColumnId, rowId), writer);
            }
            else if (column.Type == typeof (TimeSpan))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.StartGroup, writer);
                BclHelpers.WriteTimeSpan(_table.GetValue<TimeSpan>(column.ColumnId, rowId), writer);
            }
            else if (column.Type == typeof (Guid))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.StartGroup, writer);
                BclHelpers.WriteGuid(_table.GetValue<Guid>(column.ColumnId, rowId), writer);
            }
            else if (column.Type == typeof(float))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Fixed32, writer);
                ProtoWriter.WriteSingle(_table.GetValue<float>(column.ColumnId, rowId), writer);
            }
            else if (column.Type == typeof(byte))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Variant, writer);
                ProtoWriter.WriteByte(_table.GetValue<byte>(column.ColumnId, rowId), writer);
            }
            else if (column.Type == typeof(char))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Variant, writer);
                ProtoWriter.WriteInt16((short)_table.GetValue<char>(column.ColumnId, rowId), writer);
            }
        }

        private void WriteRowId(ProtoWriter writer, TableUpdate value, int operationType)
        {
            ProtoWriter.WriteFieldHeader(operationType, WireType.StartGroup, writer);
            var token = ProtoWriter.StartSubItem(value.RowIndex, writer);
            ProtoWriter.WriteFieldHeader(ProtobufFieldIds.RowId, WireType.Variant, writer);
            ProtoWriter.WriteInt32(value.RowIndex, writer);
            ProtoWriter.EndSubItem(token, writer);
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