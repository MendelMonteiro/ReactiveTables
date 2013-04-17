/*This file is part of ReactiveTables.

ReactiveTables is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ReactiveTables is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.Collections.Generic;
using System.Diagnostics;

namespace ReactiveTables.Framework.Columns
{
    public interface IReactiveColumn : IObservableColumn
    {
        /// <summary>
        /// Should be an int?
        /// </summary>
        string ColumnId { get; }
        void AddField(int rowIndex);
        IReactiveColumn Clone();
        void CopyValue(int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex);
        void RemoveField(int rowIndex);
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

        public override void AddField(int rowIndex)
        {
            var reactiveField = new ReactiveField<T>();
            if (rowIndex < Fields.Count)
            {
                Fields[rowIndex] = reactiveField;
            }
            else
            {
                Fields.Add(reactiveField);
            }
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

        public override void RemoveField(int rowIndex)
        {
            Fields[rowIndex] = null;
        }

        private List<ReactiveField<T>> Fields { get; set; }

        public override IReactiveField<T> GetValue(int rowIndex)
        {
            if (rowIndex < 0 || Fields[rowIndex] == null) return ReactiveField<T>.Empty;
            return Fields[rowIndex];
        }

        public override void SetValue(int rowIndex, T value)
        {
//            Debug.Assert(rowIndex < Fields.Count);
            ReactiveField<T> field = Fields[rowIndex];
            field.SetInternalFieldValue(value);
            NotifyObserversOnNext(rowIndex);
        }
    }
}
