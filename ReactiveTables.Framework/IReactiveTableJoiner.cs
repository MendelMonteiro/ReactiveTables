using System;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework
{
    public interface IReactiveTableJoiner
    {
        int RowCount { get; }
        int GetRowIndex(IReactiveColumn column, int joinRowIndex);
        void AddRowObserver(IObserver<RowUpdate> observer);
    }
}