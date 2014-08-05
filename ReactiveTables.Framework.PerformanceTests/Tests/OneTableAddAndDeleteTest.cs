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

namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    class OneTableAddAndDeleteTest : TestBase, ITest
    {
        private int _limit;
        private int _count;
        private int _halfWay;

        public OneTableAddAndDeleteTest(int? initialSize = null)
            : base(initialSize)
        {
        }

        public void Prepare(int limit)
        {
            _limit = limit;
            _halfWay = _limit / 2;
        }

        public void Iterate()
        {
            if (_count < _halfWay)
            {
                _count++;
                AddEntry(_count);
            }
            else
            {
                UpdateEntry(_count - _halfWay);
                _count++;
            }
        }

        public long Metric { get { return Table.RowCount; } }
    }
}
