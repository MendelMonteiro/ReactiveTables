using System;
using System.Collections.Generic;

namespace ReactiveTables
{
    public class SubscriptionAggregator<TOutput>
    {
        private readonly ReactiveColumnBase<TOutput> _outputColumn;
        private readonly HashSet<object> _observers = new HashSet<object>();
        
        public SubscriptionAggregator(ReactiveColumnBase<TOutput> outputColumn)
        {
            _outputColumn = outputColumn;
        }

        public IColumnObserver<T> GetObserver<T>()
        {
            var observer = new Observer<T>(this);
            _observers.Add(observer);
            return observer;
        }

        private void OnNext<T>(T value, int index)
        {
            _outputColumn.NotifyObserversOnNext(_outputColumn.GetValue(index).Value, index);
        }

        private void OnError<T>(Exception error, int index)
        {
            _outputColumn.NotifyObserversOnError(error, index);
        }

        private void OnCompleted<T>(int index)
        {
            _outputColumn.NotifyObserversOnCompleted(index);
        }

        class Observer<T>:IColumnObserver<T>
        {
            private readonly SubscriptionAggregator<TOutput> _subscriptionAggregator;

            public Observer(SubscriptionAggregator<TOutput> subscriptionAggregator)
            {
                _subscriptionAggregator = subscriptionAggregator;
            }

            public void OnNext(T value, int index)
            {
                _subscriptionAggregator.OnNext(value, index);
            }

            public void OnError(Exception error, int index)
            {
                _subscriptionAggregator.OnError<T>(error, index);
            }

            public void OnCompleted(int index)
            {
                _subscriptionAggregator.OnCompleted<T>(index);
            }
        }
    }
}