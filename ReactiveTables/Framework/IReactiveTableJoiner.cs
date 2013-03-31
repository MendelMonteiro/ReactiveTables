using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework
{
    public interface IReactiveTableJoiner
    {
        int RowCount { get; }
        int GetRowIndex(IReactiveColumn column, int rowIndex);
    }
}