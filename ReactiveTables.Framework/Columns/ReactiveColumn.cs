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
    public interface IReactiveColumn : IObservableColumn
    {
        /// <summary>
        /// Should be an int?
        /// </summary>
        string ColumnId { get; }

        void AddField(int rowIndex);
        IReactiveColumn Clone();
        void CopyValue(int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex);
        void RemoveField(int rowIndex);
    }

    public interface IReactiveColumn<T> : IReactiveColumn
    {
        void SetValue(int rowIndex, T value);
        T GetValue(int index);
        int Find(T value);
    }

    public interface IReactiveJoinableColumn
    {
        void SetJoiner(IReactiveTableJoiner joiner);
    }

    public class ReactiveColumn<T> : ReactiveColumnBase<T>
    {
        /// <summary>
        /// TODO: Try to implement the index as an observer of the column thus decoupling them
        /// </summary>
        private readonly IColumnIndex<T> _index;

        public ReactiveColumn(string columnId, IColumnIndex<T> index = null)
        {
            _index = index;
            ColumnId = columnId;
            Fields = new List<T>();
        }

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
                T value = Fields[rowIndex];
                _index.RemoveRowValue(value);
            }
            Fields[rowIndex] = default(T);
        }

        private List<T> Fields { get; set; }

        public override T GetValue(int rowIndex)
        {
            return rowIndex < 0 ? default(T) : Fields[rowIndex];
        }

        public override void SetValue(int rowIndex, T value)
        {
            Fields[rowIndex] = value;
            if (_index != null) _index.SetRowValue(rowIndex, value);
            NotifyObserversOnNext(rowIndex);
        }

        public override int Find(T value)
        {
            if (_index == null) throw new NotSupportedException("No index defined for this column " + ColumnId);

            return _index.GetRow(value);
        }
    }
}