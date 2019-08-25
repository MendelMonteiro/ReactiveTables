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
using System.Collections.Generic;
using System.ComponentModel;
using NFluent;
using NUnit.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Framework.Tests
{
    [TestFixture]
    public class ReactiveTableTypedTests 
    {
        [Test]
        public void ShouldHaveColumns()
        {
            var table = new ReactiveTable<TestModel>();
            Check.That(table.Columns).HasSize(4);
            CheckColumn(table, 0, "TestModel.Id", typeof (int));
            CheckColumn(table, 1, "TestModel.Name", typeof (string));
            CheckColumn(table, 2, "TestModel.Value", typeof (decimal));
            CheckColumn(table, 3, "TestModel.Timestamp", typeof (DateTime));
        }

        [Test]
        public void ShouldReadValuesFromFlyWeight()
        {
            var table = new ReactiveTable<TestModelFlyweight>();
            
            Check.That(table.Columns).HasSize(4);
            CheckColumn(table, 0, "TestModelFlyweight.Id", typeof(int));
            CheckColumn(table, 1, "TestModelFlyweight.Name", typeof(string));
            CheckColumn(table, 2, "TestModelFlyweight.Value", typeof(decimal));
            CheckColumn(table, 3, "TestModelFlyweight.Timestamp", typeof(DateTime));

            var row = table.AddRow();
            table.SetValue("TestModelFlyweight.Id", row, 42);
            table.SetValue("TestModelFlyweight.Name", row, "Hello");
            table.SetValue("TestModelFlyweight.Value", row, 43m);
            var dateTime = DateTime.UtcNow;
            table.SetValue("TestModelFlyweight.Timestamp", row, dateTime);

            var flyweight = table.Flyweights[0];
            Check.That(flyweight.Id).IsEqualTo(42);
            Check.That(flyweight.Name).IsEqualTo("Hello");
            Check.That(flyweight.Value).IsEqualTo(43m);
            Check.That(flyweight.Timestamp).IsEqualTo(dateTime);
        }

        [Test]
        public void ShouldFlyweightShouldNotifyOfChanges()
        {
            var table = new ReactiveTable<TestModelFlyweight>();

            var row = table.AddRow();
            var flyweight = table.Flyweights[0];

            var changes = new List<string>();
            flyweight.PropertyChanged += (sender, args) => changes.Add(args.PropertyName);

            table.SetValue("TestModelFlyweight.Id", row, 42);
            table.SetValue("TestModelFlyweight.Name", row, "Hello");
            table.SetValue("TestModelFlyweight.Value", row, 43m);
            var dateTime = DateTime.UtcNow;
            table.SetValue("TestModelFlyweight.Timestamp", row, dateTime);

            Check.That(changes).ContainsExactly("Id", "Name", "Value", "Timestamp");
        }

        private void CheckColumn(IReactiveTable reactiveTable, int index, string columnId, Type type)
        {
            Check.That(reactiveTable.Columns[index].ColumnId).IsEqualTo(columnId);
            Check.That(reactiveTable.Columns[index].Type).IsEqualTo(type);
        }
    }

    public class TestModel : IBaseModelFlyweight
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public DateTime Timestamp { get; set; }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add { throw new NotImplementedException(); } remove { throw new NotImplementedException(); } }
        event PropertyChangedEventHandler IBaseModelFlyweight.PropertyChanged { add { throw new NotImplementedException(); } remove { throw new NotImplementedException(); } }

        public void SetRowIndex(int rowIndex)
        {
            throw new NotImplementedException();
        }

        public void SetTable(IReactiveTable table)
        {
            throw new NotImplementedException();
        }

        public void OnPropertyChanged(string propertyName)
        {
            throw new NotImplementedException();
        }
    }

    internal class TestModelFlyweight : BaseModelFlyweight<TestModelFlyweight>
    {
        public int Id => GetValue<int>();
        public string Name => GetValue<string>();
        public decimal Value => GetValue<decimal>();
        public DateTime Timestamp => GetValue<DateTime>();
    }
}