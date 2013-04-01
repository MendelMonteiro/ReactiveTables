using System;
using System.Collections.Generic;
using ReactiveTables.Framework.UI;
using ReactiveTables.Utils;

namespace ReactiveTables.Framework
{
    /// <summary>
    /// Maybe we should store an instance of this on the table directly?
    /// </summary>
    public class PropertyChangedNotifier : IObserver<ColumnUpdate>
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
            _consumersByRowIndex[rowIndex].Remove(consumer);
            _subscription.Dispose();
        }

        public void OnNext(ColumnUpdate value)
        {
            HashSet<IReactivePropertyNotifiedConsumer> consumers;
            if (_consumersByRowIndex.TryGetValue(value.RowIndex, out consumers))
            {
                foreach (var consumer in consumers)
                {
                    consumer.OnPropertyChanged(GetPropertyName(value.Column.ColumnId));
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