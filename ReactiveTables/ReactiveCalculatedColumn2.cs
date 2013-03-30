using System;

namespace ReactiveTables
{
    public class ReactiveCalculatedColumn2<T, T1, T2> : ReactiveColumnBase<T>
    {
        private readonly IReactiveColumn<T1> _inputColumn1;
        private readonly IReactiveColumn<T2> _inputColumn2;
        private readonly Func<T1, T2, T> _converter;
        private IDisposable _col1Subscription;
        private IDisposable _col2Subscription;

        public ReactiveCalculatedColumn2(string columnId, 
                                         IReactiveColumn<T1> inputColumn1, 
                                         IReactiveColumn<T2> inputColumn2, 
                                         Func<T1, T2, T> converter)
        {
            ColumnId = columnId;
            _inputColumn1 = inputColumn1;
            _inputColumn2 = inputColumn2;

            SubscriptionAggregator<T> aggregator = new SubscriptionAggregator<T>(this);
            _col1Subscription = _inputColumn1.Subscribe(aggregator.GetObserver<T1>());
            _col2Subscription = _inputColumn2.Subscribe(aggregator.GetObserver<T2>());
            _converter = converter;
        }

        public override void SetValue(int index, T value)
        {
            throw new NotImplementedException();
        }

        /*/// <summary>
        /// For optimisation - worth it?
        /// </summary>
        /// <typeparam name="TColumn"></typeparam>
        /// <param name="index"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IReactiveField<T> GetValueOnDependentUpdate<TColumn>(int index, IReactiveColumn<TColumn> column, TColumn value)
        {
            if (column == _inputColumn1)
            {
                T1 
                return GetValue(value, _inputColumn2.GetValue(index).Value);
            }
        }*/

        public override IReactiveField<T> GetValue(int index)
        {
            // TODO: cache values??
            return GetValue(_inputColumn1.GetValue(index).Value, _inputColumn2.GetValue(index).Value);
        }

        private IReactiveField<T> GetValue(T1 value1, T2 value2)
        {
            var value = _converter(value1, value2);
            return new ReactiveVirtualField<T> {Value = value};
        }

        public override void AddField()
        {
        }

        protected override void Unsubscribe(object observer)
        {
            _col1Subscription.Dispose();
            _col2Subscription.Dispose();
            base.Unsubscribe(observer);
        }
    }
}