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
using System.Reactive.Subjects;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework
{
    /// <summary>
    /// Notifies a collection of observers of changes occurring on a <see cref="ReactiveColumn{T}"/>
    /// </summary>
    internal class ColumnChangePublisher : IColumnObserver
    {
        private readonly IReactiveColumn _column;
        private readonly Subject<TableUpdate> _observers;

        public ColumnChangePublisher(IReactiveColumn column, Subject<TableUpdate> observers)
        {
            _column = column;
            _observers = observers;
        }

        public void OnNext(int rowIndex)
        {
            var columnUpdate = new TableUpdate(TableUpdate.TableUpdateAction.Update, rowIndex, _column);
            _observers.OnNext(columnUpdate);
        }

        public void OnError(Exception error, int rowIndex)
        {
            _observers.OnError(error);
        }

        public void OnCompleted(int rowIndex)
        {
            _observers.OnCompleted();
        }
    }
}