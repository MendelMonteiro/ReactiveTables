using System;
using System.Collections.Generic;

namespace ReactiveTables
{
    public interface IReactiveColumn
    {
        /// <summary>
        /// Should be an int?
        /// </summary>
        string ColumnId { get; }
        void AddField();
    }

    public interface IReactiveColumn<T> : IReactiveColumn, IObservableColumn<T>
    {
        void SetValue(int index, T value);
        IReactiveField<T> GetValue(int index);
    }

    public class ReactiveColumn<T> : ReactiveColumnBase<T>
    {
        public ReactiveColumn(string columnId)
        {
            ColumnId = columnId;
            Fields = new List<ReactiveField<T>>();
        }

        public override void AddField()
        {
            Fields.Add(new ReactiveField<T>());
        }

        private List<ReactiveField<T>> Fields { get; set; }

        public override IReactiveField<T> GetValue(int index)
        {
            return Fields[index];
        }

        public override void SetValue(int index, T value)
        {
            ReactiveField<T> field = Fields[index];
            field.SetInternalFieldValue(value);
            base.NotifyObserversOnNext(field.Value, index);
        }
    }
}
