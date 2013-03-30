namespace ReactiveTables.Framework
{
    public struct ReactiveVirtualField<T>:IReactiveField<T>
    {
        public T Value { get; set; }
    }
}