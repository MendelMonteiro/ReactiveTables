using System;
using System.Collections.Generic;

namespace ReactiveTables.Framework.Columns
{
    public abstract class ReactiveColumnBase<T>: IReactiveColumn<T>
    {
        public string ColumnId { get; protected set; }

        private readonly List<IColumnObserver> _observers = new List<IColumnObserver>();

        public IDisposable Subscribe(IColumnObserver observer)
        {
            var token = new ReactiveColumnToken(this, observer);
            _observers.Add(observer);
            return token;
        }

        protected virtual void Unsubscribe(object observer)
        {
            _observers.Remove((IColumnObserver)observer);
        }

        internal void NotifyObserversOnNext(int index)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(index);
            }
        }

        internal void NotifyObserversOnError(Exception error, int index)
        {
            _observers.ForEach(observer => observer.OnError(error, index));
        }

        internal void NotifyObserversOnCompleted(int index)
        {
            _observers.ForEach(observer => observer.OnCompleted(index));
        }

        public abstract void AddField();

        public virtual IReactiveColumn Clone()
        {
            throw new NotImplementedException();
        }

        public abstract void CopyValue(int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex);

        public abstract void RemoveField(int rowIndex);

        public abstract void SetValue(int rowIndex, T value);

        public abstract IReactiveField<T> GetValue(int rowIndex);

        private class ReactiveColumnToken : IDisposable
        {
            private readonly ReactiveColumnBase<T> column;
            private readonly object _observer;

            public ReactiveColumnToken(ReactiveColumnBase<T> column, object observer)
            {
                this.column = column;
                _observer = observer;
            }

            public void Dispose()
            {
                column.Unsubscribe(_observer);
            }
        }
    }
}