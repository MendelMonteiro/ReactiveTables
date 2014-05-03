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
using ReactiveTables.Framework.UI;
using ReactiveTables.Framework.Utils;

namespace ReactiveTables.Framework
{
    /// <summary>
    /// Translates table updates into INotifyPropertyChanged events.  Used to centralise the subscriptions to a table.
    /// </summary>
    public class PropertyChangedNotifier : IObserver<TableUpdate>, IDisposable
    {
        private readonly Dictionary<int, HashSet<IReactivePropertyNotifiedConsumer>> _consumersByRowIndex =
            new Dictionary<int, HashSet<IReactivePropertyNotifiedConsumer>>();

        private readonly IDisposable _subscription;

        public PropertyChangedNotifier(IObservable<TableUpdate> table)
        {
            _subscription = table.Subscribe(this);
        }

        /// <summary>
        /// Register a property change consumer to this notifier
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="rowIndex">The row being watched</param>
        public void RegisterPropertyNotifiedConsumer(IReactivePropertyNotifiedConsumer consumer, int rowIndex)
        {
            var consumers = _consumersByRowIndex.GetOrAddNew(rowIndex);
            consumers.Add(consumer);
        }

        /// <summary>
        /// Unregister a property change consumer from this notifier
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="rowIndex">The row to no longer be watched</param>
        public void UnregisterPropertyNotifiedConsumer(IReactivePropertyNotifiedConsumer consumer, int rowIndex)
        {
            if (_consumersByRowIndex.ContainsKey(rowIndex))
            {
                _consumersByRowIndex[rowIndex].Remove(consumer);
            }
        }

        /// <summary>
        /// Stop watching the table for changes.
        /// </summary>
        public void Dispose()
        {
            _subscription.Dispose();
        }

        /// <summary>
        /// New table update has been received
        /// </summary>
        /// <param name="value"></param>
        public void OnNext(TableUpdate value)
        {
            if (value.IsRowUpdate()) return;

            HashSet<IReactivePropertyNotifiedConsumer> consumers;
            if (_consumersByRowIndex.TryGetValue(value.RowIndex, out consumers))
            {
                string propertyName = GetPropertyName(value.Column.ColumnId);
                foreach (var consumer in consumers)
                {
                    consumer.OnPropertyChanged(propertyName);
                }
            }
        }

        private static string GetPropertyName(string columnId)
        {
            return columnId.Substring(columnId.LastIndexOf('.') + 1);
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }
}