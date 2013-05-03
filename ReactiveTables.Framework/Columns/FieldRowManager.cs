/*This file is part of ReactiveTables.

ReactiveTables is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ReactiveTables is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.Collections.Generic;

namespace ReactiveTables.Framework.Columns
{
    public class FieldRowManager
    {
        private readonly Queue<int> _deletedRows = new Queue<int>();

        public FieldRowManager()
        {
            RowCount = 0;
        }

        public int RowCount { get; private set; }

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

        public void DeleteRow(int rowIndex)
        {
            RowCount--;
            _deletedRows.Enqueue(rowIndex);
        }

        public IEnumerable<int> GetRows()
        {
            var deletedEnum = _deletedRows.GetEnumerator();
            int firstDeleted = -1;
            for (int i = 0; i < RowCount; i++)
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
    }
}
