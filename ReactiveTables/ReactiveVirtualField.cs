namespace ReactiveTables
{
    public struct ReactiveVirtualField<T>:IReactiveField<T>
    {
        public T Value { get; set; }
    }
}