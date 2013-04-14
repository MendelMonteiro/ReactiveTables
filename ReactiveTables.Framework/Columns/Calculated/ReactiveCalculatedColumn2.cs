using System;
using ReactiveTables.Framework.Joins;

namespace ReactiveTables.Framework.Columns.Calculated
{
    public class ReactiveCalculatedColumn2<T, T1, T2> : ReactiveColumnBase<T>, IReactiveJoinableColumn
    {
        private readonly IReactiveColumn<T1> _inputColumn1;
        private readonly IReactiveColumn<T2> _inputColumn2;
        private readonly Func<T1, T2, T> _converter;
        private IReactiveTableJoiner _joiner = DefaultJoiner.DefaultJoinerInstance;
        private readonly ColumnSubscriptionAggregator<T> _aggregator;

        public ReactiveCalculatedColumn2(string columnId, 
                                         IReactiveColumn<T1> inputColumn1, 
                                         IReactiveColumn<T2> inputColumn2, 
                                         Func<T1, T2, T> converter)
        {
            ColumnId = columnId;
            _inputColumn1 = inputColumn1;
            _inputColumn2 = inputColumn2;
            _converter = converter;

            _aggregator = new ColumnSubscriptionAggregator<T>(this);
            _aggregator.SubscribeToColumn(_inputColumn1);
            _aggregator.SubscribeToColumn(_inputColumn2);
        }

        public override void CopyValue(int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex)
        {
            throw new NotImplementedException();
        }

        public override void RemoveField(int rowIndex)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(int rowIndex, T value)
        {
            throw new NotImplementedException();
        }

        /*/// <summary>
        /// For optimisation - worth it?
        /// </summary>
        /// <typeparam name="TColumn"></typeparam>
        /// <param name="rowIndex"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IReactiveField<T> GetValueOnDependentUpdate<TColumn>(int rowIndex, IReactiveColumn<TColumn> column, TColumn value)
        {
            if (column == _inputColumn1)
            {
                T1 
                return GetValue(value, _inputColumn2.GetValue(rowIndex).Value);
            }
        }*/

        public override IReactiveField<T> GetValue(int rowIndex)
        {
            // TODO: cache values??
            var rowIndex1 = _joiner.GetRowIndex(_inputColumn1, rowIndex);
            var rowIndex2 = _joiner.GetRowIndex(_inputColumn2, rowIndex);
            return GetValue(_inputColumn1.GetValue(rowIndex1).Value, _inputColumn2.GetValue(rowIndex2).Value);
        }

        private IReactiveField<T> GetValue(T1 value1, T2 value2)
        {
            var value = _converter(value1, value2);
            return new ReactiveVirtualField<T> {Value = value};
        }

        public void SetJoiner(IReactiveTableJoiner joiner)
        {
            _joiner = joiner;
        }

        public override void AddField(int rowIndex)
        {
        }

        protected override void Unsubscribe(object observer)
        {
            _aggregator.Unsubscribe();
//            base.Unsubscribe(observer);
        }
    }
}