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
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;

namespace ReactiveTables.Framework.Columns
{
    public abstract class ReactiveColumnBase<T>: IReactiveColumn<T>
    {
        public string ColumnId { get; protected set; }

        private readonly List<IColumnObserver> _observers = new List<IColumnObserver>();

        public IDisposable Subscribe(IColumnObserver observer)
        {
            var token = new ReactiveColumnToken(this, observer);
            _observers.Add(observer);
            return token;
        }

        protected virtual void Unsubscribe(object observer)
        {
            _observers.Remove((IColumnObserver)observer);
        }

        internal void NotifyObserversOnNext(int index)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(index);
            }
        }

        internal void NotifyObserversOnError(Exception error, int index)
        {
            _observers.ForEach(observer => observer.OnError(error, index));
        }

        internal void NotifyObserversOnCompleted(int index)
        {
            _observers.ForEach(observer => observer.OnCompleted(index));
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

        private class ReactiveColumnToken : IDisposable
        {
            private readonly ReactiveColumnBase<T> column;
            private readonly object _observer;

            public ReactiveColumnToken(ReactiveColumnBase<T> column, object observer)
            {
                this.column = column;
                _observer = observer;
            }

            public void Dispose()
            {
                column.Unsubscribe(_observer);
            }
        }

        public abstract int Find(T value);

        #region IEquatable<IReactieColumn> implementation
        public bool Equals(IReactiveColumn other)
        {
            return string.Equals(ColumnId, other.ColumnId);            
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ReactiveColumnBase<T>) obj);
        }

        public override int GetHashCode()
        {
            return (ColumnId != null ? ColumnId.GetHashCode() : 0);
        }
        #endregion
    }
}
