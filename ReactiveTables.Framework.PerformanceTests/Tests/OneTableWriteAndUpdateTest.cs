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
    class OneTableWriteAndUpdateTest : ITest
    {
        private const string ValueCol = "ValueCol";
        private const string TextCol = "TextCol";
        private const string IdCol = "IdCol";
        private readonly ReactiveTable _table;
        private int _iterationCount;

        public OneTableWriteAndUpdateTest(int? initialSize = null)
        {
            _table = new ReactiveTable();
            _table.AddColumn(new ReactiveColumn<int>(IdCol, initialSize: initialSize));
            _table.AddColumn(new ReactiveColumn<string>(TextCol, initialSize: initialSize));
            _table.AddColumn(new ReactiveColumn<decimal>(ValueCol, initialSize: initialSize));
        }

        public void Iterate()
        {
            var id = _table.AddRow();
            _iterationCount++;
            _table.SetValue(IdCol, id, _iterationCount);
            _table.SetValue(TextCol, id, "Some longer string that should take up some more space");
            _table.SetValue(ValueCol, id, 23213214214.3423m);

            if (id > 0)
            {
                var previousRow = id-1;
                _table.SetValue(ValueCol, previousRow, 98598643543m);
                _table.SetValue(TextCol, previousRow, "An updated string");

                if (id > 1)
                {
                    _table.DeleteRow(previousRow - 1);
                }
            }
        }

        public long Metric
        {
            get { return _table.RowCount; }
        }
    }
}
