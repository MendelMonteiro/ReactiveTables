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
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework
{
    internal class ColumnChangePublisher : IColumnObserver
    {
        private readonly IReactiveColumn _column;
        private readonly HashSet<IObserver<RowUpdate>> _rowObservers;
        private readonly HashSet<IObserver<ColumnUpdate>> _columnObservers;

        public ColumnChangePublisher(IReactiveColumn column, HashSet<IObserver<RowUpdate>> rowObservers, HashSet<IObserver<ColumnUpdate>> columnObservers)
        {
            _column = column;
            _rowObservers = rowObservers;
            _columnObservers = columnObservers;
        }

        public void OnNext(int rowIndex)
        {
            var columnUpdate = new ColumnUpdate(_column, rowIndex);
            foreach (var observer in _columnObservers)
            {
                observer.OnNext(columnUpdate);
            }
        }

        public void OnError(Exception error, int rowIndex)
        {
            foreach (var observer in _rowObservers)
            {
                observer.OnError(error);
            }
        }

        public void OnCompleted(int rowIndex)
        {
            foreach (var observer in _rowObservers)
            {
                observer.OnCompleted();
            }
        }
    }
}
