using System;
using System.Windows.Threading;

namespace ReactiveTables
{
    internal class WpfThreadMarshaller : IThreadMarshaller
    {
        private readonly Dispatcher _dispatcher;

        public WpfThreadMarshaller(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Dispatch(Action action)
        {
            _dispatcher.BeginInvoke(action);
        }
    }
}