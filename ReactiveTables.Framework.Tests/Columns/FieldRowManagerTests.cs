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

        [Test]
        public void TestGetRowAt()
        {
            var rowManager = new FieldRowManager();
            // {}
            Assert.AreEqual(-1, rowManager.GetRowAt(0));
            Assert.AreEqual(-1, rowManager.GetRowAt(1));
            Assert.AreEqual(-1, rowManager.GetRowAt(-1));

            rowManager = new FieldRowManager();
            // {X,1}
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.DeleteRow(0);

            Assert.AreEqual(1, rowManager.GetRowAt(0));
            Assert.AreEqual(-1, rowManager.GetRowAt(int.MaxValue));
            Assert.AreEqual(-1, rowManager.GetRowAt(int.MinValue));

            // {0,X,1}
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.DeleteRow(1);

            Assert.AreEqual(0, rowManager.GetRowAt(0));
            Assert.AreEqual(2, rowManager.GetRowAt(1));

            // {0,X,1,X}
            rowManager = new FieldRowManager();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.DeleteRow(1);
            rowManager.DeleteRow(3);

            Assert.AreEqual(0, rowManager.GetRowAt(0));
            Assert.AreEqual(2, rowManager.GetRowAt(1));
            Assert.AreEqual(-1, rowManager.GetRowAt(2));

            // {X,0,X,1}
            rowManager = new FieldRowManager();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.DeleteRow(0);
            rowManager.DeleteRow(2);

            Assert.AreEqual(1, rowManager.GetRowAt(0));
            Assert.AreEqual(3, rowManager.GetRowAt(1));

            // {X,X,X,X,0,1,2,3}
            rowManager = new FieldRowManager();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.DeleteRow(0);
            rowManager.DeleteRow(1);
            rowManager.DeleteRow(2);
            rowManager.DeleteRow(3);
            Assert.AreEqual(4, rowManager.GetRowAt(0));
            Assert.AreEqual(5, rowManager.GetRowAt(1));
            Assert.AreEqual(6, rowManager.GetRowAt(2));
            Assert.AreEqual(7, rowManager.GetRowAt(3));

            // {0,1,X,X,X,2,3}
            rowManager = new FieldRowManager();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.DeleteRow(2);
            rowManager.DeleteRow(3);
            rowManager.DeleteRow(4);
            Assert.AreEqual(0, rowManager.GetRowAt(0));
            Assert.AreEqual(1, rowManager.GetRowAt(1));
            Assert.AreEqual(5, rowManager.GetRowAt(2));
            Assert.AreEqual(6, rowManager.GetRowAt(3));
        }

        [Test]
        public void TestGetRowPosition()
        {
            var rowManager = new FieldRowManager();

            Assert.AreEqual(-1, rowManager.GetPositionOfRow(int.MinValue));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(-1));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(0));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(1));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(int.MaxValue));

            // {0,1,2,3}
            rowManager = new FieldRowManager();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();

            Assert.AreEqual(0, rowManager.GetPositionOfRow(0));
            Assert.AreEqual(1, rowManager.GetPositionOfRow(1));
            Assert.AreEqual(2, rowManager.GetPositionOfRow(2));
            Assert.AreEqual(3, rowManager.GetPositionOfRow(3));

            // {X,X,X,X}
            rowManager = new FieldRowManager();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.DeleteRow(0);
            rowManager.DeleteRow(1);
            rowManager.DeleteRow(2);
            rowManager.DeleteRow(3);

            Assert.AreEqual(-1, rowManager.GetPositionOfRow(0));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(1));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(2));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(3));

            // {X,1,X,2}
            rowManager = new FieldRowManager();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.DeleteRow(0);
            rowManager.DeleteRow(2);

            Assert.AreEqual(-1, rowManager.GetPositionOfRow(0));
            Assert.AreEqual(0, rowManager.GetPositionOfRow(1));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(2));
            Assert.AreEqual(1, rowManager.GetPositionOfRow(3));

            // {0,X,2,X}
            rowManager = new FieldRowManager();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.DeleteRow(1);
            rowManager.DeleteRow(3);

            Assert.AreEqual(0, rowManager.GetPositionOfRow(0));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(1));
            Assert.AreEqual(1, rowManager.GetPositionOfRow(2));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(3));

            // {X,X,2,3}
            rowManager = new FieldRowManager();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.DeleteRow(0);
            rowManager.DeleteRow(1);

            Assert.AreEqual(-1, rowManager.GetPositionOfRow(0));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(1));
            Assert.AreEqual(0, rowManager.GetPositionOfRow(2));
            Assert.AreEqual(1, rowManager.GetPositionOfRow(3));
            
            // {0,1,X,X}
            rowManager = new FieldRowManager();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.AddRow();
            rowManager.DeleteRow(2);
            rowManager.DeleteRow(3);

            Assert.AreEqual(0, rowManager.GetPositionOfRow(0));
            Assert.AreEqual(1, rowManager.GetPositionOfRow(1));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(2));
            Assert.AreEqual(-1, rowManager.GetPositionOfRow(3));
        }
    }
}