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

using NUnit.Framework;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.PerformanceTests
{
    static internal class TableTestHelper
    {
        public static ReactiveTable CreateReactiveTable()
        {
            ReactiveTable table = new ReactiveTable();
            table.AddColumn(new ReactiveColumn<int>(TestTableColumns.IdColumn));
            table.AddColumn(new ReactiveColumn<string>(TestTableColumns.StringColumn));
            table.AddColumn(new ReactiveColumn<decimal>(TestTableColumns.DecimalColumn));
            return table;
        }

        public static void SetAndTestValue<T>(ReactiveTable table, int rowId, T value, string columnId)
        {
            table.SetValue(columnId, rowId, value);
            Assert.AreEqual(value, table.GetValue<T>(columnId, rowId));
        }

        public static void SetAndTestValue<T>(ReactiveTable setTable, ReactiveTable getTable, int setRowId, int getRowId, T value, string columnId)
        {
            setTable.SetValue(columnId, setRowId, value);
            TestValue(getTable, getRowId, value, columnId);
        }

        public static void TestValue<T>(ReactiveTable table, int rowId, T value, string columnId)
        {
            Assert.AreEqual(value, table.GetValue<T>(columnId, rowId));
        }

        public static void SetAndTestValueNotPresent<T>(ReactiveTable setTable, ReactiveTable getTable, int setRowId, int getRowId, T value, string columnId)
        {
            setTable.SetValue(columnId, setRowId, value);
            Assert.AreEqual(default(T), getTable.GetValue<T>(columnId, getRowId));
        }
    }
}
