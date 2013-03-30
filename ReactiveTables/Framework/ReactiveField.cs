using System;
using System.Collections.Generic;

namespace ReactiveTables.Framework
{
    /*public interface IWritableReactiveField<T>:IReactiveField<T>
    {
        void SetInternalFieldValue(T value);
    }*/

    public interface IReactiveField<out T>
    {
//        IDisposable Subscribe(IObserver<T> observer);
        T Value { get; }
    }

    public class ReactiveField<T>: IObservable<T>, IReactiveField<T>
    {
        private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();

        public T Value { get; private set; }

        internal void SetInternalFieldValue(T value)
        {
            Value = value;
/*
            foreach (var observer in _observers)
            {
                observer.OnNext(_value);
            }
*/
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var token = new ReactiveFieldToken(this, observer);
            _observers.Add(observer);
            return token;
        }

        private void Unsubscribe(object observer)
        {
            _observers.Remove((IObserver<T>) observer);
        }

        class ReactiveFieldToken: IDisposable
        {
            private readonly ReactiveField<T> _field;
            private readonly object _observer; 

            public ReactiveFieldToken(ReactiveField<T> field, object observer)
            {
                _field = field;
                _observer = observer;
            }

            public void Dispose()
            {
                _field.Unsubscribe(_observer);
            }
        }
    }
}