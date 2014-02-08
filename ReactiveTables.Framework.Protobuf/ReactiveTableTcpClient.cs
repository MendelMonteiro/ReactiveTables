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
using ReactiveTables.Framework.Comms;
using ReactiveTables.Utils;

namespace ReactiveTables.Framework.Protobuf
{
    public class ReactiveTableTcpClient<TTable> where TTable : IReactiveTable
    {
        private readonly List<TcpClient> _clients = new List<TcpClient>();
        private readonly IReactiveTableProcessor<TTable> _reactiveTableProcessor;
        private readonly int _port;
        private readonly Dictionary<string, int> _columnsToFieldIds;
        private readonly TTable _wireTable;
        private readonly List<IReactiveTableProcessor<TTable>> _tableProcessors = new List<IReactiveTableProcessor<TTable>>();

        public ReactiveTableTcpClient(IReactiveTableProcessor<TTable> reactiveTableProcessor, TTable wireTable, 
            Dictionary<string, int> columnsToFieldIds, int port)
        {
            _columnsToFieldIds = columnsToFieldIds;
            _wireTable = wireTable;
            _reactiveTableProcessor = reactiveTableProcessor;
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
                _tableProcessors.Add(_reactiveTableProcessor);
                _reactiveTableProcessor.Setup(stream, _wireTable, new ProtobuffDecoderState(fieldIdsToColumns));
            }
            //_client.Close();
        }
    }
}