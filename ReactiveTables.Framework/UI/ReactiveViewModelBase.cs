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

using System.ComponentModel;

namespace ReactiveTables.Framework.UI
{
    /// <summary>
    /// Can consume property changed events
    /// </summary>
    public interface IReactivePropertyNotifiedConsumer
    {
        void OnPropertyChanged(string propertyName);
    }

    /// <summary>
    /// A base view model class that can consume property changed events and broadcast them as <see cref="INotifyPropertyChanged"/> events
    /// </summary>
    public class ReactiveViewModelBase : INotifyPropertyChanged, IReactivePropertyNotifiedConsumer
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
//            else Console.WriteLine("No handler for property " + propertyName);
        }
    }
}