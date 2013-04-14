using System;
using System.Threading;
using NUnit.Framework;
using ReactiveTables.Framework.Marshalling;

namespace ReactiveTables.Framework.Tests
{
    [TestFixture]
    public class BatchedTableSynchroniserTests
    {
        const int delay = 250;

        [Test]
        public void TestAdd()
        {
            ReactiveTable source = TableTestHelper.CreateReactiveTable();
            ReactiveTable target = TableTestHelper.CreateReactiveTable();
            BatchedTableSynchroniser synchroniser = new BatchedTableSynchroniser(source, target, new DefaultThreadMarshaller(), TimeSpan.FromMilliseconds(delay));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            target.Subscribe(new DelegateObserver<RowUpdate>(updateHandler.OnRowUpdate, null, null));

            var sourceRow1 = source.AddRow();
            Assert.AreEqual(0, target.RowCount);

            TableTestHelper.SetAndTestValueNotPresent(source, target, sourceRow1, updateHandler.LastRowUpdated, 101, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValueNotPresent(source, target, sourceRow1, updateHandler.LastRowUpdated, "Blah", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValueNotPresent(source, target, sourceRow1, updateHandler.LastRowUpdated, 4324m, TestTableColumns.DecimalColumn);

            var sourceRow2 = source.AddRow();
            Assert.AreEqual(0, target.RowCount);

            TableTestHelper.SetAndTestValueNotPresent(source, target, sourceRow2, updateHandler.LastRowUpdated, 102, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValueNotPresent(source, target, sourceRow2, updateHandler.LastRowUpdated, "Blah2", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValueNotPresent(source, target, sourceRow2, updateHandler.LastRowUpdated, 42m, TestTableColumns.DecimalColumn);

            Thread.Sleep(delay);
            Assert.AreEqual(2, target.RowCount);

            TableTestHelper.TestValue(target, updateHandler.RowsUpdated[0], 101, TestTableColumns.IdColumn);
            TableTestHelper.TestValue(target, updateHandler.RowsUpdated[0], "Blah", TestTableColumns.StringColumn);
            TableTestHelper.TestValue(target, updateHandler.RowsUpdated[0], 4324m, TestTableColumns.DecimalColumn);

            TableTestHelper.TestValue(target, updateHandler.RowsUpdated[1], 102, TestTableColumns.IdColumn);
            TableTestHelper.TestValue(target, updateHandler.RowsUpdated[1], "Blah2", TestTableColumns.StringColumn);
            TableTestHelper.TestValue(target, updateHandler.RowsUpdated[1], 42m, TestTableColumns.DecimalColumn);
        }

        [Test]
        public void TestUpdate()
        {
            ReactiveTable source = TableTestHelper.CreateReactiveTable();
            ReactiveTable target = TableTestHelper.CreateReactiveTable();
            BatchedTableSynchroniser synchroniser = new BatchedTableSynchroniser(source, target, new DefaultThreadMarshaller(), TimeSpan.FromMilliseconds(delay));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            target.Subscribe(new DelegateObserver<RowUpdate>(updateHandler.OnRowUpdate, null, null));

            var sourceRow1 = source.AddRow();
            Assert.AreEqual(1, target.RowCount);

            TableTestHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, 101, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, "Blah", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, 4324m, TestTableColumns.DecimalColumn);

            TableTestHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, 103, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, "Blah2", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, 43999m, TestTableColumns.DecimalColumn);

            var sourceRow2 = source.AddRow();
            Assert.AreEqual(2, target.RowCount);

            TableTestHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, 102, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, "Blah2", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, 42m, TestTableColumns.DecimalColumn);

            TableTestHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, 104, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, "Blah4", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, 421111m, TestTableColumns.DecimalColumn);
        }

        [Test]
        public void TestDelete()
        {
            ReactiveTable source = TableTestHelper.CreateReactiveTable();
            ReactiveTable target = TableTestHelper.CreateReactiveTable();
            BatchedTableSynchroniser synchroniser = new BatchedTableSynchroniser(source, target, new DefaultThreadMarshaller(), TimeSpan.FromMilliseconds(delay));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            target.Subscribe(new DelegateObserver<RowUpdate>(updateHandler.OnRowUpdate, null, null));

            var sourceRow1 = source.AddRow();
            Assert.AreEqual(1, target.RowCount);

            TableTestHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, 101, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, "Blah", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow1, updateHandler.LastRowUpdated, 4324m, TestTableColumns.DecimalColumn);

            var sourceRow2 = source.AddRow();
            Assert.AreEqual(2, target.RowCount);

            var targetRow2 = updateHandler.LastRowUpdated;
            TableTestHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, 102, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, "Blah2", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow2, updateHandler.LastRowUpdated, 42m, TestTableColumns.DecimalColumn);

            source.DeleteRow(sourceRow1);
            Assert.AreEqual(1, target.RowCount);
            Assert.AreEqual(102, target.GetValue<int>(TestTableColumns.IdColumn, targetRow2));

            source.DeleteRow(sourceRow2);
            Assert.AreEqual(0, target.RowCount);

            var sourceRow3 = source.AddRow();
            Assert.AreEqual(1, target.RowCount);

            TableTestHelper.SetAndTestValue(source, target, sourceRow3, updateHandler.LastRowUpdated, 101, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow3, updateHandler.LastRowUpdated, "Blah", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(source, target, sourceRow3, updateHandler.LastRowUpdated, 4324m, TestTableColumns.DecimalColumn);
        }
    }
}
