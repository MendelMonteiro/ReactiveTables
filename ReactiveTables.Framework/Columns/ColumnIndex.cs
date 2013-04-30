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

using ReactiveTables.Framework.Utils;

namespace ReactiveTables.Framework.Columns
{
    public interface IColumnIndex<in T>
    {
        int GetRow(T value);
        void SetRowValue(int rowIndex, T value);
        void RemoveRowValue(int rowIndex);
    }

    public class ColumnIndex<T> : IColumnIndex<T>
    {
        /// <summary>
        /// Use something a little bit more memory efficient like a RedBlack tree that 
        /// </summary>
        private readonly BidirectionalDictionary<T, int> _valueRows = new BidirectionalDictionary<T, int>();

        public int GetRow(T value)
        {
            int rowIndex;
            if (_valueRows.TryGetValue(value, out rowIndex))
            {
                return rowIndex;
            }

            return -1;
        }

        public void SetRowValue(int rowIndex, T value)
        {
            _valueRows[value] = rowIndex;
        }

        public void RemoveRowValue(int rowIndex)
        {
            _valueRows.RemoveByValue(rowIndex);
        }
    }
}