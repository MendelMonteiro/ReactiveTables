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
    class ColumnList
    {
        private readonly List<IReactiveColumn> _columns = new List<IReactiveColumn>();

        public IReadOnlyList<IReactiveColumn> Columns { get { return _columns; } }

        public IReactiveColumn AddColumn(IReactiveColumn column)
        {
            _columns.Add(column);
            // TODO: fire events for existing rows
            return column;
        }

        public IReactiveColumn GetColumnByIndex(int index)
        {
            IList<IReactiveColumn> list = _columns;
            return list[index];
        }

        public bool GetColumnByName(string columnId, out IReactiveColumn outColumn)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                var column = _columns[i];
                if (column.ColumnId == columnId)
                {
                    outColumn = column;
                    return true;
                }
            }
            outColumn = null;
            return false;
        }

        public IReactiveColumn GetColumnByName(string columnId)
        {
            IReactiveColumn col;
            if (!GetColumnByName(columnId, out col))
            {
                throw new ApplicationException(String.Format("Column {0} not found", columnId));
            }
            return col;
        }
    }
}