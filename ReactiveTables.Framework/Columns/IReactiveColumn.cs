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
        /// Get a value from the column - note that this will box the result if it is a value type.
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
}