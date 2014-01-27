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

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ReactiveTables.Utils;

namespace ReactiveTables.Framework.Protobuf
{
    public class ReactiveTableTcpClient
    {
        private readonly List<TcpClient> _clients = new List<TcpClient>();
        private readonly int _port;
        private readonly Dictionary<string, int> _columnsToFieldIds;
        private readonly IWritableReactiveTable _wireTable;
        private readonly List<ProtobufTableDecoder> _tableWriters = new List<ProtobufTableDecoder>();

        public ReactiveTableTcpClient(IWritableReactiveTable wireTable, Dictionary<string, int> columnsToFieldIds, int port)
        {
            _columnsToFieldIds = columnsToFieldIds;
            _wireTable = wireTable;
            _port = port;
        }

        public void Start()
        {
            var client = new TcpClient();
            _clients.Add(client);
            // TODO: Handle disconnections
            client.Connect(IPAddress.Loopback, _port);
            using (var stream = client.GetStream())
            {
                //                FileStream file = new FileStream("broker-output.bin", FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                //                stream.CopyTo(file);

                var fieldIdsToColumns = _columnsToFieldIds.InverseUniqueDictionary();
                var tableDecoder = new ProtobufTableDecoder(_wireTable, fieldIdsToColumns, stream);
                _tableWriters.Add(tableDecoder);
                tableDecoder.Start();
            }
            //_client.Close();
        }
    }
}