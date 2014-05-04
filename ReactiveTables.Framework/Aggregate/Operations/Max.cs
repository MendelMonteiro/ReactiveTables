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
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Aggregate.Operations
{
    class Max<TIn> : IAccumulator<TIn, TIn>
    {
        private IReactiveColumn<TIn> _sourceColumn;
        private readonly List<int> _values = new List<int>();
        private readonly SortedList<TIn, TIn> _sourceValues2 = new SortedList<TIn, TIn>();

        public void SetSourceColumn(IReactiveColumn<TIn> sourceColumn)
        {
            _sourceColumn = sourceColumn;
        }

        public void AddValue(int sourceRowIndex)
        {
            var value = _sourceColumn.GetValue(sourceRowIndex);
            //_sourceValues2.Remove(existingValue); // Make sure list doesn't keep growing
            _sourceValues2.Add(value, value);
        }

        public void RemoveValue(int sourceRowIndex)
        {
            var value = _sourceColumn.GetValue(sourceRowIndex);
            _sourceValues2.Remove(value);
        }

        public TIn CurrentValue { get { return _sourceValues2.Keys[_values.Count - 1]; } }
    }
}