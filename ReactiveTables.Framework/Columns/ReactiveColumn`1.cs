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

namespace ReactiveTables.Framework.Columns
{
    /// <summary>
    /// A reactive column which stores data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReactiveColumn<T> : ReactiveColumnBase<T>
    {
        /// <summary>
        /// TODO: Try to implement the index as an observer of the column thus decoupling them
        /// </summary>
        private readonly IColumnIndex<T> _index;

        /// <summary>
        /// Create a new column
        /// </summary>
        /// <param name="columnId"></param>
        /// <param name="index"></param>
        /// <param name="initialSize"></param>
        public ReactiveColumn(string columnId, IColumnIndex<T> index = null, int? initialSize = null)
        {
            _index = index;
            ColumnId = columnId;

            Fields = initialSize == null ? new List<T>() : new List<T>(initialSize.Value);
        }

        private List<T> Fields { get; }

        public override void AddField(int rowIndex)
        {
            if (rowIndex < Fields.Count)
            {
                Fields[rowIndex] = default(T);
            }
            else
            {
                Fields.Add(default(T));
            }
        }

        public override IReactiveColumn Clone()
        {
            return new ReactiveColumn<T>(ColumnId);
        }

        public override void CopyValue(int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex)
        {
            // Assumes that the source column is of the same type.
            var sourceCol = (IReactiveColumn<T>) sourceColumn;
            SetValue(rowIndex, sourceCol.GetValue(sourceRowIndex));
        }

        public override void RemoveField(int rowIndex)
        {
            if (_index != null)
            {
                var value = Fields[rowIndex];
                _index.RemoveRowValue(value);
            }
            Fields[rowIndex] = default(T);
        }

        public override T GetValue(int rowIndex)
        {
            return rowIndex < 0 ? default(T) : Fields[rowIndex];
        }

        public override void SetValue(int rowIndex, T value)
        {
            if (_index != null)
            {
                var oldValue = Fields[rowIndex];
                _index.SetRowValue(rowIndex, value, oldValue);
            }
            Fields[rowIndex] = value;
            NotifyObserversOnNext(rowIndex);
        }

        public override int Find(T value)
        {
            if (_index == null) throw new NotSupportedException("No index defined for this column " + ColumnId);

            return _index.GetRow(value);
        }

        public override string ToString()
        {
            return ColumnId;
        }
    }
}