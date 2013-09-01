// This file is part of ReactiveTables.
// 
// ReactiveTables is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReactiveTables is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace ReactiveTables.Framework.Columns
{
    /// <summary>
    /// Transform multiple column subscriptions into one.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class ColumnSubscriptionAggregator<TOutput>
    {
        private readonly ReactiveColumnBase<TOutput> _outputColumn;
        private readonly HashSet<object> _observers = new HashSet<object>();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        public ColumnSubscriptionAggregator(ReactiveColumnBase<TOutput> outputColumn)
        {
            _outputColumn = outputColumn;
        }

        private IColumnObserver GetObserver()
        {
            var observer = new Observer(this);
            _observers.Add(observer);
            return observer;
        }

        private void OnNext(int index)
        {
            _outputColumn.NotifyObserversOnNext(index);
        }

        private void OnError(Exception error, int index)
        {
            _outputColumn.NotifyObserversOnError(error, index);
        }

        private void OnCompleted(int index)
        {
            _outputColumn.NotifyObserversOnCompleted(index);
        }

        public void SubscribeToColumn(IReactiveColumn column)
        {
            _subscriptions.Add(column.Subscribe(GetObserver()));
        }

        public void Unsubscribe()
        {
            _subscriptions.ForEach(s => s.Dispose());
        }

        private sealed class Observer : IColumnObserver
        {
            private readonly ColumnSubscriptionAggregator<TOutput> _subscriptionAggregator;

            public Observer(ColumnSubscriptionAggregator<TOutput> subscriptionAggregator)
            {
                _subscriptionAggregator = subscriptionAggregator;
            }

            public void OnNext(int rowIndex)
            {
                _subscriptionAggregator.OnNext(rowIndex);
            }

            public void OnError(Exception error, int rowIndex)
            {
                _subscriptionAggregator.OnError(error, rowIndex);
            }

            public void OnCompleted(int rowIndex)
            {
                _subscriptionAggregator.OnCompleted(rowIndex);
            }
        }
    }
}