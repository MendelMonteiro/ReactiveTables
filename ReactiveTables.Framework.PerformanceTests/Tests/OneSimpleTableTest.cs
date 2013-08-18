using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    internal class OneSimpleTableTest : ITest
    {
        private readonly ReactiveTable _table;

        public OneSimpleTableTest(int? initialSize)
        {
            _table = new ReactiveTable();
            _table.AddColumn(new ReactiveColumn<int>("IdCol", initialSize: initialSize));
            _table.AddColumn(new ReactiveColumn<string>("TextCol", initialSize: initialSize));
            _table.AddColumn(new ReactiveColumn<decimal>("ValueCol", initialSize: initialSize));
        }

        public void Iterate()
        {
            var id = _table.AddRow();
            _table.SetValue("IdCol", id, 1);
            _table.SetValue("TextCol", id, "Some longer string that should take up some more space");
            _table.SetValue("ValueCol", id, 23213214214.3423m);
        }

        public long Metric { get { return _table.RowCount; } }
    }
}