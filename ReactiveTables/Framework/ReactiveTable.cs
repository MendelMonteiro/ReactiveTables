using System;
using System.Collections.Generic;

namespace ReactiveTables.Framework
{
    public interface IReactiveTable : IObservable<int>, ISubscribable<IObserver<int>>, IObservable<ColumnUpdate>, ISubscribable<IObserver<ColumnUpdate>>
    {
        IReactiveColumn AddColumn(IReactiveColumn column);
        T GetValue<T>(string columnId, int rowIndex);
        void RegisterPropertyNotifiedConsumer(IReactivePropertyNotifiedConsumer consumer, int rowIndex);
        void UnregisterPropertyNotifiedConsumer(IReactivePropertyNotifiedConsumer consumer, int rowIndex);
        int RowCount { get; }
        Dictionary<string, IReactiveColumn> Columns { get; }
    }

    public interface IWritableReactiveTable : IReactiveTable
    {
        void SetValue<T>(string columnId, int rowIndex, T value);
        void SetValue(string columnId, int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex);
        int AddRow();
    }

    public class ReactiveTable : IWritableReactiveTable
    {
        private readonly Dictionary<string, IReactiveColumn> _columns = new Dictionary<string, IReactiveColumn>();
        private int _lastRowIndex = -1;
        private readonly Dictionary<int, HashSet<IReactivePropertyNotifiedConsumer>> _consumersByRowIndex = new Dictionary<int, HashSet<IReactivePropertyNotifiedConsumer>>();
        private readonly HashSet<IObserver<int>> _rowObservers = new HashSet<IObserver<int>>();
        private readonly HashSet<IObserver<ColumnUpdate>> _columnObservers = new HashSet<IObserver<ColumnUpdate>>();

        public IReactiveColumn AddColumn(IReactiveColumn column)
        {
            var columnId = column.ColumnId;
            Columns.Add(columnId, column);
            column.Subscribe(new ColumnChangePublisher(this, column));
            return column;
        }

        private IReactiveColumn<T> GetColumn<T>(string columnId)
        {
            return (IReactiveColumn<T>) Columns[columnId];
        }

        private IReactiveField<T> GetField<T>(string columnId, int index)
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

        public void SetValue(string columnId, int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex)
        {
            _columns[columnId].CopyValue(rowIndex, sourceColumn, sourceRowIndex);
        }

        public int AddRow()
        {
            foreach (var column in Columns)
            {
                column.Value.AddField();
            }

            _lastRowIndex++;

            foreach (var observer in _rowObservers)
            {
                observer.OnNext(_lastRowIndex);
            }
            return _lastRowIndex;
        }

        public void RegisterPropertyNotifiedConsumer(IReactivePropertyNotifiedConsumer consumer, int rowIndex)
        {
            var consumers = _consumersByRowIndex.AddNewIfNotExists(rowIndex);
            consumers.Add(consumer);
        }

        public void UnregisterPropertyNotifiedConsumer(IReactivePropertyNotifiedConsumer consumer, int rowIndex)
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

        public Dictionary<string, IReactiveColumn> Columns
        {
            get { return _columns; }
        }

        public ReactiveTable()
        {
        }

        public ReactiveTable(IReactiveTable reactiveTable)
        {
            foreach (var column in reactiveTable.Columns)
            {
                AddColumn(column.Value.Clone());
            }
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            _rowObservers.Add(observer);
            return new SubscriptionToken<ReactiveTable, IObserver<int>>(this, observer);
        }

        public void Unsubscribe(IObserver<int> observer)
        {
            _rowObservers.Remove(observer);
        }

        public IDisposable Subscribe(IObserver<ColumnUpdate> observer)
        {
            _columnObservers.Add(observer);
            return new SubscriptionToken<ReactiveTable, IObserver<ColumnUpdate>>(this, observer);
        }

        public void Unsubscribe(IObserver<ColumnUpdate> observer)
        {
            _columnObservers.Remove(observer);
        }

        private class ColumnChangePublisher : IColumnObserver
        {
            private readonly ReactiveTable _reactiveTable;
            private readonly IReactiveColumn _column;

            public ColumnChangePublisher(ReactiveTable reactiveTable, IReactiveColumn column)
            {
                _reactiveTable = reactiveTable;
                _column = column;
            }

            public void OnNext(int rowIndex)
            {
                // Notify consumers first as everything should be in place before we notify the GUI.
                foreach (var observer in _reactiveTable._columnObservers)
                {
                    observer.OnNext(new ColumnUpdate(_column, rowIndex));
                }

                _reactiveTable.NotifyConsumers(_column.ColumnId, rowIndex);
            }

            public void OnError(Exception error, int rowIndex)
            {
                foreach (var observer in _reactiveTable._rowObservers)
                {
                    observer.OnError(error);
                }
            }

            public void OnCompleted(int rowIndex)
            {
                foreach (var observer in _reactiveTable._rowObservers)
                {
                    observer.OnCompleted();
                }
            }
        }
    }
}