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

namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    internal class OneWideTableTest : ITest
    {
        private readonly ReactiveTable _table;

        public OneWideTableTest()
        {
            _table = new ReactiveTable();
            _table.AddColumn(new ReactiveColumn<int>("IdCol"));
            _table.AddColumn(new ReactiveColumn<string>("TextCol"));
            _table.AddColumn(new ReactiveColumn<decimal>("ValueCol1"));
            _table.AddColumn(new ReactiveColumn<double>("ValueCol2"));
            _table.AddColumn(new ReactiveColumn<float>("ValueCol3"));
            _table.AddColumn(new ReactiveColumn<bool>("ValueCol4"));
            _table.AddColumn(new ReactiveColumn<string>("ValueCol5"));
            _table.AddColumn(new ReactiveColumn<DateTime>("ValueCol6"));
        }

        public void Prepare(int limit)
        {
        }

        public void Iterate()
        {
            var id = _table.AddRow();
            _table.SetValue("IdCol", id, 1);
            _table.SetValue("TextCol", id, "Some longer string that should take up some more space");
            _table.SetValue("ValueCol1", id, 23213214214.3423m);
            _table.SetValue("ValueCol2", id, 23213214214.3423d);
            _table.SetValue("ValueCol3", id, 23213214214.3423f);
            _table.SetValue("ValueCol4", id, true);
            _table.SetValue("ValueCol5", id, "Another long string");
            _table.SetValue("ValueCol6", id, DateTime.Now);
        }

        public long Metric => _table.RowCount;
    }
}