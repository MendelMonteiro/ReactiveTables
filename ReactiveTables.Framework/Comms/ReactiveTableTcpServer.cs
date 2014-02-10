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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ReactiveTables.Framework.Comms
{
    /// <summary>
    /// Handles a client connection and streaming the data encoded by the <see cref="IReactiveTableProcessor{TTable}"/> to the
    /// client.  TODO: Should implement much more efficient tcp connection to be able to handle many clients connecting.
    /// </summary>
    public class ReactiveTableTcpServer<TTable> where TTable : IReactiveTable
    {
        private readonly ManualResetEventSlim _finished;
        private readonly IPEndPoint _endPoint;
        private readonly Func<IReactiveTableProcessor<TTable>> _getEncoder;
        private readonly Action _testAction;
        private readonly Func<ReactiveClientSession, TTable> _getOutputTable;

        public ReactiveTableTcpServer(Func<IReactiveTableProcessor<TTable>> getEncoder, IPEndPoint endPoint, ManualResetEventSlim finished, 
                                      Func<ReactiveClientSession, TTable> getOutputTable, Action testAction = null)
        {
            _getEncoder = getEncoder;
            _endPoint = endPoint;
            _finished = finished;
            _testAction = testAction;
            _getOutputTable = getOutputTable;
        }

        /// <summary>
        /// Start waiting for incoming client connections - this is a blocking call
        /// </summary>
        /// <param name="encoderState"></param>
        public void Start(object encoderState)
        {
            TcpListener listener = new TcpListener(_endPoint);
            listener.Start();

            listener.BeginAcceptTcpClient(AcceptClient, new ClientState
                                                            {
                                                                Listener = listener,
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
            var output = _getOutputTable(session);
            using (var encoder = _getEncoder())
            {
                encoder.Setup(outputStream, output, state.EncoderState);

                outputStream.Flush();
                int millisecondsTimeout = _testAction == null ? -1 : 50;
                while (client.Connected && !_finished.Wait(millisecondsTimeout))
                {
                    // Run the test action every 50 milliseconds if it has been set.
                    if (_testAction != null) _testAction();
                    //outputStream.Flush();
                }
            }

            outputStream.Close();
            client.Close();
        }
    }

    public class TestTcpReadClient
    {
        private readonly IPEndPoint _endPoint;
        private readonly ManualResetEventSlim _finished;

        public TestTcpReadClient(IPEndPoint endPoint, ManualResetEventSlim finished)
        {
            _endPoint = endPoint;
            _finished = finished;
        }

        public void Start()
        {
            Console.WriteLine("Starting to listen to {0}", _endPoint);
            TcpListener listener = new TcpListener(_endPoint);
            listener.Start();

            listener.BeginAcceptTcpClient(AcceptClient, listener);

            _finished.Wait();
            listener.Stop();
        }

        private void AcceptClient(IAsyncResult ar)
        {
            Console.WriteLine("Accepted client");
            var listener = (TcpListener)ar.AsyncState;
            var client = listener.EndAcceptTcpClient(ar);
            var networkStream = client.GetStream();
            byte[] buffer = new byte[8];
            int read;

            Console.WriteLine("Waiting for data");
            while (!networkStream.DataAvailable)
            {
                if (_finished.Wait(100))
                {
                    networkStream.Close();
                    return;
                }
            }

            Console.WriteLine("Data available - reading...");
            do
            {
                //var b = networkStream.ReadByte();
                read = networkStream.Read(buffer, 0, buffer.Length);
                if (read > 0)
                {
                    Console.WriteLine("Received buffer of length {0}", read);
                    Console.WriteLine(Encoding.Default.GetString(buffer, 0, read));
                }
            } while (!_finished.Wait(100));
            networkStream.Close();
            Console.WriteLine("Finished with client");
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