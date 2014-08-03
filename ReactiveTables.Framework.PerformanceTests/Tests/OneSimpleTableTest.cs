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

namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    internal class OneSimpleTableTest : ITest
    {
        private readonly ReactiveTable _table;

        public OneSimpleTableTest(int? initialSize = null)
        {
            _table = new ReactiveTable();
            _table.AddColumn(new ReactiveColumn<int>("IdCol", initialSize: initialSize));
            _table.AddColumn(new ReactiveColumn<string>("TextCol", initialSize: initialSize));
            _table.AddColumn(new ReactiveColumn<decimal>("ValueCol", initialSize: initialSize));
        }

        public void Iterate()
        {
            var id = _table.AddRow();
            _table.SetValue("IdCol", id, 1);
            _table.SetValue("TextCol", id, "Some longer string that should take up some more space");
            _table.SetValue("ValueCol", id, 23213214214.3423m);
        }

        public long Metric
        {
            get { return _table.RowCount; }
        }
    }
}