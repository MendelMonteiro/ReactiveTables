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
    public struct TableUpdate
    {
        [Flags]
        public enum TableUpdateAction : short
        {
            Add = 1,
            Update = 2,
            Delete = 4,
        }

        private readonly TableUpdateAction _action;
        private readonly int _rowIndex;
        private readonly IReactiveColumn _column;
        private readonly IList<IReactiveColumn> _columns;

        public TableUpdate(TableUpdateAction action, int rowIndex, IReactiveColumn column = (IReactiveColumn) null)
        {
            _action = action;
            _rowIndex = rowIndex;
            _column = column;
            _columns = new[] {column};
        }

        public TableUpdate(TableUpdateAction action, int rowIndex, IList<IReactiveColumn> columns)
        {
            _action = action;
            _rowIndex = rowIndex;
            _column = null;
            _columns = null;
            if (IsColumnUpdate(action))
            {
                if (columns == null || columns.Count == 0)
                {
                    throw new ArgumentException("Columns must have values for column updates", "columns");
                }
                _column = columns[0];
                _columns = columns;
            }
        }

        public TableUpdateAction Action
        {
            get { return _action; }
        }

        public int RowIndex
        {
            get { return _rowIndex; }
        }

        public IReactiveColumn Column
        {
            get { return _column; }
        }

        public IList<IReactiveColumn> Columns
        {
            get { return _columns; }
        }

        public bool IsRowUpdate()
        {
            return IsRowUpdate(Action);
        }

        public static bool IsRowUpdate(TableUpdate update)
        {
            return update.IsRowUpdate();
        }

        public static bool IsRowUpdate(TableUpdateAction action)
        {
            return action == TableUpdateAction.Add || action == TableUpdateAction.Delete;
        }
        
        public bool IsColumnUpdate()
        {
            return IsColumnUpdate(Action);
        }

        public static bool IsColumnUpdate(TableUpdate update)
        {
            return update.IsColumnUpdate();
        }

        public static bool IsColumnUpdate(TableUpdateAction action)
        {
            return action == TableUpdateAction.Update;
        }
    }
}