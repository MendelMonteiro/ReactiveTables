using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    internal class TestBase
    {
        protected readonly ReactiveTable Table;
        private const string ValueCol = "ValueCol";
        private const string TextCol = "TextCol";
        private const string IdCol = "IdCol";
        private const string DoubleCol = "DoubleCol";
        private const string BoolCol = "BoolCol";

        public TestBase(int? initialSize)
        {
            Table = new ReactiveTable();
            Table.AddColumn(new ReactiveColumn<int>(IdCol, initialSize: initialSize));
            Table.AddColumn(new ReactiveColumn<string>(TextCol, initialSize: initialSize));
            Table.AddColumn(new ReactiveColumn<decimal>(ValueCol, initialSize: initialSize));
            Table.AddColumn(new ReactiveColumn<double>(DoubleCol, initialSize: initialSize));
            Table.AddColumn(new ReactiveColumn<bool>(BoolCol, initialSize: initialSize));            
        }

        protected void DeleteEntry(int rowIndex)
        {
            Table.DeleteRow(rowIndex);
        }

        protected void UpdateEntry(int rowIndex)
        {
            Table.SetValue(ValueCol, rowIndex, 98598643543m);
            Table.SetValue(TextCol, rowIndex, "An updated string");
            Table.SetValue(ValueCol, rowIndex, 9857437.3543m);
            Table.SetValue(DoubleCol, rowIndex, 94357348.43);
            Table.SetValue(BoolCol, rowIndex, false);
        }

        protected int AddEntry(int id)
        {
            var rowIndex = Table.AddRow();
            Table.SetValue<int>(IdCol, rowIndex, id);
            Table.SetValue(TextCol, rowIndex, "Some longer string that should take up some more space");
            Table.SetValue(ValueCol, rowIndex, 23213214214.3423m);
            Table.SetValue(DoubleCol, rowIndex, 234.23435);
            Table.SetValue(BoolCol, rowIndex, true);
            return rowIndex;
        }
    }
}