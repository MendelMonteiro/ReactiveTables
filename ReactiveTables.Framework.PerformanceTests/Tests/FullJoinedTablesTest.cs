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
using ReactiveTables.Framework.Joins;

namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    internal class FullJoinedTablesTest : ITest
    {
        private readonly ReactiveTable _table;
        private readonly ReactiveTable _table2;
        private readonly IReactiveTable _joinedTable;

        public FullJoinedTablesTest()
        {
            _table = new ReactiveTable();
            _table.AddColumn(new ReactiveColumn<int>("IdCol"));
            _table.AddColumn(new ReactiveColumn<string>("TextCol"));
            _table.AddColumn(new ReactiveColumn<decimal>("ValueCol"));

            _table2 = new ReactiveTable();
            _table2.AddColumn(new ReactiveColumn<int>("IdCol2"));
            _table2.AddColumn(new ReactiveColumn<string>("TextCol2"));
            _table2.AddColumn(new ReactiveColumn<decimal>("ValueCol2"));

            _joinedTable = _table.Join(_table2, new Join<int>(_table, "IdCol", _table2, "IdCol2"));
        }

        public void Prepare(int limit)
        {
        }

        public void Iterate()
        {
            var id = _table.AddRow();
            _table.SetValue("IdCol", id, 1);
            _table.SetValue("TextCol", id, "Some longer string that should take up some more space");
            _table.SetValue("ValueCol", id, 23213214214.3423m);

            var id2 = _table2.AddRow();
            _table2.SetValue("IdCol2", id2, 1);
            _table2.SetValue("TextCol2", id2, "Some longer string that should take up some more space");
            _table2.SetValue("ValueCol2", id2, 23213214214.3423m);
        }

        public long Metric => _joinedTable.RowCount;
    }
}