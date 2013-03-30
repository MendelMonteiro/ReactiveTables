using System;
using System.Collections.Generic;

namespace ReactiveTables
{
    public abstract class ReactiveColumnBase<T>: IReactiveColumn<T>
    {
        public string ColumnId { get; protected set; }

        private readonly List<IColumnObserver<T>> _observers = new List<IColumnObserver<T>>();

        public IDisposable Subscribe(IColumnObserver<T> observer)
        {
            var token = new ReactiveColumnToken(this, observer);
            _observers.Add(observer);
            return token;
        }

        protected virtual void Unsubscribe(object observer)
        {
            _observers.Remove((IColumnObserver<T>)observer);
        }

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

        internal void NotifyObserversOnNext(T value, int index)
        {
            _observers.ForEach(observer => observer.OnNext(value, index));
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

        public abstract void SetValue(int index, T value);

        public abstract IReactiveField<T> GetValue(int index);
    }
}