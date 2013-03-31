namespace ReactiveTables.Framework.Columns.Calculated
{
    public struct ReactiveVirtualField<T>:IReactiveField<T>
    {
        public T Value { get; set; }
    }
}