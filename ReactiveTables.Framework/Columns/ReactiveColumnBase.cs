// This file is part of ReactiveTables.
// 
// ReactiveTables is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReactiveTables is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace ReactiveTables.Framework.Columns
{
    public abstract class ReactiveColumnBase<T> : IReactiveColumn<T>
    {
// ReSharper disable StaticFieldInGenericType
        private static readonly Type _type = typeof (T);
// ReSharper restore StaticFieldInGenericType

        protected readonly Subject<TableUpdate> UpdateSubject = new Subject<TableUpdate>(); 

        public string ColumnId { get; protected set; }

        public virtual Type Type
        {
            get { return _type; }
        }

        internal void NotifyObserversOnNext(int index)
        {
            UpdateSubject.OnNext(new TableUpdate(TableUpdateAction.Update, index, this));
        }

        public abstract void AddField(int rowIndex);

        public virtual IReactiveColumn Clone()
        {
            throw new NotImplementedException();
        }

        public abstract void CopyValue(int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex);

        public abstract void RemoveField(int rowIndex);

        object IReactiveColumn.GetValue(int rowIndex)
        {
            return GetValue(rowIndex);
        }

        public abstract void SetValue(int rowIndex, T value);

        public abstract T GetValue(int rowIndex);

        public abstract int Find(T value);

        #region IEquatable<IReactieColumn> implementation

        public bool Equals(IReactiveColumn other)
        {
            return string.Equals(ColumnId, other.ColumnId);
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            return UpdateSubject.Subscribe(observer);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ReactiveColumnBase<T>) obj);
        }

        public override int GetHashCode()
        {
            return (ColumnId != null ? ColumnId.GetHashCode() : 0);
        }

        #endregion
    }
}