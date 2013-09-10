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
using ProtoBuf;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Comms.Protobuf
{
    /// <summary>
    /// Observes an <see cref="IReactiveTable"/> and replicates the updates to the given ProtoWriter.
    /// </summary>
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
                    WriteUpdate(_table.Columns.Values, value.RowIndex);
                    break;
                case TableUpdate.TableUpdateAction.Update:
                    WriteUpdate(value.Columns, value.RowIndex);
                    break;
                case TableUpdate.TableUpdateAction.Delete:
                    WriteDelete(value);
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

        private void WriteUpdate(IEnumerable<IReactiveColumn> columns, int rowIndex)
        {
            // Start the row group
            ProtoWriter.WriteFieldHeader(ProtobufOperationTypes.Update, WireType.StartGroup, _writer);
            var token = ProtoWriter.StartSubItem(rowIndex, _writer);

            var rowId = rowIndex;

            // Send the row id so that it can be matched against the local row id at the other end.
            ProtoWriter.WriteFieldHeader(ProtobufFieldIds.RowId, WireType.Variant, _writer);
            ProtoWriter.WriteInt32(rowId, _writer);

            foreach (var column in columns)
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
                var value = _table.GetValue<string>(column.ColumnId, rowId);
//                Console.WriteLine("Writing string {0}", value);
                ProtoWriter.WriteString(value ?? string.Empty, _writer);
            }
            else if (column.Type == typeof (bool))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Variant, _writer);
                ProtoWriter.WriteBoolean(_table.GetValue<bool>(column.ColumnId, rowId), _writer);
            }
            else if (column.Type == typeof (double))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.Fixed32, _writer);
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
            else if (column.Type == typeof (DateTime))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.StartGroup, _writer);
                BclHelpers.WriteDateTime(_table.GetValue<DateTime>(column.ColumnId, rowId), _writer);
            }
            else if (column.Type == typeof (TimeSpan))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.StartGroup, _writer);
                BclHelpers.WriteTimeSpan(_table.GetValue<TimeSpan>(column.ColumnId, rowId), _writer);
            }
            else if (column.Type == typeof (Guid))
            {
                ProtoWriter.WriteFieldHeader(fieldId, WireType.StartGroup, _writer);
                BclHelpers.WriteGuid(_table.GetValue<Guid>(column.ColumnId, rowId), _writer);
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