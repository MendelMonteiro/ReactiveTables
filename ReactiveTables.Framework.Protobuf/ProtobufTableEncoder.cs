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
using System.IO;
using ProtoBuf;
using ReactiveTables.Framework.Comms;

namespace ReactiveTables.Framework.Protobuf
{
    /// <summary>
    /// Encodes a <see cref="IReactiveTable"/> by observing all changes and writing them to the given stream
    /// using the protobuf protocol.
    /// </summary>
    public class ProtobufTableEncoder : IReactiveTableEncoder
    {
        private IDisposable _token;
        private ProtoWriter _protoWriter;

        public void Setup(Stream outputStream, IReactiveTable table, object state)
        {
            var config = (ProtobufEncoderState) state;
            // TODO: Find way to re-utilise the proto writers (object pool?)
            _protoWriter = new ProtoWriter(outputStream, null, null);
            var writerObserver = new ProtobufWriterObserver(table, _protoWriter, config.ColumnsToFieldIds);
            _token = table.Subscribe(writerObserver);
            // Replay state when new client connects
            table.ReplayRows(writerObserver);
        }

        public void Close()
        {
            if (_token != null) _token.Dispose();
            if (_protoWriter != null) _protoWriter.Close();
        }

        public void Dispose()
        {
            Close();
        }
    }
}