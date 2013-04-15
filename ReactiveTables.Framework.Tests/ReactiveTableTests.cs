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
using NUnit.Framework;

namespace ReactiveTables.Framework.Tests
{
    [TestFixture]
    public class ReactiveTableTests
    {
        [Test]
        public void TestAdd()
        {
            var table = TableTestHelper.CreateReactiveTable();

            var rowId1 = table.AddRow();
            Assert.AreEqual(1, table.RowCount);
            TableTestHelper.SetAndTestValue(table, rowId1, 123, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, "Hello", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, 321m, TestTableColumns.DecimalColumn);

            var rowId2 = table.AddRow();
            Assert.AreEqual(2, table.RowCount);
            TableTestHelper.SetAndTestValue(table, rowId2, 1234, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, "Hello 12", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, 3214m, TestTableColumns.DecimalColumn);
        }

        [Test]
        public void TestUpdate()
        {
            var table = TableTestHelper.CreateReactiveTable();

            var rowId1 = table.AddRow();
            Assert.AreEqual(1, table.RowCount);
            TableTestHelper.SetAndTestValue(table, rowId1, 123, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, "Hello", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, 321m, TestTableColumns.DecimalColumn);

            var rowId2 = table.AddRow();
            Assert.AreEqual(2, table.RowCount);
            TableTestHelper.SetAndTestValue(table, rowId2, 1234, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, "Hello 12", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, 3214m, TestTableColumns.DecimalColumn);

            TableTestHelper.SetAndTestValue(table, rowId2, 12341, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, "Hello", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, 321m, TestTableColumns.DecimalColumn);

            TableTestHelper.SetAndTestValue(table, rowId1, 321, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, "Hello 334", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, 32132m, TestTableColumns.DecimalColumn);
        }

        [Test]
        public void TestDelete()
        {
            var table = TableTestHelper.CreateReactiveTable();

            var rowId1 = table.AddRow();
            Assert.AreEqual(1, table.RowCount);
            TableTestHelper.SetAndTestValue(table, rowId1, 123, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, "Hello", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, 321m, TestTableColumns.DecimalColumn);

            var rowId2 = table.AddRow();
            Assert.AreEqual(2, table.RowCount);
            TableTestHelper.SetAndTestValue(table, rowId2, 1234, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, "Hello 12", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, 3214m, TestTableColumns.DecimalColumn);

            table.DeleteRow(rowId2);
            Assert.AreEqual(1, table.RowCount);

            table.DeleteRow(rowId1);
            Assert.AreEqual(0, table.RowCount);
        }
    }
}
