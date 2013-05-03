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
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveTables.Framework
{
    public static class TableExtensions
    {
        public static IObservable<TableUpdate> RowUpdates(this IObservable<TableUpdate> table)
        {
            return table.Where(TableUpdate.IsRowUpdate);
        }

        public static IObservable<TableUpdate> ColumnUpdates(this IObservable<TableUpdate> table)
        {
            return table.Where(TableUpdate.IsColumnUpdate);
        }

        public static IDisposable ReplayAndSubscribe(this IReactiveTable table, Action<TableUpdate> onNext)
        {
            Subject<TableUpdate> rowObserver = new Subject<TableUpdate>();
            var subscription = rowObserver.Subscribe(onNext);
            table.ReplayRows(rowObserver);
            table.Subscribe(rowObserver);
            return subscription;
        }
    }
}