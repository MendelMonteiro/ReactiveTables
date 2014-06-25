using System;
using ReactiveTables.Framework.Aggregate;
using ReactiveTables.Framework.Aggregate.Operations;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    class GroupedTableTest : ITest
    {
        private readonly int? _drainTableLimit;
        private readonly ReactiveTable _table1;
        private int _batchNumber;
        private const string SumColumnId = "Sum";
        private const string GroupColumnId = "GroupColumn1";
        private const string ValueColumnId = "ValueColumn1";
        private const string CountColumnId = "Count";
        private const string AverageColumnId = "Average";
        private const string MinColumnId = "Min";
        private const string MaxColumnId = "Max";
        private int _highWaterMark;

        public GroupedTableTest(int? drainTableLimit = null)
        {
            _drainTableLimit = drainTableLimit;
            _table1 = new ReactiveTable();
            var groupColumn = new ReactiveColumn<string>(GroupColumnId);
            _table1.AddColumn(groupColumn);
            var valueColumn = new ReactiveColumn<int>(ValueColumnId);
            _table1.AddColumn(valueColumn);

            var groupedTable = new AggregatedTable(_table1);
            groupedTable.GroupBy<string>(groupColumn.ColumnId);
            groupedTable.AddAggregate(groupColumn, CountColumnId, () => new Count<string>());
            groupedTable.AddAggregate(valueColumn, SumColumnId, () => new Sum<int>());
            groupedTable.AddAggregate(valueColumn, AverageColumnId, () => new Average<int>());
            groupedTable.AddAggregate(valueColumn, MinColumnId, () => new Min<int>());
            groupedTable.AddAggregate(valueColumn, MaxColumnId, () => new Max<int>());
        }

        public void Iterate()
        {
            _batchNumber++;

            if (_drainTableLimit == null || _batchNumber < _drainTableLimit)
            {
                var name1 = "Mendel" + _batchNumber;
                var name2 = "Marie" + _batchNumber;
                AddRow(name1, 42);
                AddRow(name2, 43);
                AddRow(name1, 44);
                AddRow(name2, 45);
                AddRow(name1, 46);
                AddRow(name2, 45);
            }
            else
            {
                RemoveRow();
            }
        }

        private void RemoveRow()
        {
            if (_table1.RowCount > 0)
            {
                _table1.DeleteRow(_table1.RowCount - 1);
            }
        }

        private void AddRow(string groupVal, int value)
        {
            var row1 = _table1.AddRow();
            _table1.SetValue(GroupColumnId, row1, groupVal);
            _table1.SetValue(ValueColumnId, row1, value);
            _highWaterMark = Math.Max(_table1.RowCount, _highWaterMark);
        }

        public long Metric { get { return _highWaterMark; } }
    }
}
