using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Columns.Calculated;

namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    internal class CalculatedColumnTableTest : ITest
    {
        private readonly ReactiveTable _table;

        public CalculatedColumnTableTest()
        {
            _table = new ReactiveTable();
            var idCol = new ReactiveColumn<int>("IdCol");
            _table.AddColumn(idCol);
            var textCol = new ReactiveColumn<string>("TextCol");
            _table.AddColumn(textCol);
            var valueCol = new ReactiveColumn<decimal>("ValueCol");
            _table.AddColumn(valueCol);

            _table.AddColumn(new ReactiveCalculatedColumn2<decimal, int, decimal>(
                                 "CalcCol1", idCol, valueCol, (id, val) => id*val));

            _table.AddColumn(new ReactiveCalculatedColumn2<string, int, string>(
                                 "CalcCol2", idCol, textCol, (id, text) => text + id));
        }

        public void Iterate()
        {
            var id = _table.AddRow();
            _table.SetValue("IdCol", id, 1);
            _table.SetValue("TextCol", id, "Some longer string that should take up some more space");
            _table.SetValue("ValueCol", id, 23213214214.3423m);

            var calc1 = _table.GetValue<decimal>("CalcCol1", id);
            var calc2 = _table.GetValue<string>("CalcCol2", id);
        }

        public long Metric { get { return _table.RowCount; } }
    }
}