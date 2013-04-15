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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Tests.Columns
{
    [TestFixture]
    public class FieldRowManagerTests
    {
        [Test]
        public void Basic()
        {
            FieldRowManager rowManager = new FieldRowManager();
            int rowIndex = rowManager.AddRow();
            rowManager.DeleteRow(rowIndex);
            Assert.AreEqual(0, rowManager.RowCount);

            rowIndex = rowManager.AddRow();
            rowManager.DeleteRow(rowIndex);
            Assert.AreEqual(0, rowManager.RowCount);

            var rowIndex2 = rowManager.AddRow();
            var rowIndex3 = rowManager.AddRow();
            var rowIndex4 = rowManager.AddRow();
            rowManager.DeleteRow(rowIndex3);
            Assert.AreEqual(2, rowManager.RowCount);
            rowManager.DeleteRow(rowIndex4);
            Assert.AreEqual(1, rowManager.RowCount);
            var rowIndex5 = rowManager.AddRow();
            Assert.AreEqual(2, rowManager.RowCount);
            rowManager.DeleteRow(rowIndex5);
            Assert.AreEqual(1, rowManager.RowCount);
            rowManager.DeleteRow(rowIndex2);
            Assert.AreEqual(0, rowManager.RowCount);
        }
    }
}
