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
using System.Net;
using System.Net.Sockets;

namespace ReactiveTables.Framework.Comms
{
    public class ReactiveTableTcpClient<TTable> : IDisposable where TTable : IReactiveTable
    {
        private readonly List<TcpClient> _clients = new List<TcpClient>();
        private readonly IReactiveTableProcessor<TTable> _reactiveTableProcessor;
        private readonly TTable _wireTable;
        private readonly List<IReactiveTableProcessor<TTable>> _tableProcessors = new List<IReactiveTableProcessor<TTable>>();
        private readonly object _processorState;
        private readonly IPEndPoint _endPoint;

        public ReactiveTableTcpClient(IReactiveTableProcessor<TTable> reactiveTableProcessor, TTable wireTable, object processorState, IPEndPoint endPoint)
        {
            _reactiveTableProcessor = reactiveTableProcessor;
            _wireTable = wireTable;
            _processorState = processorState;
            _endPoint = endPoint;
        }

        public void Start()
        {
            var client = new TcpClient();
            _clients.Add(client);
            // TODO: Handle disconnections
            client.Connect(_endPoint);

            Stream stream = client.GetStream();

            _tableProcessors.Add(_reactiveTableProcessor);
            _reactiveTableProcessor.Setup(stream, _wireTable, _processorState);

            //_client.Close();
        }

        public void Dispose()
        {
            foreach (var processor in _tableProcessors)
            {
                processor.Dispose();
            }
            foreach (var client in _clients)
            {
                client.Close();
            }
        }
    }
}