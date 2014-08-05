using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reflection;
using System.Text;
using Adaptive.SimpleBinaryEncoding;
using ReactiveTables.Framework.Comms;

namespace ReactiveTables.Framework.SimpleBinaryEncoding
{
    public class SbeTableEncoderState
    {
        public Dictionary<string, int> ColumnsToFieldIds { get; set; } 
    }

    public class SbeTableEncoder : IReactiveTableProcessor<IReactiveTable>
    {
        private IDisposable _token;
        private Stream _outputStream;
        private readonly SbeTableUpdate _update;
        private readonly DirectBuffer _buffer;
        private readonly MessageHeader _header;
        private readonly byte[] _byteArray;
        private readonly byte[] _stringBuffer = new byte[ushort.MaxValue];
        private readonly byte[] _decimalBuffer = new byte[sizeof(decimal)];
        private readonly Action<decimal, byte[]> _decimalGetBytes;
        private const short MessageTemplateVersion = 0;

        public SbeTableEncoder(int bufferSize = 4096)
        {
            _update = new SbeTableUpdate();
            _byteArray = new byte[bufferSize];
            _buffer = new DirectBuffer(_byteArray);
            _header = new MessageHeader();

            // Jon skeet's delegeate to relfection hack https://msmvps.com/blogs/jon_skeet/archive/2008/08/09/making-reflection-fly-and-exploring-delegates.aspx
            var methodInfo = typeof (decimal).GetMethod("GetBytes", BindingFlags.Static | BindingFlags.NonPublic);
            _decimalGetBytes = (Action<decimal, byte[]>) Delegate.CreateDelegate(typeof (Action<decimal, byte[]>), methodInfo);
        }

        public void Setup(Stream outputStream, IReactiveTable table, object state)
        {
            _outputStream = outputStream;
            var encodeState = (SbeTableEncoderState) state;
            _token = table.Subscribe(u => OnTableUpdate(u, table, encodeState));
            // Replay state when new client connects
            table.ReplayRows(new AnonymousObserver<TableUpdate>(tableUpdate => OnTableUpdate(tableUpdate, table, encodeState)));
        }

        private void OnTableUpdate(TableUpdate tableUpdate, IReactiveTable table, SbeTableEncoderState encodeState)
        {
            int offset = 0;

            _header.Wrap(_buffer, offset, MessageTemplateVersion);
            _header.SchemaId = SbeTableUpdate.SchemaId;
            _header.TemplateId = SbeTableUpdate.TemplateId;   // identifier for the table update object (SBE template ID)
            _header.Version = SbeTableUpdate.Schema_Version; // this can be overriden if we want to support different versions of the table update object (advanced functionality)
            offset += MessageHeader.Size;
            
            _update.WrapForEncode(_buffer, offset);
            _update.Type = MapType(tableUpdate.Action);
            _update.RowId = tableUpdate.RowIndex;
            offset += _update.Size;

            if (tableUpdate.Action == TableUpdateAction.Update)
            {
                int fieldId;
                if (!encodeState.ColumnsToFieldIds.TryGetValue(tableUpdate.Column.ColumnId, out fieldId))
                {
                    return;
                }

                _update.FieldId = fieldId;
                offset += WriteUpdateValue(table, tableUpdate, _buffer, offset);
            }

            _header.BlockLength = (ushort) offset; // size that a table update takes on the wire
            Debug.WriteLine("Writing block length of {0} for row {1}", _header.BlockLength, _update.RowId);

            _outputStream.Write(_byteArray, 0, offset);

            // Write all the columns after each add.
            if (tableUpdate.Action == TableUpdateAction.Add)
            {
                Debug.WriteLine("Sent new row {0}", _update.RowId);
                foreach (var columnId in encodeState.ColumnsToFieldIds.Keys)
                {
                    OnTableUpdate(new TableUpdate(TableUpdateAction.Update, tableUpdate.RowIndex, table.GetColumnByName(columnId)),
                                  table,
                                  encodeState);
                }
            }
        }

        private int WriteUpdateValue(IReactiveTable table, TableUpdate tableUpdate, DirectBuffer buffer, int offset)
        {
            var column = tableUpdate.Column;
            if (column.Type == typeof(int))
            {
                buffer.Int32PutLittleEndian(offset, table.GetValue<int>(column.ColumnId, tableUpdate.RowIndex));
                return sizeof(int);
            }
            if (column.Type == typeof(short))
            {
                buffer.Int16PutLittleEndian(offset, table.GetValue<short>(column.ColumnId, tableUpdate.RowIndex));
                return sizeof(short);
            }
            if (column.Type == typeof(string))
            {
                var stringValue = table.GetValue<string>(column.ColumnId, tableUpdate.RowIndex) ?? string.Empty;
                // Set the length
                var length = (ushort) stringValue.Length;
                buffer.Uint16PutLittleEndian(offset, length);
                int written = sizeof(ushort);
                offset += written;
                // And then the value
                var bytesWritten = Encoding.Default.GetBytes(stringValue, 0, stringValue.Length, _stringBuffer, 0);
                _buffer.SetBytes(offset, _stringBuffer, 0, bytesWritten);
                written += bytesWritten;
                //written += WriteStringToByteArray(stringValue, _byteArray, offset);
                return written;
            }
            if (column.Type == typeof(bool))
            {
                buffer.CharPut(offset, (byte) (table.GetValue<bool>(column.ColumnId, tableUpdate.RowIndex) ? 1 : 0));
                return sizeof(byte);
            }
            if (column.Type == typeof(double))
            {
                var value = table.GetValue<double>(column.ColumnId, tableUpdate.RowIndex);
                buffer.DoublePutLittleEndian(offset, value);
                return sizeof(double);
            }
            if (column.Type == typeof(long))
            {
                buffer.Int64PutLittleEndian(offset, table.GetValue<long>(column.ColumnId, tableUpdate.RowIndex));
                return sizeof(long);
            }
            if (column.Type == typeof(decimal))
            {
                var decimalVal = table.GetValue<decimal>(column.ColumnId, tableUpdate.RowIndex);
                _decimalGetBytes(decimalVal, _decimalBuffer);
                buffer.SetBytes(offset, _decimalBuffer, 0, _decimalBuffer.Length);
                return _decimalBuffer.Length;
            }
            if (column.Type == typeof(DateTime))
            {
                throw new NotImplementedException();
            }
            if (column.Type == typeof(TimeSpan))
            {
                throw new NotImplementedException();
            }
            if (column.Type == typeof(Guid))
            {
                throw new NotImplementedException();
            }
            if (column.Type == typeof(float))
            {
                buffer.FloatPutLittleEndian(offset, table.GetValue<float>(column.ColumnId, tableUpdate.RowIndex));
                return sizeof(float);
            }
            if (column.Type == typeof(byte))
            {
                buffer.CharPut(offset, table.GetValue<byte>(column.ColumnId, tableUpdate.RowIndex));
                return sizeof(byte);
            }
            if (column.Type == typeof(char))
            {
                buffer.Uint16PutLittleEndian(offset, table.GetValue<char>(column.ColumnId, tableUpdate.RowIndex));
                return sizeof(char);
            }
            return 0;
        }

        /*unsafe static void WriteDecimal(decimal d, byte[] buffer, int bufferOffset)
        {
            
        }*/

        private static OperationType MapType(TableUpdateAction action)
        {
            switch (action)
            {
                case TableUpdateAction.Add:
                    return OperationType.Add;
                case TableUpdateAction.Update:
                    return OperationType.Update;
                case TableUpdateAction.Delete:
                    return OperationType.Delete;
                default:
                    throw new ArgumentOutOfRangeException("action");
            }
        }

        public void Dispose()
        {
            if (_buffer != null) _buffer.Dispose();
            if (_token != null) _token.Dispose();
        }
    }
}
