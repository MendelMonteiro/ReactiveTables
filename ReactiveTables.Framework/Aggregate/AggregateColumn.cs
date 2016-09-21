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
using MiscUtil.Collections.Extensions;
using ReactiveTables.Framework.Aggregate.Operations;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Aggregate
{
    internal interface IAggregateColumn : IReactiveColumn
    {
        void ProcessValue(int sourceRowIndex, int rowIndex);
        IReactiveColumn SourceColumn { get; }
        void RemoveOldValue(int sourceIndex, int rowIndex);
    }

    internal class AggregateColumn<TIn, TOut> : ReactiveColumnBase<TOut>, IAggregateColumn
    {
        private readonly IReactiveColumn<TIn> _sourceColumn;
        private readonly Func<IAccumulator<TIn, TOut>> _accumulatorFactory;
        private readonly List<bool> _initialised = new List<bool>();
        // TODO: Remove un-needed accumulators
        private readonly Dictionary<int, IAccumulator<TIn, TOut>> _accumulators = new Dictionary<int, IAccumulator<TIn, TOut>>();

        public AggregateColumn(IReactiveColumn<TIn> sourceColumn, string columnId, 
            Func<IAccumulator<TIn, TOut>> accumulatorFactory)
        {
            _sourceColumn = sourceColumn;
            _accumulatorFactory = accumulatorFactory;
            Fields = new List<TOut>();
            ColumnId = columnId;
        }

        public IReactiveColumn SourceColumn => _sourceColumn;

        private List<TOut> Fields { get; }

        public void ProcessValue(int sourceRowIndex, int rowIndex)
        {
            var existingValue = Fields[rowIndex];
            _initialised[rowIndex] = true;
            
            var accumulator = _accumulators.GetOrCreate(rowIndex, _accumulatorFactory);
            accumulator.SetSourceColumn(_sourceColumn);
            accumulator.AddValue(sourceRowIndex);
            Fields[rowIndex] = accumulator.CurrentValue;
            
            NotifyObserversOnNext(rowIndex);
        }

        public void RemoveOldValue(int sourceIndex, int rowIndex)
        {
            IAccumulator<TIn, TOut> accumulator;
            if (_accumulators.TryGetValue(rowIndex, out accumulator))
            {
                accumulator.RemoveValue(sourceIndex);
                
                Fields[rowIndex] = accumulator.CurrentValue;
                NotifyObserversOnNext(rowIndex);
            }
        }

        public override void AddField(int rowIndex)
        {
            if (rowIndex < Fields.Count)
            {
                Fields[rowIndex] = default(TOut);
                _initialised[rowIndex] = false;
            }
            else
            {
                Fields.Add(default(TOut));
                _initialised.Add(false);
            }
        }

        public override void CopyValue(int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex)
        {
            var sourceCol = (IReactiveColumn<TOut>)sourceColumn;
            SetValue(rowIndex, sourceCol.GetValue(sourceRowIndex));
        }

        public override void RemoveField(int rowIndex)
        {
            Fields[rowIndex] = default(TOut);
            _initialised[rowIndex] = false;
        }

        public override TOut GetValue(int rowIndex)
        {
            return rowIndex < 0 ? default(TOut) : Fields[rowIndex];
        }

        public override int Find(TOut value)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(int rowIndex, TOut value)
        {
            throw new NotImplementedException();
        }
    }
}