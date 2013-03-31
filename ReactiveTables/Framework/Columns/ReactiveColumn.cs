using System.Collections.Generic;

namespace ReactiveTables.Framework.Columns
{
    public interface IReactiveColumn : IObservableColumn
    {
        /// <summary>
        /// Should be an int?
        /// </summary>
        string ColumnId { get; }
        void AddField();
        IReactiveColumn Clone();
        void CopyValue(int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex);
    }

    public interface IReactiveColumn<T> : IReactiveColumn
    {
        void SetValue(int rowIndex, T value);
        IReactiveField<T> GetValue(int index);
    }

    public interface IReactiveJoinableColumn
    {
        void SetJoiner(IReactiveTableJoiner joiner);
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

        public override IReactiveColumn Clone()
        {
            return new ReactiveColumn<T>(ColumnId);
        }

        public override void CopyValue(int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex)
        {
            // Assumes that the source column is of the same type.
            var sourceCol = (IReactiveColumn<T>)sourceColumn;
            SetValue(rowIndex, sourceCol.GetValue(sourceRowIndex).Value);
        }

        private List<ReactiveField<T>> Fields { get; set; }

        public override IReactiveField<T> GetValue(int rowIndex)
        {
            if (rowIndex < 0) return ReactiveField<T>.Empty;
            return Fields[rowIndex];
        }

        public override void SetValue(int rowIndex, T value)
        {
            ReactiveField<T> field = Fields[rowIndex];
            field.SetInternalFieldValue(value);
            NotifyObserversOnNext(rowIndex);
        }
    }
}
