using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
        private readonly Func<byte[], decimal> _bytesToDecimal;
        private readonly byte[] _decimalBuffer = new byte[sizeof(decimal)];

        public SbeTableDecoder(int bufferSize = 4096)
        {
            _update = new SbeTableUpdate();
            _byteArray = new byte[bufferSize];
            _buffer = new DirectBuffer(_byteArray);
            _header = new MessageHeader();

            // Jon skeet's delegeate to relfection hack https://msmvps.com/blogs/jon_skeet/archive/2008/08/09/making-reflection-fly-and-exploring-delegates.aspx
            var methodInfo = typeof(decimal).GetMethod("ToDecimal", BindingFlags.Static | BindingFlags.NonPublic);
            _bytesToDecimal = (Func<byte[], decimal>)Delegate.CreateDelegate(typeof(Func<byte[], decimal>), methodInfo);
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
            int read, tornMessageSize = 0;
            while ((read = stream.Read(_byteArray, tornMessageSize, _byteArray.Length - tornMessageSize)) > 0)
            {
                Debug.WriteLine("Received {0} bytes - previous read {1} bytes", read, tornMessageSize);
                read += tornMessageSize;
                tornMessageSize = 0;
                var bufferOffset = 0;
                while (bufferOffset < read)
                {
                    var messageStart = bufferOffset;
                    if (bufferOffset + MessageHeader.Size > read)
                    {
                        tornMessageSize = MoveTornMessage(read, messageStart);
                        break;
                    }

                    _header.Wrap(_buffer, bufferOffset, MessageTemplateVersion);
                    int actingBlockLength = _header.BlockLength;
                    Debug.Assert(actingBlockLength > 0 && actingBlockLength < 255, "Acting block length too long");
                    int schemaId = _header.SchemaId;
                    int actingVersion = _header.Version;
                    bufferOffset += MessageHeader.Size;

                    var realUpdateSize = (actingBlockLength - MessageHeader.Size);
                    if (bufferOffset + realUpdateSize > read)
                    {
                        tornMessageSize = MoveTornMessage(read, messageStart);
                        break;
                    }

                    _update.WrapForDecode(_buffer, bufferOffset, SbeTableUpdate.BlockLength, actingVersion);
                    var operationType = _update.Type;
                    var rowId = _update.RowId;
                    Debug.WriteLine("Reading message with length {0} at offset {2} for rowId {1}", actingBlockLength, rowId, bufferOffset);

                    if (operationType == OperationType.Add)
                    {
                        if (rowId >= 0)
                        {
                            Debug.WriteLine("Received new row {0}", rowId);
                            remoteToLocalRowIds.Add(rowId, _table.AddRow());
                        }
                    }
                    else if (operationType == OperationType.Update)
                    {
                        var colValOffset = bufferOffset + _update.Size;
                        var column = _table.GetColumnByName(fieldIdsToColumns[_update.FieldId]);
                        var written = WriteFieldsToTable(_table, column, rowId, _buffer, colValOffset, read);
                        if (written == -1)
                        {
                            tornMessageSize = MoveTornMessage(read, messageStart);
                            break;
                        }
                        Debug.Assert(written == actingBlockLength - MessageHeader.Size - _update.Size, "Written less than acting block length");
                        //bufferOffset += written;
                    }
                    else if (operationType == OperationType.Delete)
                    {
                        if (rowId >= 0)
                        {
                            _table.DeleteRow(remoteToLocalRowIds[rowId]);
                            remoteToLocalRowIds.Remove(rowId);
                        }
                    }

                    bufferOffset += actingBlockLength - MessageHeader.Size;
                }
            }
        }

        private int MoveTornMessage(int read, int messageStart)
        {
            var tornMessageSize = read - messageStart;
            Buffer.BlockCopy(_byteArray, messageStart, _byteArray, 0, tornMessageSize);
            Debug.WriteLine("Moved torn message of size {0} from buffer {1} of {2}", tornMessageSize, messageStart, read);
            return tornMessageSize;
        }

        private int WriteFieldsToTable(IWritableReactiveTable table, IReactiveColumn column, int rowId, DirectBuffer buffer, int bufferOffset, int read)
        {
            var columnId = column.ColumnId;
            var remaining = read - bufferOffset;

            if (column.Type == typeof(int))
            {
                if (remaining < sizeof (int)) return -1;

                table.SetValue(columnId, rowId, buffer.Int32GetLittleEndian(bufferOffset));
                return sizeof(int);
            }
            else if (column.Type == typeof(short))
            {
                if (remaining < sizeof(short)) return -1;
                table.SetValue(columnId, rowId, buffer.Int16GetLittleEndian(bufferOffset));
                return sizeof(short);
            }
            else if (column.Type == typeof(string))
            {
                if (remaining < sizeof(ushort)) return -1;
                var stringLength = buffer.Uint16GetLittleEndian(bufferOffset);

                bufferOffset += sizeof(ushort);
                remaining = read - bufferOffset;

                if (remaining < stringLength) return -1;
                var bytesRead = buffer.GetBytes(bufferOffset, _stringTempBuffer, 0, stringLength);
                var value = Encoding.Default.GetString(_stringTempBuffer, 0, bytesRead);
                table.SetValue(columnId, rowId, value);
                return sizeof(ushort) + bytesRead;
            }
            else if (column.Type == typeof(bool))
            {
                if (remaining < sizeof(byte)) return -1;
                var b = buffer.CharGet(bufferOffset);
                table.SetValue(columnId, rowId, b == 1);
                return sizeof(byte);
            }
            else if (column.Type == typeof(double))
            {
                if (remaining < sizeof(double)) return -1;
                var value = buffer.DoubleGetLittleEndian(bufferOffset);
                table.SetValue(columnId, rowId, value);
                return sizeof(double);
            }
            else if (column.Type == typeof(long))
            {
                if (remaining < sizeof(long)) return -1;
                table.SetValue(columnId, rowId, buffer.Int64GetLittleEndian(bufferOffset));
                return sizeof(long);
            }
            else if (column.Type == typeof(decimal))
            {
                if (remaining < sizeof(decimal)) return -1;
                Buffer.BlockCopy(_byteArray, bufferOffset, _decimalBuffer, 0, _decimalBuffer.Length);
                table.SetValue(columnId, rowId, _bytesToDecimal(_decimalBuffer));
                return _decimalBuffer.Length;
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
                if (remaining < sizeof(byte)) return -1;
                table.SetValue(columnId, rowId, buffer.CharGet(bufferOffset));
                return sizeof(byte);
            }
            else if (column.Type == typeof(char))
            {
                if (remaining < sizeof(char)) return -1;
                table.SetValue(columnId, rowId, (char)buffer.Uint16GetLittleEndian(bufferOffset));
                return sizeof(char);
            }
            else if (column.Type == typeof(float))
            {
                if (remaining < sizeof(float)) return -1;
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
            var b = new DirectBuffer(new byte[16]);
            var initial = 123.456;
            b.DoublePutLittleEndian(0, initial);

            var d = b.DoubleGetLittleEndian(0);
            
        }
    }
}
