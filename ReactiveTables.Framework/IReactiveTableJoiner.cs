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
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework
{
    /// <summary>
    /// Joins two reactive tables
    /// </summary>
    public interface IReactiveTableJoiner : IDisposable
    {
        /// <summary>
        /// The number of rows in the table.
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// Get the underlying row index (in the original table) for the <see cref="joinRowIndex"/>
        /// </summary>
        /// <param name="column"></param>
        /// <param name="joinRowIndex"></param>
        /// <returns></returns>
        int GetRowIndex(IReactiveColumn column, int joinRowIndex);
        
        /// <summary>
        /// Register an observer with this joiner
        /// </summary>
        /// <param name="observer"></param>
        void AddObserver(IObserver<TableUpdate> observer);
        
        /// <summary>
        /// Unregister an observer with this joiner
        /// </summary>
        /// <param name="observer"></param>
        void RemoveObserver(IObserver<TableUpdate> observer);
        
        /// <summary>
        /// Get all the row indeces present in this table
        /// </summary>
        /// <returns></returns>
        IEnumerable<int> GetRows();
        
        /// <summary>
        /// Get the index of the row at <see cref="position"/>
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        int GetRowAt(int position);
        
        /// <summary>
        /// Get the position of the given <see cref="rowIndex"/> in the table.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        int GetPositionOfRow(int rowIndex);
    }
}