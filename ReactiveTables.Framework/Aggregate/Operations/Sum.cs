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
using System.Collections.Generic;
using MiscUtil;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Aggregate.Operations
{
    public class Sum<TIn> : IAccumulator<TIn, TIn>
    {
        private IReactiveColumn<TIn> _sourceColumn;
        /// <summary>
        /// The fact that we need to duplicate this data is another argument for propagating the previous value
        /// in the TableUpdate events.
        /// </summary>
        private readonly Dictionary<int, TIn> _sourceValues = new Dictionary<int, TIn>(); 

        public void SetSourceColumn(IReactiveColumn<TIn> sourceColumn)
        {
            _sourceColumn = sourceColumn;
        }

        public void AddValue(int sourceRowIndex)
        {
            var value = _sourceColumn.GetValue(sourceRowIndex);
            TIn existingSourceValue;
            if (_sourceValues.TryGetValue(sourceRowIndex, out existingSourceValue))
            {
                CurrentValue = Operator.SubtractAlternative(CurrentValue, existingSourceValue);
            }
            CurrentValue = Operator.AddAlternative(CurrentValue, value);
            _sourceValues[sourceRowIndex] = value;
        }

        public void RemoveValue(int sourceRowIndex)
        {
            var value = _sourceColumn.GetValue(sourceRowIndex);
            CurrentValue = Operator.SubtractAlternative(CurrentValue, value);
            _sourceValues.Remove(sourceRowIndex);
        }

        public TIn CurrentValue { get; private set; }
    }
}