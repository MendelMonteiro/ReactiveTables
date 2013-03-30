using System;
using System.Collections.Generic;

namespace ReactiveTables
{
    public interface IReactiveTable : IObservable<int>, ISubscribable<IObserver<int>>
    {
        IReactiveColumn<T> AddColumn<T>(IReactiveColumn<T> column);
        T GetValue<T>(string columnId, int rowIndex);
        void RegisterConsumer(IReactiveConsumer consumer, int rowIndex);
        void UnregisterConsumer(IReactiveConsumer consumer, int rowIndex);
        int RowCount { get; }
    }

    public interface IWritableReactiveTable : IReactiveTable
    {
        IReactiveColumn<T> AddColumn<T>(IReactiveColumn<T> column);
        T GetValue<T>(string columnId, int rowIndex);
        void SetValue<T>(string columnId, int rowIndex, T value);
        int AddRow();
        void RegisterConsumer(IReactiveConsumer consumer, int rowIndex);
        void UnregisterConsumer(IReactiveConsumer consumer, int rowIndex);
        int RowCount { get; }
    }

    public class ReactiveTable : IWritableReactiveTable
    {
        private readonly Dictionary<string, IReactiveColumn> _columns = new Dictionary<string, IReactiveColumn>();
        private int _lastRowIndex = -1;
        private readonly Dictionary<int, HashSet<IReactiveConsumer>> _consumersByRowIndex = new Dictionary<int, HashSet<IReactiveConsumer>>();

        public IReactiveColumn<T> AddColumn<T>(IReactiveColumn<T> column)
        {
            var columnId = column.ColumnId;
            _columns.Add(columnId, column);
            column.Subscribe(new ColumnChangePublisher<T>(this, column));
            return column;
        }

        private IReactiveColumn<T> GetColumn<T>(string columnId)
        {
            return (IReactiveColumn<T>) _columns[columnId];
        }

        public IReactiveField<T> GetField<T>(string columnId, int index)
        {
            return GetColumn<T>(columnId).GetValue(index);
        }

        public T GetValue<T>(string columnId, int rowIndex)
        {
            return GetColumn<T>(columnId).GetValue(rowIndex).Value;
        }

        public void SetValue<T>(string columnId, int rowIndex, T value)
        {
            GetColumn<T>(columnId).SetValue(rowIndex, value);
        }

        public int AddRow()
        {
            foreach (var column in _columns)
            {
                column.Value.AddField();
            }

            _lastRowIndex++;

            foreach (var observer in _observers)
            {
                observer.OnNext(_lastRowIndex);
            }
            return _lastRowIndex;
        }

        public void RegisterConsumer(IReactiveConsumer consumer, int rowIndex)
        {
            var consumers = _consumersByRowIndex.AddNewIfNotExists(rowIndex);
            consumers.Add(consumer);
        }

        public void UnregisterConsumer(IReactiveConsumer consumer, int rowIndex)
        {
            _consumersByRowIndex[rowIndex].Remove(consumer);
        }

        private void NotifyConsumers(string columnId, int rowIndex)
        {
            if (!_consumersByRowIndex.ContainsKey(rowIndex)) return;

            var consumers = _consumersByRowIndex[rowIndex];
            foreach (var consumer in consumers)
            {
                consumer.OnPropertyChanged(columnId);
            }
        }

        public int RowCount { get { return _lastRowIndex + 1; } }

        private class ColumnChangePublisher<T> : IColumnObserver<T>
        {
            private readonly ReactiveTable _reactiveTable;
            private readonly IReactiveColumn<T> _column;

            public ColumnChangePublisher(ReactiveTable reactiveTable, IReactiveColumn<T> column)
            {
                _reactiveTable = reactiveTable;
                _column = column;
            }

            public void OnNext(T value, int index)
            {
                _reactiveTable.NotifyConsumers(_column.ColumnId, index);
            }

            public void OnError(Exception error, int index)
            {
                throw new NotImplementedException();
            }

            public void OnCompleted(int index)
            {
                throw new NotImplementedException();
            }
        }

        private static readonly HashSet<IObserver<int>> _observers = new HashSet<IObserver<int>>();
        public IDisposable Subscribe(IObserver<int> observer)
        {
            _observers.Add(observer);
            return new SubscriptionToken<ReactiveTable, IObserver<int>>(this, observer);
        }

        public void Unsubscribe(IObserver<int> observer)
        {
            _observers.Remove(observer);
        }
    }
}