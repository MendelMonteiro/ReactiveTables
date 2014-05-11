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
    public class Max<TIn> : MinOrMax<TIn>
    {
        public Max() : base(MinOrMaxType.Max){}
    }

    public class Min<TIn> : MinOrMax<TIn>
    {
        public Min() : base(MinOrMaxType.Min){}
    }

    public enum MinOrMaxType
    {
        Min,
        Max
    }

    public class MinOrMax<TIn> : IAccumulator<TIn, TIn>
    {
        private readonly MinOrMaxType _type;
        private IReactiveColumn<TIn> _sourceColumn;
        private readonly SortedList<TIn, int> _sourceValues = new SortedList<TIn, int>();
        private readonly Dictionary<int, TIn> _sourceRowsToValues = new Dictionary<int, TIn>(); 
        
        public MinOrMax(MinOrMaxType type)
        {
            _type = type;
        }

        public void SetSourceColumn(IReactiveColumn<TIn> sourceColumn)
        {
            _sourceColumn = sourceColumn;
        }

        public void AddValue(int sourceRowIndex)
        {
            var value = _sourceColumn.GetValue(sourceRowIndex);

            TIn oldValue;
            if (_sourceRowsToValues.TryGetValue(sourceRowIndex, out oldValue))
            {
                RemoveSourceValue(oldValue);
            }

            _sourceRowsToValues[sourceRowIndex] = value;
            //_sourceValues.Remove(existingValue); // Make sure list doesn't keep growing
            AddSourceValue(value);
        }

        private void AddSourceValue(TIn value)
        {
            if (!_sourceValues.ContainsKey(value))
            {
                _sourceValues.Add(value, 1);
            }
            else
            {
                _sourceValues[value] = _sourceValues[value] + 1;
            }
        }

        public void RemoveValue(int sourceRowIndex)
        {
            var value = _sourceColumn.GetValue(sourceRowIndex);
            _sourceRowsToValues.Remove(sourceRowIndex);
            RemoveSourceValue(value);
        }

        private void RemoveSourceValue(TIn value)
        {
            var count = _sourceValues[value];
            count--;
            if (count == 0)
            {
                _sourceValues.Remove(value);
            }
            else
            {
                _sourceValues[value] = count;
            }
        }

        public TIn CurrentValue
        {
            get
            {
                if (_sourceValues.Count <= 0)
                {
                    return default(TIn);
                }
                if (_type == MinOrMaxType.Max)
                {
                    return _sourceValues.Keys[_sourceValues.Count - 1];
                }
                return _sourceValues.Keys[0];
            }
        }
    }
}