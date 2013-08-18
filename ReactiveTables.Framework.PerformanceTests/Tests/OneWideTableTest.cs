using System;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    internal class OneWideTableTest : ITest
    {
        private readonly ReactiveTable _table;

        public OneWideTableTest()
        {
            _table = new ReactiveTable();
            _table.AddColumn(new ReactiveColumn<int>("IdCol"));
            _table.AddColumn(new ReactiveColumn<string>("TextCol"));
            _table.AddColumn(new ReactiveColumn<decimal>("ValueCol1"));
            _table.AddColumn(new ReactiveColumn<double>("ValueCol2"));
            _table.AddColumn(new ReactiveColumn<float>("ValueCol3"));
            _table.AddColumn(new ReactiveColumn<bool>("ValueCol4"));
            _table.AddColumn(new ReactiveColumn<string>("ValueCol5"));
            _table.AddColumn(new ReactiveColumn<DateTime>("ValueCol6"));
        }

        public void Iterate()
        {
            var id = _table.AddRow();
            _table.SetValue("IdCol", id, 1);
            _table.SetValue("TextCol", id, "Some longer string that should take up some more space");
            _table.SetValue("ValueCol1", id, 23213214214.3423m);
            _table.SetValue("ValueCol2", id, 23213214214.3423d);
            _table.SetValue("ValueCol3", id, 23213214214.3423f);
            _table.SetValue("ValueCol4", id, true);
            _table.SetValue("ValueCol5", id, "Another long string");
            _table.SetValue("ValueCol6", id, DateTime.Now);
        }

        public long Metric { get { return _table.RowCount; } }
    }
}