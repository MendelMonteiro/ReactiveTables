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
using ReactiveTables.Framework.Columns.Calculated;

namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    internal class CalculatedColumnTableTest : ITest
    {
        private readonly ReactiveTable _table;

        public CalculatedColumnTableTest()
        {
            _table = new ReactiveTable();
            var idCol = new ReactiveColumn<int>("IdCol");
            _table.AddColumn(idCol);
            var textCol = new ReactiveColumn<string>("TextCol");
            _table.AddColumn(textCol);
            var valueCol = new ReactiveColumn<decimal>("ValueCol");
            _table.AddColumn(valueCol);

            _table.AddColumn(new ReactiveCalculatedColumn2<decimal, int, decimal>(
                                 "CalcCol1", idCol, valueCol, (id, val) => id*val));

            _table.AddColumn(new ReactiveCalculatedColumn2<string, int, string>(
                                 "CalcCol2", idCol, textCol, (id, text) => text + id));
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

            var calc1 = _table.GetValue<decimal>("CalcCol1", id);
            var calc2 = _table.GetValue<string>("CalcCol2", id);
        }

        public long Metric => _table.RowCount;
    }
}