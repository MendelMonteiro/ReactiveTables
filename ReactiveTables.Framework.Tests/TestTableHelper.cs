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
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Joins;

namespace ReactiveTables.Framework.Tests
{
    internal static class TestTableHelper
    {
        public static ReactiveTable CreateReactiveTable()
        {
            ReactiveTable table = new ReactiveTable();
            table.AddColumn(new ReactiveColumn<int>(TestTableColumns.IdColumn));
            table.AddColumn(new ReactiveColumn<string>(TestTableColumns.StringColumn));
            table.AddColumn(new ReactiveColumn<decimal>(TestTableColumns.DecimalColumn));
            return table;
        }

        public static ReactiveTable CreateReactiveTable2()
        {
            ReactiveTable table = new ReactiveTable();
            table.AddColumn(new ReactiveColumn<int>(TestTableColumns.IdColumn2));
            table.AddColumn(new ReactiveColumn<int>(TestTableColumns.OtherIdColumn2));
            table.AddColumn(new ReactiveColumn<string>(TestTableColumns.StringColumn2));
            table.AddColumn(new ReactiveColumn<decimal>(TestTableColumns.DecimalColumn2));
            return table;
        }

        public static ReactiveTable CreateReactiveTableFull()
        {
            ReactiveTable table = new ReactiveTable();
            table.AddColumn(new ReactiveColumn<int>(TestTableColumns.IdColumn));
            table.AddColumn(new ReactiveColumn<string>(TestTableColumns.StringColumn));
            table.AddColumn(new ReactiveColumn<decimal>(TestTableColumns.DecimalColumn));
            table.AddColumn(new ReactiveColumn<bool>(TestTableColumns.BoolColumn));
            table.AddColumn(new ReactiveColumn<double>(TestTableColumns.DoubleColumn));
            table.AddColumn(new ReactiveColumn<short>(TestTableColumns.ShortColumn));
            table.AddColumn(new ReactiveColumn<long>(TestTableColumns.LongColumn));
            table.AddColumn(new ReactiveColumn<DateTime>(TestTableColumns.DateTimeColumn));
            table.AddColumn(new ReactiveColumn<TimeSpan>(TestTableColumns.TimespanColumn));
            table.AddColumn(new ReactiveColumn<Guid>(TestTableColumns.GuidColumn));
            table.AddColumn(new ReactiveColumn<float>(TestTableColumns.FloatColumn));
            table.AddColumn(new ReactiveColumn<byte>(TestTableColumns.ByteColumn));
            table.AddColumn(new ReactiveColumn<char>(TestTableColumns.CharColumn));
            return table;
        }

        public static ReactiveTable CreateIndexedReactiveTable()
        {
            ReactiveTable table = new ReactiveTable();
            table.AddColumn(new ReactiveColumn<int>(TestTableColumns.IdColumn, new ColumnIndex<int>()));
            table.AddColumn(new ReactiveColumn<string>(TestTableColumns.StringColumn, new ColumnIndex<string>()));
            table.AddColumn(new ReactiveColumn<decimal>(TestTableColumns.DecimalColumn, new ColumnIndex<decimal>()));
            return table;
        }

        public static void SetAndTestValue<T>(IWritableReactiveTable table, int rowId, T value, string columnId)
        {
            table.SetValue(columnId, rowId, value);
            Assert.AreEqual(value, table.GetValue<T>(columnId, rowId));
        }

        public static void SetAndTestValue<T>(IWritableReactiveTable setTable, IReactiveTable getTable, int setRowId, int getRowId, T value, string columnId)
        {
            setTable.SetValue(columnId, setRowId, value);
            TestValue(getTable, getRowId, value, columnId);
        }

        public static void TestValue<T>(IReactiveTable table, int rowId, T value, string columnId)
        {
            Assert.AreEqual(value, table.GetValue<T>(columnId, rowId));
        }

        public static void SetAndTestValueNotPresent<T>(IWritableReactiveTable setTable,
                                                        IReactiveTable getTable,
                                                        int setRowId,
                                                        int getRowId,
                                                        T value,
                                                        string columnId)
        {
            setTable.SetValue(columnId, setRowId, value);
            Assert.AreEqual(default(T), getTable.GetValue<T>(columnId, getRowId));
        }

        public static IReactiveTable CreateJoinedReactiveTable(out IWritableReactiveTable table1, out IWritableReactiveTable table2)
        {
            table1 = CreateReactiveTable();
            table2 = CreateReactiveTable2();

            return table1.Join(table2, new Join<int>(table1, TestTableColumns.IdColumn, table2, TestTableColumns.OtherIdColumn2));
        }
    }
}