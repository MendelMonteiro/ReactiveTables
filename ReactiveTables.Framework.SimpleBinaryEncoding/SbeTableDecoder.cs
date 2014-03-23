using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Adaptive.SimpleBinaryEncoding;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Comms;

namespace ReactiveTables.Framework.SimpleBinaryEncoding
{
    public class SbeTableDecoderState
    {
        public Dictionary<int, string> FieldIdsToColumns { get; set; }
    }

    public class SbeTableDecoder : IReactiveTableProcessor<IWritableReactiveTable>
    {
        private IWritableReactiveTable _table;
        private Stream _stream;
        private readonly ManualResetEventSlim _finished = new ManualResetEventSlim(false);
        private readonly byte[] _byteArray;
        private readonly DirectBuffer _buffer;
        private readonly MessageHeader _header;
        private readonly SbeTableUpdate _update;
        private const short MessageTemplateVersion = 0;
        private readonly byte[] _stringTempBuffer = new byte[ushort.MaxValue];

        public SbeTableDecoder()
        {
            _update = new SbeTableUpdate();
            _byteArray = new byte[1024];
            _buffer = new DirectBuffer(_byteArray);
            _header = new MessageHeader();
        }

        /// <summary>
        /// Note that this is a blocking call.
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="table"></param>
        /// <param name="state"></param>
        public void Setup(Stream outputStream, IWritableReactiveTable table, object state)
        {
            var config = (SbeTableDecoderState)state;
            var fieldIdsToColumns = config.FieldIdsToColumns;
            _table = table;
            _stream = outputStream;

            var remoteToLocalRowIds = new Dictionary<int, int>();

            _finished.Reset();
            while (!_finished.Wait(0))
            {
                ReadStream(_stream, remoteToLocalRowIds, fieldIdsToColumns);
            }
        }

        public void Stop()
        {
            _finished.Set();
        }

        private void ReadStream(Stream stream, Dictionary<int, int> remoteToLocalRowIds, Dictionary<int, string> fieldIdsToColumns)
        {
            int read;
            while ((read = stream.Read(_byteArray, 0, _byteArray.Length)) > 0)
            {
                int bufferOffset = 0;
                while (bufferOffset < read)
                {
                    if (bufferOffset + MessageHeader.Size > read) throw new ApplicationException("Not enough data read");
                    _header.Wrap(_buffer, bufferOffset, MessageTemplateVersion);

                    int actingBlockLength = _header.BlockLength;
                    int schemaId = _header.SchemaId;
                    int actingVersion = _header.Version;
                    bufferOffset += MessageHeader.Size;

                    if (bufferOffset + _update.Size > read) throw new ApplicationException("Not enough data read");
                    _update.WrapForDecode(_buffer, bufferOffset, actingBlockLength, actingVersion);

                    var operationType = _update.Type;

                    var rowId = _update.RowId;
                    bufferOffset += _update.Size;
                    if (operationType == OperationType.Add)
                    {
                        if (rowId >= 0)
                        {
                            remoteToLocalRowIds.Add(rowId, _table.AddRow());
                        }
                    }
                    else if (operationType == OperationType.Update)
                    {
                        var column = _table.Columns[fieldIdsToColumns[_update.FieldId]];
                        bufferOffset += WriteFieldsToTable(_table, column, rowId, _buffer, bufferOffset);
                    }
                    else if (operationType == OperationType.Delete)
                    {
                        if (rowId >= 0)
                        {
                            _table.DeleteRow(remoteToLocalRowIds[rowId]);
                            remoteToLocalRowIds.Remove(rowId);
                        }
                    }
                }
            }
        }

        private int WriteFieldsToTable(IWritableReactiveTable table, IReactiveColumn column, int rowId, DirectBuffer buffer, int bufferOffset)
        {
            var columnId = column.ColumnId;

            if (column.Type == typeof(int))
            {
                table.SetValue(columnId, rowId, buffer.Int32GetLittleEndian(bufferOffset));
                return sizeof(int);
            }
            else if (column.Type == typeof(short))
            {
                table.SetValue(columnId, rowId, buffer.Int16GetLittleEndian(bufferOffset));
                return sizeof(short);
            }
            else if (column.Type == typeof(string))
            {
                ushort stringLength = buffer.Uint16GetLittleEndian(bufferOffset);
                bufferOffset += sizeof(ushort);
                var bytesRead = buffer.GetBytes(bufferOffset, _stringTempBuffer, 0, stringLength);
                var value = Encoding.Default.GetString(_stringTempBuffer, 0, bytesRead);
                table.SetValue(columnId, rowId, value);
                return sizeof(ushort) + bytesRead;
            }
            else if (column.Type == typeof(bool))
            {
                byte b = buffer.CharGet(bufferOffset);
                table.SetValue(columnId, rowId, b == 1);
                return sizeof(byte);
            }
            else if (column.Type == typeof(double))
            {
                var value = buffer.DoubleGetLittleEndian(bufferOffset);
                table.SetValue(columnId, rowId, value);
                return sizeof(double);
            }
            else if (column.Type == typeof(long))
            {
                table.SetValue(columnId, rowId, buffer.Int64GetLittleEndian(bufferOffset));
                return sizeof(long);
            }
            else if (column.Type == typeof(decimal))
            {
                //                    table.SetValue(columnId, rowId, BclHelpers.ReadDecimal(reader));
            }
            else if (column.Type == typeof(DateTime))
            {
                //                    table.SetValue(columnId, rowId, BclHelpers.ReadDateTime(reader));
            }
            else if (column.Type == typeof(TimeSpan))
            {
                //                    table.SetValue(columnId, rowId, BclHelpers.ReadTimeSpan(reader));
            }
            else if (column.Type == typeof(Guid))
            {
                //                    table.SetValue(columnId, rowId, BclHelpers.ReadGuid(reader));
            }
            else if (column.Type == typeof(byte))
            {
                table.SetValue(columnId, rowId, buffer.CharGet(bufferOffset));
                return sizeof(byte);
            }
            else if (column.Type == typeof(char))
            {
                table.SetValue(columnId, rowId, (char)buffer.Uint16GetLittleEndian(bufferOffset));
                return sizeof(char);
            }
            else if (column.Type == typeof(float))
            {
                table.SetValue(columnId, rowId, buffer.FloatGetLittleEndian(bufferOffset));
                return sizeof(float);
            }
            return 0;
        }

        public void Dispose()
        {
            if (_buffer != null) _buffer.Dispose();
        }

        public static void TestDouble()
        {
            DirectBuffer b = new DirectBuffer(new byte[16]);
            var initial = 123.456;
            b.DoublePutLittleEndian(0, initial);

            var d = b.DoubleGetLittleEndian(0);
            
        }
    }
}
