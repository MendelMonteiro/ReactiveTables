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
using System.Collections.Generic;
using ReactiveTables.Framework.UI;
using ReactiveTables.Utils;

namespace ReactiveTables.Framework
{
    /// <summary>
    /// Maybe we should store an instance of this on the table directly?
    /// </summary>
    public class PropertyChangedNotifier : IObserver<ColumnUpdate>, IDisposable
    {
        private readonly Dictionary<int, HashSet<IReactivePropertyNotifiedConsumer>> _consumersByRowIndex = new Dictionary<int, HashSet<IReactivePropertyNotifiedConsumer>>();
        private readonly IDisposable _subscription;

        public PropertyChangedNotifier(IObservable<ColumnUpdate> table)
        {
            _subscription = table.Subscribe(this);
        }

        public void RegisterPropertyNotifiedConsumer(IReactivePropertyNotifiedConsumer consumer, int rowIndex)
        {
            var consumers = _consumersByRowIndex.AddNewIfNotExists(rowIndex);
            consumers.Add(consumer);
        }

        public void UnregisterPropertyNotifiedConsumer(IReactivePropertyNotifiedConsumer consumer, int rowIndex)
        {
            if (_consumersByRowIndex.ContainsKey(rowIndex))
            {
                _consumersByRowIndex[rowIndex].Remove(consumer);
            }
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        public void OnNext(ColumnUpdate value)
        {
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
