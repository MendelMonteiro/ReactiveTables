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
using NUnit.Framework;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Synchronisation;
using System.Linq;

namespace ReactiveTables.Framework.Tests
{
    [TestFixture]
    public class BatchedPassThroughTableTests
    {

        [Test]
        public void TestAdd()
        {
            ReactiveTable target = TestTableHelper.CreateReactiveTable();
            ReactiveBatchedPassThroughTable source = new ReactiveBatchedPassThroughTable(target, new DefaultThreadMarshaller());

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            target.RowUpdates().Subscribe(updateHandler.OnRowUpdate);

            TestAdd(source, target, updateHandler);
        }

        private static void TestAdd(ReactiveBatchedPassThroughTable source, ReactiveTable target, RowUpdateHandler updateHandler)
        {
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

            source.SynchroniseChanges();
            Assert.AreEqual(2, target.RowCount);

            TestTableHelper.TestValue(target, updateHandler.RowsUpdated[0], 101, TestTableColumns.IdColumn);
            TestTableHelper.TestValue(target, updateHandler.RowsUpdated[0], "Blah", TestTableColumns.StringColumn);
            TestTableHelper.TestValue(target, updateHandler.RowsUpdated[0], 4324m, TestTableColumns.DecimalColumn);

            TestTableHelper.TestValue(target, updateHandler.RowsUpdated[1], 102, TestTableColumns.IdColumn);
            TestTableHelper.TestValue(target, updateHandler.RowsUpdated[1], "Blah2", TestTableColumns.StringColumn);
            TestTableHelper.TestValue(target, updateHandler.RowsUpdated[1], 42m, TestTableColumns.DecimalColumn);
        }

        [Test]
        public void TestUpdate()
        {
            ReactiveTable target = TestTableHelper.CreateReactiveTable();
            ReactiveBatchedPassThroughTable source = new ReactiveBatchedPassThroughTable(target, new DefaultThreadMarshaller());

            RowUpdateHandler rowUpdates = new RowUpdateHandler();
            target.RowUpdates().Subscribe(rowUpdates.OnRowUpdate);
            ColumnUpdateHandler columnUpdates = new ColumnUpdateHandler();
            target.ColumnUpdates().Subscribe(columnUpdates.OnColumnUpdate);

            TestAdd(source, target, rowUpdates);

            source.SetValue(TestTableColumns.StringColumn, 0, "Changed");
            Assert.AreEqual("Blah", target.GetValue<string>(TestTableColumns.StringColumn, 0));

            source.SynchroniseChanges();

            Assert.AreEqual("Changed", target.GetValue<string>(TestTableColumns.StringColumn, 0));
        }

        [Test]
        public void TestUpdateOnlyLastValue()
        {
            ReactiveTable target = TestTableHelper.CreateReactiveTable();
            ReactiveBatchedPassThroughTable source = new ReactiveBatchedPassThroughTable(target, new DefaultThreadMarshaller(), true);

            RowUpdateHandler rowUpdates = new RowUpdateHandler();
            target.RowUpdates().Subscribe(rowUpdates.OnRowUpdate);
            ColumnUpdateHandler columnUpdates = new ColumnUpdateHandler();
            target.ColumnUpdates().Subscribe(columnUpdates.OnColumnUpdate);

            TestAdd(source, target, rowUpdates);

            columnUpdates.LastColumnsUpdated.Clear();

            source.SetValue(TestTableColumns.StringColumn, 0, "Changed");
            Assert.AreEqual("Blah", target.GetValue<string>(TestTableColumns.StringColumn, 0));
            source.SetValue(TestTableColumns.StringColumn, 0, "Changed2");
            Assert.AreEqual("Blah", target.GetValue<string>(TestTableColumns.StringColumn, 0));

            source.SynchroniseChanges();

            // Check that there was only one col update fro the StringColumn
            Assert.AreEqual(1, columnUpdates.LastColumnsUpdated.Count);
            Assert.AreEqual(columnUpdates.LastColumnsUpdated.Last(), TestTableColumns.StringColumn);
            // And that the value is the last one
            Assert.AreEqual("Changed2", target.GetValue<string>(TestTableColumns.StringColumn, 0));
        }

        [Test]
        public void TestDelete()
        {
            ReactiveTable target = TestTableHelper.CreateReactiveTable();
            ReactiveBatchedPassThroughTable source = new ReactiveBatchedPassThroughTable(target, new DefaultThreadMarshaller());

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            target.RowUpdates().Subscribe(updateHandler.OnRowUpdate);

            TestAdd(source, target, updateHandler);

            source.DeleteRow(0);
            Assert.AreEqual(2, target.RowCount);

            source.SynchroniseChanges();
            Assert.AreEqual(1, target.RowCount);
            Assert.AreEqual(102, target.GetValue<int>(TestTableColumns.IdColumn, 1));

            source.DeleteRow(1);
            Assert.AreEqual(1, target.RowCount);

            source.SynchroniseChanges();
            Assert.AreEqual(0, target.RowCount);

            source.AddRow();
            Assert.AreEqual(0, target.RowCount);
            
            source.SynchroniseChanges();
            Assert.AreEqual(1, target.RowCount);
        }
    }
}