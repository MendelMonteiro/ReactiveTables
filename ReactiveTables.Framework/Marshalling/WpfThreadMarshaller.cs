/*This file is part of ReactiveTables.

ReactiveTables is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ReactiveTables is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Windows.Threading;

namespace ReactiveTables.Framework.Marshalling
{
    /// <summary>
    /// Marshals all actions onto the specified WPF Dispatcher.
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
