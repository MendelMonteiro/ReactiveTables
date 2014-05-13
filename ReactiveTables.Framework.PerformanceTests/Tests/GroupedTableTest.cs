using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveTables.Framework.Aggregate;
using ReactiveTables.Framework.Aggregate.Operations;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    class GroupedTableTest : ITest
    {
        private readonly ReactiveTable _table1;
        private readonly AggregatedTable _groupedTable;
        private int _batchNumber;
        private const string SumColumnId = "Sum";
        private const string GroupColumnId = "GroupColumn1";
        private const string ValueColumnId = "ValueColumn1";
        private const string CountColumnId = "Count";
        private const string AverageColumnId = "Average";
        private const string MinColumnId = "Min";
        private const string MaxColumnId = "Max";

        public GroupedTableTest()
        {
            _table1 = new ReactiveTable();
            var groupColumn = new ReactiveColumn<string>(GroupColumnId);
            _table1.AddColumn(groupColumn);
            var valueColumn = new ReactiveColumn<int>(ValueColumnId);
            _table1.AddColumn(valueColumn);

            _groupedTable = new AggregatedTable(_table1);
            _groupedTable.GroupBy<string>(groupColumn.ColumnId);
            _groupedTable.AddAggregate(groupColumn, CountColumnId, () => new Count<string>());
            _groupedTable.AddAggregate(valueColumn, SumColumnId, () => new Sum<int>());
            _groupedTable.AddAggregate(valueColumn, AverageColumnId, () => new Average<int>());
            _groupedTable.AddAggregate(valueColumn, MinColumnId, () => new Min<int>());
            _groupedTable.AddAggregate(valueColumn, MaxColumnId, () => new Max<int>());
        }

        public void Iterate()
        {
            _batchNumber++;
            var name1 = "Mendel" + _batchNumber;
            var name2 = "Marie" + _batchNumber;
            AddRow(name1, 42);
            AddRow(name2, 43);
            AddRow(name1, 44);
            AddRow(name2, 45);
            AddRow(name1, 46);
            AddRow(name2, 45);
        }

        private void AddRow(string groupVal, int value)
        {
            var row1 = _table1.AddRow();
            _table1.SetValue(GroupColumnId, row1, groupVal);
            _table1.SetValue(ValueColumnId, row1, value);
        }

        public long Metric { get { return _table1.RowCount; } }
    }
}
