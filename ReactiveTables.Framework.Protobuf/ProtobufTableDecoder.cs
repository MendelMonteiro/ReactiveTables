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
using System.Threading;
using ProtoBuf;

namespace ReactiveTables.Framework.Protobuf
{
    /// <summary>
    /// Writes changes from the given protobuf stream to an <see cref="IWritableReactiveTable"/>.
    /// </summary>
    public class ProtobufTableDecoder
    {
        private readonly IWritableReactiveTable _table;
        private readonly Dictionary<int, string> _fieldIdsToColumns;
        private readonly Stream _stream;
        private readonly ProtoReader _reader;
        private readonly ManualResetEventSlim _finished = new ManualResetEventSlim();

        public ProtobufTableDecoder(IWritableReactiveTable table, Dictionary<int, string> fieldIdsToColumns, Stream stream)
        {
            _table = table;
            _fieldIdsToColumns = fieldIdsToColumns;
            _stream = stream;
            _reader = new ProtoReader(_stream, null, null);
        }

        /// <summary>
        /// Start listening for changes on the stream and writing to the table
        /// </summary>
        public void Start()
        {
            var remoteToLocalRowIds = new Dictionary<int, int>();

            _finished.Reset();
            while (!_finished.Wait(10))
            {
                try
                {
                    ReadStream(remoteToLocalRowIds);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Stop listening to updates from the stream
        /// </summary>
        public void Stop()
        {
            _finished.Set();
        }

        private void ReadStream(Dictionary<int, int> remoteToLocalRowIds)
        {
            int fieldId;
            while ((fieldId = _reader.ReadFieldHeader()) != 0)
            {
                if (fieldId == ProtobufOperationTypes.Add)
                {
                    var rowId = ReadRowId(_reader);
                    if (rowId >= 0)
                    {
                        remoteToLocalRowIds.Add(rowId, _table.AddRow());
                        fieldId = _reader.ReadFieldHeader();
                        ReadUpdate(remoteToLocalRowIds);
                    }
                }
                else if (fieldId == ProtobufOperationTypes.Update)
                {
                    ReadUpdate(remoteToLocalRowIds);
                }
                else if (fieldId == ProtobufOperationTypes.Delete)
                {
                    var rowId = ReadRowId(_reader);
                    if (rowId >= 0)
                    {
                        _table.DeleteRow(remoteToLocalRowIds[rowId]);
                        remoteToLocalRowIds.Remove(rowId);
                    }
                }
            }
        }

        private void ReadUpdate(Dictionary<int, int> remoteToLocalRowIds)
        {
            var token = ProtoReader.StartSubItem(_reader);

            int fieldId = _reader.ReadFieldHeader();
            if (fieldId == ProtobufFieldIds.RowId) // Check for row id
            {
                var rowId = _reader.ReadInt32();

                WriteFieldsToTable(_table, _fieldIdsToColumns, _reader, remoteToLocalRowIds[rowId]);
            }

            ProtoReader.EndSubItem(token, _reader);
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

        private void WriteFieldsToTable(IWritableReactiveTable table,
                                        Dictionary<int, string> fieldIdsToColumns,
                                        ProtoReader reader,
                                        int rowId)
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
                    var value = reader.ReadString();
//                    Console.WriteLine("Writing string {0}", value);
                    table.SetValue(columnId, rowId, value);
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
                else if (column.Type == typeof (DateTime))
                {
                    table.SetValue(columnId, rowId, BclHelpers.ReadDateTime(reader));
                }
                else if (column.Type == typeof (TimeSpan))
                {
                    table.SetValue(columnId, rowId, BclHelpers.ReadTimeSpan(reader));
                }
                else if (column.Type == typeof (Guid))
                {
                    table.SetValue(columnId, rowId, BclHelpers.ReadGuid(reader));
                }
                else if (column.Type == typeof(byte))
                {
                    table.SetValue(columnId, rowId, reader.ReadByte());
                }
                else if (column.Type == typeof(char))
                {
                    table.SetValue(columnId, rowId, (char)reader.ReadInt16());
                }
                else if (column.Type == typeof(float))
                {
                    table.SetValue(columnId, rowId, reader.ReadSingle());
                }
            }
        }
    }
}