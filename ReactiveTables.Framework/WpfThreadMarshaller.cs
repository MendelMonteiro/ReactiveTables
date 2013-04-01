using System;
using System.Windows.Threading;

namespace ReactiveTables.Framework
{
    /// <summary>
    /// TODO: Move this to a GUI specific class library
    /// </summary>
    public class WpfThreadMarshaller : IThreadMarshaller
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