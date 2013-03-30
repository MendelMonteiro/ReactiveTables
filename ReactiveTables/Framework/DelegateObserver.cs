using System;

namespace ReactiveTables.Framework
{
    public class DelegateObserver<T> : IObserver<T>
    {
        public static DelegateObserver<T> CreateDelegateObserver(Action<T> onNext)
        {
            return new DelegateObserver<T>(onNext, null, null);
        }
        public static DelegateObserver<T> CreateDelegateObserver(Action<T> onNext, Action onCompleted)
        {
            return new DelegateObserver<T>(onNext, null, onCompleted);
        }
        public static DelegateObserver<T> CreateDelegateObserver(Action<T> onNext, Action<Exception> onError)
        {
            return new DelegateObserver<T>(onNext, onError, null);
        }
        public static DelegateObserver<T> CreateDelegateObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            return new DelegateObserver<T>(onNext, onError, onCompleted);
        }

        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;


        private DelegateObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public void OnNext(T value)
        {
            _onNext(value);
        }

        public void OnError(Exception error)
        {
            if (_onError != null)
            {
                
                _onError(error);
            }
        }

        public void OnCompleted()
        {
            if (_onCompleted != null)
            {
                _onCompleted();
            }
        }
    }
}