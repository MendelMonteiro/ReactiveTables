using System;

namespace ReactiveTables.Framework
{
    public class SubscriptionToken<TObservable, TObserver> : IDisposable where TObservable : ISubscribable<TObserver>
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

    public interface ISubscribable<in TObserver>
    {
        void Unsubscribe(TObserver observer);
    }
}