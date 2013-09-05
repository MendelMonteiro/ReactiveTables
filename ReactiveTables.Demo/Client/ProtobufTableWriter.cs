using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ProtoBuf;
using ReactiveTables.Demo.Server;
using ReactiveTables.Framework;

namespace ReactiveTables.Demo.Client
{
    class ProtobufTableWriter
    {
        private readonly IWritableReactiveTable _table;
        private readonly Dictionary<int, string> _fieldIdsToColumns;
        private readonly Stream _stream;
        private readonly ProtoReader _reader;
        readonly ManualResetEventSlim _finished = new ManualResetEventSlim();

        public ProtobufTableWriter(IWritableReactiveTable table, Dictionary<int, string> fieldIdsToColumns, Stream stream)
        {
            _table = table;
            _fieldIdsToColumns = fieldIdsToColumns;
            _stream = stream;
            _reader = new ProtoReader(_stream, null, null);
        }

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
                }
            }
        }

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
                    }
                }
                if (fieldId == ProtobufOperationTypes.Update)
                {
                    var token = ProtoReader.StartSubItem(_reader);

                    fieldId = _reader.ReadFieldHeader();
                    if (fieldId == ProtobufFieldIds.RowId) // Check for row id
                    {
                        var rowId = _reader.ReadInt32();

                        WriteFieldsToTable(_table, _fieldIdsToColumns, _reader, remoteToLocalRowIds[rowId]);
                    }

                    ProtoReader.EndSubItem(token, _reader);
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

        private void WriteFieldsToTable(IWritableReactiveTable table, Dictionary<int, string> fieldIdsToColumns,
                                        ProtoReader reader, int rowId)
        {
            int fieldId;
            while ((fieldId = reader.ReadFieldHeader()) != 0)
            {
                var columnId = fieldIdsToColumns[fieldId];
                var column = table.Columns[columnId];

                if (column.Type == typeof(int))
                {
                    table.SetValue(columnId, rowId, reader.ReadInt32());
                }
                else if (column.Type == typeof(short))
                {
                    table.SetValue(columnId, rowId, reader.ReadInt16());
                }
                else if (column.Type == typeof(string))
                {
                    table.SetValue(columnId, rowId, reader.ReadString());
                }
                else if (column.Type == typeof(bool))
                {
                    table.SetValue(columnId, rowId, reader.ReadBoolean());
                }
                else if (column.Type == typeof(double))
                {
                    table.SetValue(columnId, rowId, reader.ReadDouble());
                }
                else if (column.Type == typeof(long))
                {
                    table.SetValue(columnId, rowId, reader.ReadInt64());
                }
                else if (column.Type == typeof(decimal))
                {
                    table.SetValue(columnId, rowId, BclHelpers.ReadDecimal(reader));
                }
            }
        }
    }
}