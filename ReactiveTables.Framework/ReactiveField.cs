namespace ReactiveTables.Framework
{
    public interface IReactiveField<out T>
    {
        T Value { get; }
    }

    public class ReactiveField<T>: IReactiveField<T>
    {
        public static readonly ReactiveField<T> Empty = new ReactiveField<T>(default(T));

        public ReactiveField()
        {
        }

        private ReactiveField(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }

        internal void SetInternalFieldValue(T value)
        {
            Value = value;
        }
    }
}