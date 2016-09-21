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
using System.Diagnostics;

namespace ReactiveTables.Framework.Columns
{
    /// <summary>
    /// Maintains a list of deleted rows so that those slots can be re-used when next inserting.
    /// This allows us to never call List.Remove() thus avoiding a costly operation.
    /// </summary>
    public class FieldRowManager
    {
        private readonly Queue<int> _deletedRows = new Queue<int>();

        /// <summary>
        /// The number of rows present
        /// </summary>
        public int RowCount { get; private set; }

        /// <summary>
        /// Create a new FieldRowManager
        /// </summary>
        public FieldRowManager()
        {
            RowCount = 0;
        }

        /// <summary>
        /// Add a new row
        /// </summary>
        /// <returns>The index of the newly added row</returns>
        public int AddRow()
        {
            RowCount++;
            var newRow = _deletedRows.Count <= 0;
            if (!newRow)
            {
                return _deletedRows.Dequeue();
            }
            return RowCount - 1;
        }

        /// <summary>
        /// Remove a row at the given index
        /// </summary>
        /// <param name="rowIndex"></param>
        public void DeleteRow(int rowIndex)
        {
            RowCount--;
            _deletedRows.Enqueue(rowIndex);
        }

        /// <summary>
        /// Get all the row indeces currently being used
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> GetRows()
        {
            var deletedEnum = _deletedRows.GetEnumerator();
            var firstDeleted = -1;
            for (var i = 0; i < RowCount; i++)
            {
                // Find the next deleted index
                while (firstDeleted <= i)
                {
                    firstDeleted = deletedEnum.MoveNext() ? deletedEnum.Current : int.MaxValue;
                }

                // Skip deleteds
                if (i == firstDeleted)
                    continue;

                // Return the rest
                yield return i;
            }
        }
        
        /// <summary>
        /// Returns the row index at the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int GetRowAt(int position)
        {
            if (position >= RowCount || position < 0)
                //throw new ArgumentOutOfRangeException("position", "Position must be less than RowCount and greater than 0");
                return -1;

            //GetRows().Take(position);

            if (_deletedRows.Count == 0) return position;
            
            var deletedCount = 0;
            foreach (var deletedRow in _deletedRows)
            {
                // Add to the deleted
                deletedCount++;

                if (deletedRow - deletedCount >= position)
                {
                    if (deletedRow > position)
                    {
                        deletedCount--;
                    }
                    break;
                }
            }

            return position + deletedCount;
        }

        /// <summary>
        /// Returns the position in the list of the given row index
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public int GetPositionOfRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex > RowCount + _deletedRows.Count || RowCount == 0)
                return -1;

            //GetRows().Count(i => i <= rowIndex);
            
            var deletedCount = 0;
            foreach (var deletedRow in _deletedRows)
            {
                if (deletedRow < rowIndex)
                    deletedCount++;
                else if (deletedRow == rowIndex)
                    return -1;
                else
                    break;
            }
            return rowIndex - deletedCount;
        }

        /// <summary>
        /// Reset the state of the manager (no rows present and none deleted)
        /// </summary>
        public void Reset()
        {
            _deletedRows.Clear();
            RowCount = 0;
        }
    }
}