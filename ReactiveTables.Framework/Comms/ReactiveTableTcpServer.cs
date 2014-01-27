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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ReactiveTables.Framework.Comms
{
    /// <summary>
    /// Handles a client connection and streaming the data encoded by the <see cref="IReactiveTableEncoder"/> to the
    /// client.  TODO: Should implement much more efficient tcp connection to be able to handle many clients connecting.
    /// </summary>
    public class ReactiveTableTcpServer
    {
        private readonly ManualResetEventSlim _finished;
        private readonly IPEndPoint _endPoint;
        private readonly IReactiveTableEncoder _encoder;
        private readonly Action _testAction;
        private readonly Func<ReactiveClientSession, IReactiveTable> _source;

        public ReactiveTableTcpServer(IReactiveTableEncoder encoder, IPEndPoint endPoint, ManualResetEventSlim finished, 
                                      Func<ReactiveClientSession, IReactiveTable> source, Action testAction = null)
        {
            _encoder = encoder;
            _endPoint = endPoint;
            _finished = finished;
            _testAction = testAction;
            _source = source;
        }

        /// <summary>
        /// Start waiting for incoming client connections - this is a blocking call
        /// </summary>
        /// <param name="table"></param>
        /// <param name="encoderState"></param>
        public void Start(IReactiveTable table, object encoderState)
        {
            TcpListener listener = new TcpListener(_endPoint);
            listener.Start();

            listener.BeginAcceptTcpClient(AcceptClient, new ClientState
                                                            {
                                                                Listener = listener,
                                                                Table = table,
                                                                EncoderState = encoderState
                                                            });

            _finished.Wait();
            listener.Stop();
        }

        /// <summary>
        /// When a client connects to the server we need to stream changes to it every 50ms
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptClient(IAsyncResult ar)
        {
            var state = (ClientState) ar.AsyncState;
            var client = state.Listener.EndAcceptTcpClient(ar);
            var outputStream = client.GetStream();
            
            var session = new ReactiveClientSession {RemoteEndPoint = (IPEndPoint) client.Client.RemoteEndPoint};
            IReactiveTable output = _source(session);
            _encoder.Setup(outputStream, output, state.EncoderState);

            outputStream.Flush();
            int millisecondsTimeout = _testAction == null ? -1 : 50;
            while (client.Connected && !_finished.Wait(millisecondsTimeout))
            {
                // Run the test action every 50 milliseconds if it has been set.
                if (_testAction != null) _testAction();
                //outputStream.Flush();
            }

            _encoder.Close();

            outputStream.Close();
            client.Close();
        }
    }

    public class ReactiveClientSession
    {
        public IPEndPoint RemoteEndPoint { get; set; } 
    }

    public interface ITcpServerDataSource
    {
        IReactiveTable GetOutputTable(ReactiveClientSession session);
    }
}