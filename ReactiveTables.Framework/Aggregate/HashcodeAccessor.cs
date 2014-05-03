using System;
using System.Reactive.Subjects;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Aggregate
{
    class HashcodeAccessor<T> : IHashcodeAccessor, IReactiveColumn<T>
    {
        private readonly IReactiveColumn<T> _sourceColumn;
        private readonly Subject<TableUpdate> _updates = new Subject<TableUpdate>(); 

        public HashcodeAccessor(IReactiveColumn<T> sourceColumn)
        {
            _sourceColumn = sourceColumn;
        }

        public int GetColumnHashCode(int rowId)
        {
            var value = _sourceColumn.GetValue(rowId);
            if (typeof (T).IsValueType)
            {
                return value.GetHashCode();
            }

            if (Equals(null, value))
            {
                return 0;
            }

            return value.GetHashCode();
        }

        public string ColumnId { get { return _sourceColumn.ColumnId; } }
        public void NotifyObserversOnNext(int index)
        {
            _updates.OnNext(new TableUpdate(TableUpdateAction.Update, index, this));
        }

        public Type Type { get { return typeof (T); } }

        public void AddField(int rowIndex)
        {
            throw new NotImplementedException();
        }

        public IReactiveColumn Clone()
        {
            throw new NotImplementedException();
        }

        public void CopyValue(int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex)
        {
            throw new NotImplementedException();
        }

        public void RemoveField(int rowIndex)
        {
            throw new NotImplementedException();
        }

        public int Find(T value)
        {
            throw new NotImplementedException();
        }

        public void SetValue(int rowIndex, T value)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int index)
        {
            return _sourceColumn.GetValue(index);
        }

        T IReactiveColumn<T>.GetValue(int index)
        {
            return _sourceColumn.GetValue(index);
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            return _updates.Subscribe(observer);
        }

        public bool Equals(IReactiveColumn other)
        {
            return Equals((object)other);
        }
    }
}