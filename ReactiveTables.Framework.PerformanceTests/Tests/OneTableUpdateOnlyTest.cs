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

using System.Runtime.CompilerServices;

namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    class OneTableUpdateOnlyTest : TestBase, ITest
    {
        private int _iterationCount;
        private int _limit;

        public OneTableUpdateOnlyTest(int? initialSize = null)
            : base(initialSize)
        {
        }

        public void Prepare(int limit)
        {
            _limit = limit;
            for (int i = 0; i < limit; i++)
            {
                AddEntry(_iterationCount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public void Iterate()
        {
            _iterationCount++;
            // Wrap around
            if (_iterationCount >= _limit)
            {
                _iterationCount = 0;
            }
            UpdateEntry(_iterationCount);
        }

        public long Metric { get { return Table.RowCount; } }
    }
}
