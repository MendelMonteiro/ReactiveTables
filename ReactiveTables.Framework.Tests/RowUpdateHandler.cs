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
along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.Collections.Generic;

namespace ReactiveTables.Framework.Tests
{
    class RowUpdateHandler
    {
        public int CurrentRowCount { get; private set; }
        public int LastRowUpdated { get; private set; }
        public List<int> RowsUpdated { get; private set; }

        public RowUpdateHandler()
        {
            RowsUpdated = new List<int>();
            LastRowUpdated = -1;
        }

        public void OnRowUpdate(RowUpdate update)
        {
            RowsUpdated.Add(update.RowIndex);
            if (update.Action == RowUpdate.RowUpdateAction.Add)
            {
                CurrentRowCount++;
            }
            else
            {
                CurrentRowCount--;
            }
            LastRowUpdated = update.RowIndex;
        }
    }
}
