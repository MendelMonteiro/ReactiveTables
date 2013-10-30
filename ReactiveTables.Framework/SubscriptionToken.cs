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

namespace ReactiveTables.Framework
{
    /// <summary>
    /// A basic token for handling subscriptions to <see cref="ISubscribable{TObserver}"/> objects.
    /// </summary>
    /// <typeparam name="TObservable"></typeparam>
    /// <typeparam name="TObserver"></typeparam>
    internal class SubscriptionToken<TObservable, TObserver> : IDisposable where TObservable : ISubscribable<TObserver>
    {
        private readonly TObservable _item;
        private readonly TObserver _observer;

        public SubscriptionToken(TObservable item, TObserver observer)
        {
            _item = item;
            _observer = observer;
        }

        public void Dispose()
        {
            _item.Unsubscribe(_observer);
        }
    }

    /// <summary>
    /// Can be unsubscribed from.
    /// </summary>
    /// <typeparam name="TObserver"></typeparam>
    internal interface ISubscribable<in TObserver>
    {
        /// <summary>
        /// Unsubscribe the given observer
        /// </summary>
        /// <param name="observer"></param>
        void Unsubscribe(TObserver observer);
    }
}