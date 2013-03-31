using System;
using System.Collections.Generic;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework
{
    public class SubscriptionAggregator<TOutput>
    {
        private readonly ReactiveColumnBase<TOutput> _outputColumn;
        private readonly HashSet<object> _observers = new HashSet<object>();
        
        public SubscriptionAggregator(ReactiveColumnBase<TOutput> outputColumn)
        {
            _outputColumn = outputColumn;
        }

        public IColumnObserver GetObserver<T>()
        {
            var observer = new Observer<T>(this);
            _observers.Add(observer);
            return observer;
        }

        private void OnNext(int index)
        {
            _outputColumn.NotifyObserversOnNext(index);
        }

        private void OnError<T>(Exception error, int index)
        {
            _outputColumn.NotifyObserversOnError(error, index);
        }

        private void OnCompleted<T>(int index)
        {
            _outputColumn.NotifyObserversOnCompleted(index);
        }

        class Observer<T>:IColumnObserver
        {
            private readonly SubscriptionAggregator<TOutput> _subscriptionAggregator;

            public Observer(SubscriptionAggregator<TOutput> subscriptionAggregator)
            {
                _subscriptionAggregator = subscriptionAggregator;
            }

            public void OnNext(int rowIndex)
            {
                _subscriptionAggregator.OnNext(rowIndex);
            }

            public void OnError(Exception error, int rowIndex)
            {
                _subscriptionAggregator.OnError<T>(error, rowIndex);
            }

            public void OnCompleted(int rowIndex)
            {
                _subscriptionAggregator.OnCompleted<T>(rowIndex);
            }
        }
    }
}