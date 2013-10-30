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
using ReactiveTables.Framework.Joins;

namespace ReactiveTables.Framework.Columns
{
    /// <summary>
    /// Represents a column - exposes observable changes.
    /// </summary>
    public interface IReactiveColumn : IObservable<TableUpdate>, IEquatable<IReactiveColumn>
    {
        /// <summary>
        /// Should be an int?
        /// </summary>
        string ColumnId { get; }

        /// <summary>
        /// The data type represented by the column
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Add a value to the column
        /// </summary>
        /// <param name="rowIndex"></param>
        void AddField(int rowIndex);

        /// <summary>
        /// Clone the column
        /// </summary>
        /// <returns></returns>
        IReactiveColumn Clone();

        /// <summary>
        /// Copy the value from to another column (of the same type)
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="sourceRowIndex"></param>
        void CopyValue(int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex);

        /// <summary>
        /// Remove a value from the column
        /// </summary>
        /// <param name="rowIndex"></param>
        void RemoveField(int rowIndex);

        /// <summary>
        /// Get a value from the column
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        object GetValue(int rowIndex);
    }

    /// <summary>
    /// A typed reactive column
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactiveColumn<T> : IReactiveColumn
    {
        /// <summary>
        /// Set a value at a particular row in the column
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="value"></param>
        void SetValue(int rowIndex, T value);

        /// <summary>
        /// Get a value at a particular row in the column
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        new T GetValue(int index);

        /// <summary>
        /// Find the first row that contains the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        int Find(T value);
    }

    /// <summary>
    /// A joinable reactive column (columns need to be joinable to be used by the <see cref="JoinedTable"/>)
    /// </summary>
    public interface IReactiveJoinableColumn
    {
        /// <summary>
        /// Set the current joiner
        /// </summary>
        /// <param name="joiner"></param>
        void SetJoiner(IReactiveTableJoiner joiner);
    }

    public class ReactiveColumn<T> : ReactiveColumnBase<T>
    {
        /// <summary>
        /// TODO: Try to implement the index as an observer of the column thus decoupling them
        /// </summary>
        private readonly IColumnIndex<T> _index;

        public ReactiveColumn(string columnId, IColumnIndex<T> index = null, int? initialSize = null)
        {
            _index = index;
            ColumnId = columnId;

            Fields = initialSize == null ? new List<T>() : new List<T>(initialSize.Value);
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
            if (_index != null)
            {
                T oldValue = Fields[rowIndex];
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
    }
}