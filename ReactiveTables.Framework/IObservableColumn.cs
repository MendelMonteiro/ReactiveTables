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
    /// The observable pair to <see cref="IColumnObserver"/>
    /// TODO: Modify this so that it we return ColumnUpdates that have both the before and after values, also return removes.
    /// </summary>
    public interface IObservableColumn //: IObservable<ColumnUpdate>
    {
        /*/// <summary>
        /// Subscribe to all changes from this column
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        IDisposable Subscribe(IObserver<ColumnUpdate> observer);*/
    }
}