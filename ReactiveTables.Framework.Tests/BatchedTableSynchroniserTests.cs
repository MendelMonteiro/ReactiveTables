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
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Threading;
using NUnit.Framework;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Synchronisation;

namespace ReactiveTables.Framework.Tests
{
    [TestFixture]
    public class BatchedTableSynchroniserTests
    {
        const int delay = 250;

        [Test]
        public void TestAdd()
        {
            var source = TestTableHelper.CreateReactiveTable();
            var target = TestTableHelper.CreateReactiveTable();
            var synchroniser = new BatchedTableSynchroniser(source, target, new DefaultThreadMarshaller(), TimeSpan.FromMilliseconds(delay));

            var updateHandler = new RowUpdateHandler();
            target.RowUpdates().Subscribe(updateHandler.OnRowUpdate);

            var sourceRow1 = source.AddRow();
            Assert.AreEqual(0, target.RowCount);

            TestTableHelper.SetAndTestValueNotPresent(source, target, sourceRow1, updateHandler.LastRowUpdated, 101, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValueNotPresent(source, target, sourceRow1, updateHandler.LastRowUpdated, "Blah", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValueNotPresent(source, target, sourceRow1, updateHandler.LastRowUpdated, 4324m, TestTableColumns.DecimalColumn);

            var sourceRow2 = source.AddRow();
            Assert.AreEqual(0, target.RowCount);

            TestTableHelper.SetAndTestValueNotPresent(source, target, sourceRow2, updateHandler.LastRowUpdated, 102, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValueNotPresent(source, target, sourceRow2, updateHandler.LastRowUpdated, "Blah2", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValueNotPresent(source, target, sourceRow2, updateHandler.LastRowUpdated, 42m, TestTableColumns.DecimalColumn);

            Thread.Sleep(delay + 50);
            Assert.AreEqual(2, target.RowCount);

            TestTableHelper.TestValue(target, updateHandler.RowsUpdated[0], 101, TestTableColumns.IdColumn);
            TestTableHelper.TestValue(target, updateHandler.RowsUpdated[0], "Blah", TestTableColumns.StringColumn);
            TestTableHelper.TestValue(target, updateHandler.RowsUpdated[0], 4324m, TestTableColumns.DecimalColumn);

            TestTableHelper.TestValue(target, updateHandler.RowsUpdated[1], 102, TestTableColumns.IdColumn);
            TestTableHelper.TestValue(target, updateHandler.RowsUpdated[1], "Blah2", TestTableColumns.StringColumn);
            TestTableHelper.TestValue(target, updateHandler.RowsUpdated[1], 42m, TestTableColumns.DecimalColumn);
        }

//        [Test]
        public void TestUpdate()
        {
            var source = TestTableHelper.CreateReactiveTable();
            var target = TestTableHelper.CreateReactiveTable();
            var synchroniser = new BatchedTableSynchroniser(source, target, new DefaultThreadMarshaller(), TimeSpan.FromMilliseconds(delay));

            var updateHandler = new RowUpdateHandler();
            target.RowUpdates().Subscribe(updateHandler.OnRowUpdate);

            var sourceRow1 = source.AddRow();
            Assert.AreEqual(1, target.RowCount);

            TestTableHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, 101, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, "Blah", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, 4324m, TestTableColumns.DecimalColumn);

            TestTableHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, 103, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, "Blah2", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, 43999m, TestTableColumns.DecimalColumn);

            var sourceRow2 = source.AddRow();
            Assert.AreEqual(2, target.RowCount);

            TestTableHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, 102, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, "Blah2", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, 42m, TestTableColumns.DecimalColumn);

            TestTableHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, 104, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, "Blah4", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, 421111m, TestTableColumns.DecimalColumn);
        }

//        [Test]
        public void TestDelete()
        {
            var source = TestTableHelper.CreateReactiveTable();
            var target = TestTableHelper.CreateReactiveTable();
            var synchroniser = new BatchedTableSynchroniser(source, target, new DefaultThreadMarshaller(), TimeSpan.FromMilliseconds(delay));

            var updateHandler = new RowUpdateHandler();
            target.RowUpdates().Subscribe(updateHandler.OnRowUpdate);

            var sourceRow1 = source.AddRow();
            Assert.AreEqual(1, target.RowCount);

            TestTableHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, 101, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, "Blah", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, 4324m, TestTableColumns.DecimalColumn);

            var sourceRow2 = source.AddRow();
            Assert.AreEqual(2, target.RowCount);

            var targetRow2 = updateHandler.LastRowUpdated;
            TestTableHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, 102, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, "Blah2", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, 42m, TestTableColumns.DecimalColumn);

            source.DeleteRow(sourceRow1);
            Assert.AreEqual(1, target.RowCount);
            Assert.AreEqual(102, target.GetValue<int>(TestTableColumns.IdColumn, targetRow2));

            source.DeleteRow(sourceRow2);
            Assert.AreEqual(0, target.RowCount);

            var sourceRow3 = source.AddRow();
            Assert.AreEqual(1, target.RowCount);

            TestTableHelper.SetAndTestValue(source, target, sourceRow3, updateHandler.LastRowUpdated, 101, TestTableColumns.IdColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow3, updateHandler.LastRowUpdated, "Blah", TestTableColumns.StringColumn);
            TestTableHelper.SetAndTestValue(source, target, sourceRow3, updateHandler.LastRowUpdated, 4324m, TestTableColumns.DecimalColumn);
        }
    }
}
