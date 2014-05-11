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
    public class Count<TIn> : IAccumulator<TIn, int>
    {
        private readonly HashSet<int> _sourceRows = new HashSet<int>();

        public void SetSourceColumn(IReactiveColumn<TIn> sourceColumn)
        {
        }

        public void AddValue(int sourceRowIndex)
        {
            _sourceRows.Add(sourceRowIndex);
            CurrentValue = _sourceRows.Count;
        }

        public void RemoveValue(int sourceRowIndex)
        {
            _sourceRows.Remove(sourceRowIndex);
            CurrentValue = _sourceRows.Count;
        }

        public int CurrentValue { get; private set; }
    }
}