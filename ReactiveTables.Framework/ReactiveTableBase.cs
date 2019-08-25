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
using ReactiveTables.Framework.Filters;
using ReactiveTables.Framework.Joins;

namespace ReactiveTables.Framework
{
    /// <summary>
    /// Contains functionality common to all tables
    /// </summary>
    public abstract class ReactiveTableBase : IReactiveTable
    {
        private readonly Lazy<PropertyChangedNotifier> _changeNotifier;

        public ReactiveTableBase()
        {
            _changeNotifier = new Lazy<PropertyChangedNotifier>(() => new PropertyChangedNotifier(this));
        }

        public abstract IReactiveColumn AddColumn(IReactiveColumn column, bool shouldSubscribe = true);
        public abstract T GetValue<T>(string columnId, int rowIndex);
        public abstract object GetValue(string columnId, int rowIndex);
        public abstract int RowCount { get; }
//        public abstract IDictionary<string, IReactiveColumn> Columns { get; }
        public abstract IReadOnlyList<IReactiveColumn> Columns { get; }

        public abstract IReactiveColumn GetColumnByIndex(int index);

        public PropertyChangedNotifier ChangeNotifier => _changeNotifier.Value;

        public abstract void ReplayRows(IObserver<TableUpdate> observer);
        public abstract int GetRowAt(int position);
        public abstract int GetPositionOfRow(int rowIndex);
        public abstract IReactiveColumn GetColumnByName(string columnId);
        public abstract bool GetColumnByName(string columnId, out IReactiveColumn column);

        public abstract IDisposable Subscribe(IObserver<TableUpdate> observer);

        public virtual IReactiveTable Filter(IReactivePredicate predicate)
        {
            return new FilteredTable(this, predicate);
        }

        public virtual IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner)
        {
            return new JoinedTable(this, otherTable, joiner);
        }
    }
}