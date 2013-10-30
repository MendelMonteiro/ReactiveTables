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
using System.Reactive.Subjects;

namespace ReactiveTables.Framework.Columns
{
    /// <summary>
    /// Transform multiple column subscriptions into one.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class ColumnSubscriptionAggregator<TOutput> : IDisposable
    {
        private readonly ReactiveColumnBase<TOutput> _outputColumn;
        private readonly Subject<TableUpdate> _updateSubject;
        private readonly HashSet<object> _observers = new HashSet<object>();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        public ColumnSubscriptionAggregator(ReactiveColumnBase<TOutput> outputColumn, Subject<TableUpdate> updateSubject)
        {
            _outputColumn = outputColumn;
            _updateSubject = updateSubject;
        }

        private IObserver<TableUpdate> GetObserver()
        {
            var observer = new Observer(this);
            _observers.Add(observer);
            return observer;
        }

        private void OnNext(int index)
        {
            _updateSubject.OnNext(new TableUpdate(TableUpdate.TableUpdateAction.Update, index, _outputColumn));
        }

        private void OnError(Exception error)
        {
            _updateSubject.OnError(error);
        }

        private void OnCompleted()
        {
            _updateSubject.OnCompleted();
        }

        public void SubscribeToColumn(IReactiveColumn column)
        {
            _subscriptions.Add(column.Subscribe(GetObserver()));
        }

        public void Dispose()
        {
            _subscriptions.ForEach(s => s.Dispose());
            _subscriptions.Clear();
        }

        private sealed class Observer : IObserver<TableUpdate>
        {
            private readonly ColumnSubscriptionAggregator<TOutput> _subscriptionAggregator;

            public Observer(ColumnSubscriptionAggregator<TOutput> subscriptionAggregator)
            {
                _subscriptionAggregator = subscriptionAggregator;
            }

            public void OnNext(TableUpdate value)
            {
                _subscriptionAggregator.OnNext(value.RowIndex);
            }

            public void OnError(Exception error)
            {
                _subscriptionAggregator.OnError(error);
            }

            public void OnCompleted()
            {
                _subscriptionAggregator.OnCompleted();
            }
        }
    }
}