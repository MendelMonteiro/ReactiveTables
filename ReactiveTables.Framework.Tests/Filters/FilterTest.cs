using System.Collections.Generic;
using NUnit.Framework;
using ReactiveTables.Framework.Filters;

namespace ReactiveTables.Framework.Tests.Filters
{
    [TestFixture]
    public class FilterTest
    {
        [Test]
        public void TestNullFilter()
        {
            var rawTable = TestTableHelper.CreateReactiveTable();
            var filteredTable = rawTable.Filter(new TestPredicate(new List<string> {TestTableColumns.IdColumn}, true));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            filteredTable.Subscribe(updateHandler);

            AddRow(rawTable, 1, "Blah1", 123.123m);
            Assert.AreEqual(1, rawTable.RowCount);
            Assert.AreEqual(1, filteredTable.RowCount);
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah1", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));

            AddRow(rawTable, 2, "Blah2", 123.123m);
            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(2, filteredTable.RowCount);
            Assert.AreEqual(2, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah2", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));
        }

        [Test]
        public void TestExclusionFilter()
        {
            var rawTable = TestTableHelper.CreateReactiveTable();
            var filteredTable = rawTable.Filter(new TestPredicate(new List<string> {TestTableColumns.IdColumn}, false));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            filteredTable.Subscribe(updateHandler);

            AddRow(rawTable, 1, "Blah1", 123.123m);
            Assert.AreEqual(1, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            AddRow(rawTable, 2, "Blah2", 123.123m);
            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);
        }

        [Test]
        public void TestOneColumnFilter()
        {
            var rawTable = TestTableHelper.CreateReactiveTable();
            var filteredTable = rawTable.Filter(
                new DelegatePredicate1<string>(TestTableColumns.StringColumn,
                                               s => !string.IsNullOrEmpty(s) && s.EndsWith("2")));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            filteredTable.Subscribe(updateHandler);

            AddRow(rawTable, 1, "Blah1", 123.123m);
            Assert.AreEqual(1, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            AddRow(rawTable, 2, "Blah2", 123.123m);
            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(1, filteredTable.RowCount);
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah2", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));
        }

        [Test]
        public void TestTwoColumnFilter()
        {

            var rawTable = TestTableHelper.CreateReactiveTable();
            var filteredTable = rawTable.Filter(
                new DelegatePredicate2<string, decimal>(TestTableColumns.StringColumn,
                                                        TestTableColumns.DecimalColumn,
                                                        (s, d) => !string.IsNullOrEmpty(s) && s.StartsWith("Blah") && d > 100m));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            filteredTable.Subscribe(updateHandler);

            AddRow(rawTable, 1, "Blah1", 23.123m);
            Assert.AreEqual(1, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            AddRow(rawTable, 2, "Blah2", 123.123m);
            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(1, filteredTable.RowCount);
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah2", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));
        }

        [Test]
        public void TestFilterFilledTable()
        {
            var rawTable = TestTableHelper.CreateReactiveTable();
            AddRow(rawTable, 1, "Blah1", 123.123m);
            Assert.AreEqual(1, rawTable.RowCount);

            AddRow(rawTable, 2, "Blah2", 123.123m);
            Assert.AreEqual(2, rawTable.RowCount);

            var filteredTable = rawTable.Filter(new TestPredicate(new List<string> {TestTableColumns.IdColumn}, false));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            filteredTable.Subscribe(updateHandler);

            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            var filteredTable2 = rawTable.Filter(new TestPredicate(new List<string> {TestTableColumns.IdColumn}, true));

            RowUpdateHandler updateHandler2 = new RowUpdateHandler();
            filteredTable2.Subscribe(updateHandler2);

            Assert.AreEqual(2, filteredTable2.RowCount);
            // No updates will be fired as existing rows are processed in the constructor
            Assert.AreEqual(0, updateHandler2.CurrentRowCount);
        }

        [Test]
        public void TestFilterJoinedTable()
        {
            IWritableReactiveTable table1, table2;
            var rawTable = TestTableHelper.CreateJoinedReactiveTable(out table1, out table2);
            bool[] visible = { true };
            var filteredTable = (FilteredTable)rawTable.Filter(
                new DelegatePredicate1<string>(TestTableColumns.StringColumn, s => visible[0]));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            filteredTable.Subscribe(updateHandler);

            AddRow(table1, 1, "Blah1", 123.123m);
            Assert.AreEqual(1, rawTable.RowCount);
            Assert.AreEqual(1, filteredTable.RowCount);
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah1", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));

            AddRow2(table2, 11, 1, "Other1", 321.21m);
            Assert.AreEqual(1, rawTable.RowCount);
            Assert.AreEqual(1, filteredTable.RowCount);
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            Assert.AreEqual("Other1", filteredTable.GetValue<string>(TestTableColumns.StringColumn2, updateHandler.LastRowUpdated));
            
            AddRow(table1, 2, "Blah2", 123.123m);
            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(2, filteredTable.RowCount);
            Assert.AreEqual(2, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah2", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));

            AddRow2(table2, 12, 2, "Other2", 321.21m);
            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(2, filteredTable.RowCount);
            Assert.AreEqual(2, updateHandler.CurrentRowCount);
            Assert.AreEqual("Other2", filteredTable.GetValue<string>(TestTableColumns.StringColumn2, updateHandler.LastRowUpdated));

            visible[0] = false;
            filteredTable.PredicateChanged();

            AddRow2(table2, 13, 3, "Other3", 321.21m);
            Assert.AreEqual(3, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            Assert.AreEqual(3, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            AddRow(table1, 3, "Blah3", 123.123m);
            Assert.AreEqual(3, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            visible[0] = true;
            filteredTable.PredicateChanged();

            Assert.AreEqual(3, rawTable.RowCount);
            Assert.AreEqual(3, filteredTable.RowCount);
            Assert.AreEqual(3, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah3", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));
            Assert.AreEqual("Other3", filteredTable.GetValue<string>(TestTableColumns.StringColumn2, updateHandler.LastRowUpdated));

            AddRow(table1, 4, "Blah4", 123.123m);
            Assert.AreEqual(4, rawTable.RowCount);
            Assert.AreEqual(4, filteredTable.RowCount);
            Assert.AreEqual(4, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah4", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));

            visible[0] = false;
            filteredTable.PredicateChanged();

            AddRow2(table2, 14, 4, "Other4", 321.21m);
            Assert.AreEqual(4, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            AddRow2(table2, 15, 5, "Other5", 321.21m);
            Assert.AreEqual(5, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);
            
            AddRow(table1, 5, "Blah5", 123.123m);
            Assert.AreEqual(5, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            visible[0] = true;
            filteredTable.PredicateChanged();

            Assert.AreEqual(5, rawTable.RowCount);
            Assert.AreEqual(5, filteredTable.RowCount);
            Assert.AreEqual(5, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah5", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));
            Assert.AreEqual("Other5", filteredTable.GetValue<string>(TestTableColumns.StringColumn2, updateHandler.LastRowUpdated));

            AddRow(table1, 6, "Blah6", 123.123m);
            Assert.AreEqual(6, rawTable.RowCount);
            Assert.AreEqual(6, filteredTable.RowCount);
            Assert.AreEqual(6, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah6", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));
        }

        [Test]
        public void TestChangingFilterEmptyTable()
        {
            var rawTable = TestTableHelper.CreateReactiveTable();
            bool[] visible = {true};
            var filteredTable = (FilteredTable) rawTable.Filter(
                new DelegatePredicate1<string>(TestTableColumns.StringColumn, s => visible[0]));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            filteredTable.Subscribe(updateHandler);

            Assert.AreEqual(0, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            visible[0] = false;
            filteredTable.PredicateChanged();

            Assert.AreEqual(0, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            AddRow(rawTable, 1, "Blah1", 123.123m);
            Assert.AreEqual(1, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            visible[0] = true;
            filteredTable.PredicateChanged();

            Assert.AreEqual(1, rawTable.RowCount);
            Assert.AreEqual(1, filteredTable.RowCount);
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah1", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));

            AddRow(rawTable, 2, "Blah2", 123.123m);
            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(2, filteredTable.RowCount);
            Assert.AreEqual(2, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah2", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));
        }

        [Test]
        public void TestChangingFilterFilledTable()
        {
            var rawTable = TestTableHelper.CreateReactiveTable();
            bool[] visible = { true };
            var filteredTable = (FilteredTable)rawTable.Filter(
                new DelegatePredicate1<string>(TestTableColumns.StringColumn, s => visible[0]));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            filteredTable.Subscribe(updateHandler);

            AddRow(rawTable, 1, "Blah1", 123.123m);
            Assert.AreEqual(1, rawTable.RowCount);
            Assert.AreEqual(1, filteredTable.RowCount);
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah1", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));

            AddRow(rawTable, 2, "Blah2", 123.123m);
            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(2, filteredTable.RowCount);
            Assert.AreEqual(2, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah2", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));

            visible[0] = false;
            filteredTable.PredicateChanged();

            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            visible[0] = true;
            filteredTable.PredicateChanged();

            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(2, filteredTable.RowCount);
            Assert.AreEqual(2, updateHandler.CurrentRowCount);

            visible[0] = false;
            filteredTable.PredicateChanged();

            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);
        }

        [Test]
        public void TestChangingFilterAndAddingRows()
        {
            var rawTable = TestTableHelper.CreateReactiveTable();
            bool[] visible = { true };
            var filteredTable = (FilteredTable)rawTable.Filter(
                new DelegatePredicate1<string>(TestTableColumns.StringColumn, s => visible[0]));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            filteredTable.Subscribe(updateHandler);

            AddRow(rawTable, 1, "Blah1", 123.123m);
            Assert.AreEqual(1, rawTable.RowCount);
            Assert.AreEqual(1, filteredTable.RowCount);
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah1", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));

            AddRow(rawTable, 2, "Blah2", 123.123m);
            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(2, filteredTable.RowCount);
            Assert.AreEqual(2, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah2", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));

            visible[0] = false;
            filteredTable.PredicateChanged();

            Assert.AreEqual(2, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            AddRow(rawTable, 3, "Blah3", 123.123m);
            Assert.AreEqual(3, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            visible[0] = true;
            filteredTable.PredicateChanged();

            Assert.AreEqual(3, rawTable.RowCount);
            Assert.AreEqual(3, filteredTable.RowCount);
            Assert.AreEqual(3, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah3", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));

            AddRow(rawTable, 4, "Blah4", 123.123m);
            Assert.AreEqual(4, rawTable.RowCount);
            Assert.AreEqual(4, filteredTable.RowCount);
            Assert.AreEqual(4, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah4", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));

            visible[0] = false;
            filteredTable.PredicateChanged();

            Assert.AreEqual(4, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            AddRow(rawTable, 5, "Blah5", 123.123m);
            Assert.AreEqual(5, rawTable.RowCount);
            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            visible[0] = true;
            filteredTable.PredicateChanged();

            Assert.AreEqual(5, rawTable.RowCount);
            Assert.AreEqual(5, filteredTable.RowCount);
            Assert.AreEqual(5, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah5", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));

            AddRow(rawTable, 6, "Blah6", 123.123m);
            Assert.AreEqual(6, rawTable.RowCount);
            Assert.AreEqual(6, filteredTable.RowCount);
            Assert.AreEqual(6, updateHandler.CurrentRowCount);
            Assert.AreEqual("Blah6", filteredTable.GetValue<string>(TestTableColumns.StringColumn, updateHandler.LastRowUpdated));
        }

        [Test]
        public void TestExistingRowsBug()
        {
            var rawTable = TestTableHelper.CreateReactiveTable();

            AddRow(rawTable, 1, "Foo1", 1);
            AddRow(rawTable, 2, "Foo2", 2);
            AddRow(rawTable, 3, "Foo3", 3);
            
            decimal[] filter = { 0 };
            var filteredTable = (FilteredTable) rawTable.Filter(new DelegatePredicate1<decimal>(
                                                                    TestTableColumns.DecimalColumn, s => s > filter[0]));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            filteredTable.Subscribe(updateHandler);

            AddRow(rawTable, 4, "Foo4", 4);
            AddRow(rawTable, 5, "Foo5", 5);
            Assert.AreEqual(5, filteredTable.RowCount);

            filter[0] = 50;
            filteredTable.PredicateChanged();
            Assert.AreEqual(0, filteredTable.RowCount);

            AddRow(rawTable, 6, "Foo6", 6);

            filter[0] = 3;
            filteredTable.PredicateChanged();
            Assert.AreEqual(3, filteredTable.RowCount);

            AddRow(rawTable, 7, "Foo7", 7);
            Assert.AreEqual(4, filteredTable.RowCount);

            filter[0] = 50;
            filteredTable.PredicateChanged();
            Assert.AreEqual(0, filteredTable.RowCount);

            filter[0] = 2;
            filteredTable.PredicateChanged();
            Assert.AreEqual(5, filteredTable.RowCount);
        }

        private static int AddRow(IWritableReactiveTable table, int id, string stringVal, decimal decimalVal)
        {
            var row1 = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, row1, id);
            table.SetValue(TestTableColumns.StringColumn, row1, stringVal);
            table.SetValue(TestTableColumns.DecimalColumn, row1, decimalVal);
            return row1;
        }

        private static int AddRow2(IWritableReactiveTable table, int id, int otherId, string stringVal, decimal decimalVal)
        {
            var row1 = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn2, row1, id);
            table.SetValue(TestTableColumns.OtherIdColumn2, row1, otherId);
            table.SetValue(TestTableColumns.StringColumn2, row1, stringVal);
            table.SetValue(TestTableColumns.DecimalColumn2, row1, decimalVal);
            return row1;
        }
    }

    public class TestPredicate : IReactivePredicate
    {
        private readonly bool _rowIsVisible;

        public TestPredicate(IList<string> columns, bool rowIsVisible)
        {
            Columns = columns;
            _rowIsVisible = rowIsVisible;
        }

        public IList<string> Columns { get; private set; }

        public bool RowIsVisible(IReactiveTable sourceTable, int rowIndex)
        {
            return _rowIsVisible;
        }
    }
}