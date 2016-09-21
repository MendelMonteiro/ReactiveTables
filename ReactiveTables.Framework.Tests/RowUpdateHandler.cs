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

using System;
using System.Collections.Generic;

namespace ReactiveTables.Framework.Tests
{
    class RowUpdateHandler:IObserver<TableUpdate>
    {
        public int CurrentRowCount { get; private set; }
        public int LastRowUpdated { get; private set; }
        public List<int> RowsUpdated { get; }
        public List<TableUpdate> RowUpdates { get; }

        public RowUpdateHandler()
        {
            RowsUpdated = new List<int>();
            RowUpdates = new List<TableUpdate>();
            LastRowUpdated = -1;
        }

        public void OnRowUpdate(TableUpdate update)
        {
            RowsUpdated.Add(update.RowIndex);
            RowUpdates.Add(update);
            if (update.Action == TableUpdateAction.Add)
            {
                CurrentRowCount++;
            }
            else if (update.Action == TableUpdateAction.Delete)
            {
                CurrentRowCount--;
            }
            LastRowUpdated = update.RowIndex;
        }

        public void OnNext(TableUpdate value)
        {
            if (value.IsRowUpdate())
            {
                OnRowUpdate(value);
            }
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }
}
