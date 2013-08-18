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
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using NUnit.Framework;

namespace ReactiveTables.Framework.Tests
{
    [TestFixture]
    public class ReactiveTableTests
    {
        [Test]
        public void TestAdd()
        {
            var table = TestTableHelper.CreateReactiveTable();

            var rowId1 = table.AddRow();
            Assert.AreEqual(1, table.RowCount);
            TestTableHelper.SetAndTestValue(table, rowId1, 123, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(table, rowId1, "Hello", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(table, rowId1, 321m, TestTableColumns.DecimalColumn);

            var rowId2 = table.AddRow();
            Assert.AreEqual(2, table.RowCount);
            TestTableHelper.SetAndTestValue(table, rowId2, 1234, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(table, rowId2, "Hello 12", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(table, rowId2, 3214m, TestTableColumns.DecimalColumn);
        }

        [Test]
        public void TestUpdate()
        {
            var table = TestTableHelper.CreateReactiveTable();

            var rowId1 = table.AddRow();
            Assert.AreEqual(1, table.RowCount);
            TestTableHelper.SetAndTestValue(table, rowId1, 123, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(table, rowId1, "Hello", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(table, rowId1, 321m, TestTableColumns.DecimalColumn);

            var rowId2 = table.AddRow();
            Assert.AreEqual(2, table.RowCount);
            TestTableHelper.SetAndTestValue(table, rowId2, 1234, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(table, rowId2, "Hello 12", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(table, rowId2, 3214m, TestTableColumns.DecimalColumn);

            TestTableHelper.SetAndTestValue(table, rowId2, 12341, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(table, rowId2, "Hello", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(table, rowId2, 321m, TestTableColumns.DecimalColumn);

            TestTableHelper.SetAndTestValue(table, rowId1, 321, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(table, rowId1, "Hello 334", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(table, rowId1, 32132m, TestTableColumns.DecimalColumn);
        }

        [Test]
        public void TestDelete()
        {
            var table = TestTableHelper.CreateReactiveTable();

            var rowId1 = table.AddRow();
            Assert.AreEqual(1, table.RowCount);
            TestTableHelper.SetAndTestValue(table, rowId1, 123, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(table, rowId1, "Hello", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(table, rowId1, 321m, TestTableColumns.DecimalColumn);

            var rowId2 = table.AddRow();
            Assert.AreEqual(2, table.RowCount);
            TestTableHelper.SetAndTestValue(table, rowId2, 1234, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(table, rowId2, "Hello 12", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(table, rowId2, 3214m, TestTableColumns.DecimalColumn);

            table.DeleteRow(rowId2);
            Assert.AreEqual(1, table.RowCount);

            table.DeleteRow(rowId1);
            Assert.AreEqual(0, table.RowCount);
        }

        [Test]
        public void TestSearch()
        {
            var table = TestTableHelper.CreateIndexedReactiveTable();

            var addedRowIndex = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, addedRowIndex, 1);
            table.SetValue(TestTableColumns.StringColumn, addedRowIndex, "blah");
            table.SetValue(TestTableColumns.DecimalColumn, addedRowIndex, 324.34m);

            int rowIndex = table.Find(TestTableColumns.StringColumn, "blah");
            Assert.AreEqual(addedRowIndex, rowIndex);

            rowIndex = table.Find(TestTableColumns.StringColumn, "blah2");
            Assert.AreEqual(-1, rowIndex);

            table.SetValue(TestTableColumns.StringColumn, addedRowIndex, "hello");

            rowIndex = table.Find(TestTableColumns.StringColumn, "blah");
            Assert.AreEqual(-1, rowIndex);

            rowIndex = table.Find(TestTableColumns.StringColumn, "hello");
            Assert.AreEqual(addedRowIndex, rowIndex);
        }

        [Test]
        public void TestSearchMultipleRows()
        {
            var table = TestTableHelper.CreateIndexedReactiveTable();

            var addedRowIndex1 = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, addedRowIndex1, 1);
            table.SetValue(TestTableColumns.StringColumn, addedRowIndex1, "blah");
            table.SetValue(TestTableColumns.DecimalColumn, addedRowIndex1, 324.34m);

            var addedRowIndex2 = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, addedRowIndex2, 2);
            Assert.Throws<InvalidOperationException>(() => table.SetValue(TestTableColumns.StringColumn, addedRowIndex2, "blah"));
        }
    }
}