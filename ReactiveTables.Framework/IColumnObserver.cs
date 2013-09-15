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

namespace ReactiveTables.Framework
{
    /// <summary>
    /// IObserver implementation that notifies exceptions/completions with the row index being processed.
    /// </summary>
    public interface IColumnObserver 
    {
        /// <summary>
        /// Column has been modified at row <see cref="rowIndex"/>
        /// </summary>
        /// <param name="rowIndex"></param>
        void OnNext(int rowIndex);
        /// <summary>
        /// Error has occurred whe processing column at row <see cref="rowIndex"/>
        /// </summary>
        /// <param name="error"></param>
        /// <param name="rowIndex"></param>
        void OnError(Exception error, int rowIndex);
        /// <summary>
        /// Completed processing of row at <see cref="rowIndex"/>
        /// </summary>
        /// <param name="rowIndex"></param>
        void OnCompleted(int rowIndex);
    }
}