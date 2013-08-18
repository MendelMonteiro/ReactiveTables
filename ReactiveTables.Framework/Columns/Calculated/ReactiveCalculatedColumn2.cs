/*This file is part of ReactiveTables.

ReactiveTables is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ReactiveTables is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using ReactiveTables.Framework.Joins;

namespace ReactiveTables.Framework.Columns.Calculated
{
    public class ReactiveCalculatedColumn2<TOut, TIn1, TIn2> : ReactiveColumnBase<TOut>, IReactiveJoinableColumn
    {
        private readonly IReactiveColumn<TIn1> _inputColumn1;
        private readonly IReactiveColumn<TIn2> _inputColumn2;
        private readonly Func<TIn1, TIn2, TOut> _converter;
        private IReactiveTableJoiner _joiner = DefaultJoiner.DefaultJoinerInstance;
        private readonly ColumnSubscriptionAggregator<TOut> _aggregator;

        public ReactiveCalculatedColumn2(string columnId, 
                                         IReactiveColumn<TIn1> inputColumn1, 
                                         IReactiveColumn<TIn2> inputColumn2, 
                                         Func<TIn1, TIn2, TOut> converter)
        {
            ColumnId = columnId;
            _inputColumn1 = inputColumn1;
            _inputColumn2 = inputColumn2;
            _converter = converter;

            _aggregator = new ColumnSubscriptionAggregator<TOut>(this);
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

        public override void SetValue(int rowIndex, TOut value)
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

        public override TOut GetValue(int rowIndex)
        {
            // TODO: cache values??
            var rowIndex1 = _joiner.GetRowIndex(_inputColumn1, rowIndex);
            var rowIndex2 = _joiner.GetRowIndex(_inputColumn2, rowIndex);
            return GetValue(_inputColumn1.GetValue(rowIndex1), _inputColumn2.GetValue(rowIndex2));
        }

        public override int Find(TOut value)
        {
            throw new NotImplementedException();
        }

        private TOut GetValue(TIn1 value1, TIn2 value2)
        {
            var value = _converter(value1, value2);
            return value;
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
