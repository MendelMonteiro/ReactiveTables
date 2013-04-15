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
along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Utils;

namespace ReactiveTables.Framework
{
    public class JoinedTable : IReactiveTable
    {
        private readonly IReactiveTable _leftTable;
        private readonly IReactiveTable _rightTable;
        private readonly IReactiveTableJoiner _joiner;
        private readonly HashSet<IObserver<RowUpdate>> _rowObservers = new HashSet<IObserver<RowUpdate>>();
        private readonly HashSet<IObserver<ColumnUpdate>> _columnObservers = new HashSet<IObserver<ColumnUpdate>>();

        public JoinedTable(IReactiveTable leftTable, IReactiveTable rightTable, IReactiveTableJoiner joiner)
        {
            _leftTable = leftTable;
            _rightTable = rightTable;
            _joiner = joiner;

            Columns = new Dictionary<string, IReactiveColumn>();
            _leftTable.Columns.CopyTo(Columns);
            _rightTable.Columns.CopyTo(Columns);
            
            ChangeNotifier = new PropertyChangedNotifier(this);
        }

        public IDisposable Subscribe(IObserver<RowUpdate> observer)
        {
            _joiner.AddRowObserver(observer);
            _rowObservers.Add(observer);
            return new SubscriptionToken<JoinedTable, IObserver<RowUpdate>>(this, observer);
        }

        public void Unsubscribe(IObserver<RowUpdate> observer)
        {
            _rowObservers.Remove(observer);
        }

        public IDisposable Subscribe(IObserver<ColumnUpdate> observer)
        {
            _leftTable.Subscribe(observer);
            _rightTable.Subscribe(observer);
            _columnObservers.Add(observer);
            return new SubscriptionToken<JoinedTable, IObserver<ColumnUpdate>>(this, observer);
        }

        public void Unsubscribe(IObserver<ColumnUpdate> observer)
        {
            _leftTable.Unsubscribe(observer);
            _rightTable.Unsubscribe(observer);
            _columnObservers.Remove(observer);
        }

        public void AddColumn(IReactiveColumn column)
        {
            // Add calc'ed columns
            Columns.Add(column.ColumnId, column);
            var joinableCol = column as IReactiveJoinableColumn;
            if (joinableCol != null)
                joinableCol.SetJoiner(_joiner);

            // Need to subscribe to changes in calculated columns
            column.Subscribe(new ColumnChangePublisher(column, _rowObservers, _columnObservers));
        }

        public T GetValue<T>(string columnId, int rowIndex)
        {
            IReactiveColumn column;
            // Use the joiner for when the column is defined directly on them
            // if the table is a joined table delegate the joining to it.
            if (_leftTable.Columns.TryGetValue(columnId, out column))
            {
                return _leftTable.GetValue<T>(columnId, _joiner.GetRowIndex(column, rowIndex));
            }
            if (_rightTable.Columns.TryGetValue(columnId, out column))
            {
                return _rightTable.GetValue<T>(columnId, _joiner.GetRowIndex(column, rowIndex));
            }
            // Otherwise return calc'ed columns
            return GetColumn<T>(columnId).GetValue(rowIndex).Value;
        }

        private IReactiveColumn<T> GetColumn<T>(string columnId)
        {
            return (IReactiveColumn<T>) Columns[columnId];
        }

        public int RowCount
        {
            get
            {
                // Delegate to the joiner
                return _joiner.RowCount;
            }
        }

        public Dictionary<string, IReactiveColumn> Columns { get; private set; }

        public PropertyChangedNotifier ChangeNotifier { get; private set; }

        public IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner)
        {
            return new JoinedTable(this, otherTable, joiner);
        }
    }
}
