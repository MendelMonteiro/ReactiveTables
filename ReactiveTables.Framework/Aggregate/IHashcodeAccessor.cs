namespace ReactiveTables.Framework.Aggregate
{
    internal interface IHashcodeAccessor
    {
        int GetColumnHashCode(int rowId);
        object GetValue(int rowId);
        string ColumnId { get; }
        void NotifyObserversOnNext(int index);
    }
}