﻿using System.Collections.Generic;
using System.Reactive.Subjects;
using NUnit.Framework;
using System;
using System.Linq;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Tests.Aggregate
{
    [TestFixture]
    public class AggregateTests
    {
        /// <summary>
        /// Test a grouped column by using a standard table which has one groupable column
        /// and one value column upon which we can perform aggregate functions.
        /// </summary>
        [Test]
        public void TestGroupByOneColumn()
        {
            // Create group by
            var baseTable = TestTableHelper.CreateReactiveTable();
            var groupedTable = new AggregatedTable(baseTable);
            groupedTable.GroupBy(TestTableColumns.StringColumn);
//            groupedTable.AddAggregatedColumn<int, decimal>("Count", TestTableColumns.StringColumn, );

            // Add values

            // Modify values

            // Modify grouped columns

            // Remove rows
        }

        [Test]
        public void TestGroupByMultipleColumns()
        {

        }

        [Test]
        public void TestGroupByOnExistingValues()
        {

        }

        [Test]
        public void TestGroupByWithCount()
        {

        }

        [Test]
        public void TestGroupByWithSum()
        {

        }

        [Test]
        public void TestGroupByWithMin()
        {

        }

        [Test]
        public void TestGroupByWithMax()
        {

        }
    }

    public class AggregatedTable
    {
        private readonly HashSet<string> _groupColumns = new HashSet<string>();
        private readonly Dictionary<string, IReactiveColumn> _aggregateColumns = new Dictionary<string, IReactiveColumn>();

        public AggregatedTable(IReactiveTable sourceTable)
        {
            var subject = new Subject<TableUpdate>();
            sourceTable.Subscribe(OnNext);
        }

        public void GroupBy(string columnId)
        {
            _groupColumns.Add(columnId);
        }

        public void AddAggregatedColumn<TOutput, TInput>(string columnId, string sourceColumnId,
                                                         Func<TOutput, IEnumerable<TInput>> evaluator)
        {
            var col = new ReactiveColumn<TOutput>(columnId);
            _aggregateColumns.Add(sourceColumnId, col);
        }

        private void OnNext(TableUpdate tableUpdate)
        {
            if (_groupColumns.Contains(tableUpdate.Column.ColumnId))
            {
                var upatedColumn = tableUpdate.Column;
                if (tableUpdate.IsColumnUpdate())
                {
                    // Updated values - reevaluate grouping
                }
                else if (tableUpdate.Action == TableUpdateAction.Delete)
                {
                    // Deleted row - reevaluate grouping, subtracting value from current group
                }
            }

            if (_aggregateColumns.ContainsKey(tableUpdate.Column.ColumnId))
            {
                var updateColumn = tableUpdate.Column;
                if (tableUpdate.IsColumnUpdate())
                {
                    // Updated values - reevaluate grouping
                }
                else if (tableUpdate.Action == TableUpdateAction.Delete)
                {
                    // Deleted row - reevaluate grouping, subtracting value from current group
                }
            }
        }
    }
}