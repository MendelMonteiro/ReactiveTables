using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using ReactiveTables.Framework;

namespace ReactiveTables.Demo.Server
{
    class ProtobufTableWriter
    {
        private readonly ReactiveTable _table;
        private readonly Dictionary<int, string> _fieldIdsToColumns;

        public ProtobufTableWriter(ReactiveTable table, Dictionary<int, string> fieldIdsToColumns)
        {
            _table = table;
            _fieldIdsToColumns = fieldIdsToColumns;
        }

        public void WriteStream(Stream stream)
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
                            remoteToLocalRowIds.Add(rowId, _table.AddRow());
                        }
                    }
                    if (fieldId == ProtobufOperationTypes.Update)
                    {
                        var token = ProtoReader.StartSubItem(reader);

                        fieldId = reader.ReadFieldHeader();
                        if (fieldId == ProtobufFieldIds.RowId) // Check for row id
                        {
                            var rowId = reader.ReadInt32();

                            WriteFieldsToTable(_table, _fieldIdsToColumns, reader, remoteToLocalRowIds[rowId]);
                        }

                        ProtoReader.EndSubItem(token, reader);
                    }
                    else if (fieldId == ProtobufOperationTypes.Delete)
                    {
                        var rowId = ReadRowId(reader);
                        if (rowId >= 0)
                        {
                            _table.DeleteRow(remoteToLocalRowIds[rowId]);
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