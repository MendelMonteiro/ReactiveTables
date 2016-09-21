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
    class OneTableWriteAndUpdateTest : TestBase, ITest
    {
        private int _iterationCount;

        public OneTableWriteAndUpdateTest(int? initialSize = null)
            : base(initialSize)
        {
        }

        public void Prepare(int limit)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public void Iterate()
        {
            _iterationCount++;
            var id = AddEntry(_iterationCount);

            if (id > 0)
            {
                var previousRow = id-1;
                UpdateEntry(previousRow);

                if (id > 1)
                {
                    DeleteEntry(previousRow - 1);
                }
            }
        }

        public long Metric => Table.RowCount;
    }
}
