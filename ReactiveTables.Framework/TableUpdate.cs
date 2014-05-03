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
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework
{
    /// <summary>
    /// The action being performed to the row
    /// </summary>
    [Flags]
    public enum TableUpdateAction : short
    {
        Add = 1,
        Update = 2,
        Delete = 4,
    }

    /// <summary>
    /// Represents an update to a <see cref="ReactiveTable"/>
    /// </summary>
    public struct TableUpdate
    {
        private readonly TableUpdateAction _action;
        private readonly int _rowIndex;
        private readonly IReactiveColumn _column;

        /// <summary>
        /// Create new table update
        /// </summary>
        /// <param name="action"></param>
        /// <param name="rowIndex"></param>
        /// <param name="column"></param>
        public TableUpdate(TableUpdateAction action, int rowIndex, IReactiveColumn column = (IReactiveColumn) null)
        {
            _action = action;
            _rowIndex = rowIndex;
            _column = column;
        }

        /// <summary>
        /// Create a new Add update
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public static TableUpdate NewAddUpdate(int rowIndex)
        {
            return new TableUpdate(TableUpdateAction.Add, rowIndex);
        }

        /// <summary>
        /// Create a new Delete update
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public static TableUpdate NewDeleteUpdate(int rowIndex)
        {
            return new TableUpdate(TableUpdateAction.Delete, rowIndex);
        }

        /// <summary>
        /// Create a new Column Update (column value is being updated)
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static TableUpdate NewColumnUpdate(int rowIndex, IReactiveColumn column)
        {
            return new TableUpdate(TableUpdateAction.Update, rowIndex, column);
        }

        /// <summary>
        /// The action being performed in this update
        /// </summary>
        public TableUpdateAction Action
        {
            get { return _action; }
        }

        /// <summary>
        /// The row index of the table being affected
        /// </summary>
        public int RowIndex
        {
            get { return _rowIndex; }
        }

        /// <summary>
        /// The column being affected (or first column if multiple columns are affected)
        /// </summary>
        public IReactiveColumn Column
        {
            get { return _column; }
        }

        /// <summary>
        /// Whether this change affects the whole row
        /// </summary>
        /// <returns></returns>
        public bool IsRowUpdate()
        {
            return IsRowUpdate(Action);
        }

        /// <summary>
        /// Whether this change affects the whole row 
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public static bool IsRowUpdate(TableUpdate update)
        {
            return update.IsRowUpdate();
        }

        /// <summary>
        /// Whether this change affects the whole row 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool IsRowUpdate(TableUpdateAction action)
        {
            return action == TableUpdateAction.Add || action == TableUpdateAction.Delete;
        }
        
        /// <summary>
        /// Whether this change only affects one or more columns, but not the whole row.
        /// </summary>
        /// <returns></returns>
        public bool IsColumnUpdate()
        {
            return IsColumnUpdate(Action);
        }

        /// <summary>
        /// Whether this change only affects one or more columns, but not the whole row.        
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public static bool IsColumnUpdate(TableUpdate update)
        {
            return update.IsColumnUpdate();
        }

        /// <summary>
        /// Whether this change only affects one or more columns, but not the whole row.        
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool IsColumnUpdate(TableUpdateAction action)
        {
            return action == TableUpdateAction.Update;
        }

        public override string ToString()
        {
            return string.Format("Action: {0}, RowIndex: {1}, Column: {2}", _action, _rowIndex, _column == null ? "null" : _column.ColumnId);
        }
    }
}