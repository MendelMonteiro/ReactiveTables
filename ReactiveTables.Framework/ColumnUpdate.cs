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

using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework
{
    public struct ColumnUpdate
    {
        private readonly IReactiveColumn _column;
        private readonly int _rowIndex;

        public ColumnUpdate(IReactiveColumn column, int rowIndex)
        {
            _column = column;
            _rowIndex = rowIndex;
        }

        public IReactiveColumn Column
        {
            get { return _column; }
        }

        public int RowIndex
        {
            get { return _rowIndex; }
        }
    }
}