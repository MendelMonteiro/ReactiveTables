using System.Collections.Generic;
using NUnit.Framework;
using ReactiveTables.Framework.Columns;
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
            var filteredTable = (IReactiveTable)rawTable.Filter(new TestPredicate(new List<IReactiveColumn>
                                                                      {
                                                                          rawTable.Columns[TestTableColumns.IdColumn]
                                                                      },
                                                                  true
                                                    ));

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
            var filteredTable = rawTable.Filter(new TestPredicate(new List<IReactiveColumn>
                                                                      {
                                                                          rawTable.Columns[TestTableColumns.IdColumn]
                                                                      },
                                                                  false
                                                    ));

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
                new DelegatePredicate1<string>((ReactiveColumn<string>) rawTable.Columns[TestTableColumns.StringColumn],
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
                new DelegatePredicate2<string, decimal>((ReactiveColumn<string>) rawTable.Columns[TestTableColumns.StringColumn],
                                                        (ReactiveColumn<decimal>) rawTable.Columns[TestTableColumns.DecimalColumn],
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

            var filteredTable = rawTable.Filter(new TestPredicate(new List<IReactiveColumn>
                                                                      {
                                                                          rawTable.Columns[TestTableColumns.IdColumn]
                                                                      },
                                                                  false
                                                    ));

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            filteredTable.Subscribe(updateHandler);

            Assert.AreEqual(0, filteredTable.RowCount);
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            var filteredTable2 = rawTable.Filter(new TestPredicate(new List<IReactiveColumn>
                                                                      {
                                                                          rawTable.Columns[TestTableColumns.IdColumn]
                                                                      },
                                                                  true
                                                    ));

            RowUpdateHandler updateHandler2 = new RowUpdateHandler();
            filteredTable2.Subscribe(updateHandler2);

            Assert.AreEqual(2, filteredTable2.RowCount);
            // No updates will be fired as existing rows are processed in the constructor
            Assert.AreEqual(0, updateHandler2.CurrentRowCount);
        }

        [Test]
        public void TestFilterJoinedTable()
        {
        }

        private static int AddRow(ReactiveTable table, int id, string stringVal, decimal decimalVal)
        {
            var row1 = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, row1, id);
            table.SetValue(TestTableColumns.StringColumn, row1, stringVal);
            table.SetValue(TestTableColumns.DecimalColumn, row1, decimalVal);
            return row1;
        }
    }

    public class TestPredicate : IReactivePredicate
    {
        private readonly bool _rowIsVisible;

        public TestPredicate(List<IReactiveColumn> columns, bool rowIsVisible)
        {
            Columns = columns;
            _rowIsVisible = rowIsVisible;
        }

        public List<IReactiveColumn> Columns { get; private set; }
        public bool RowIsVisible(int rowIndex)
        {
            return _rowIsVisible;
        }
    }
}