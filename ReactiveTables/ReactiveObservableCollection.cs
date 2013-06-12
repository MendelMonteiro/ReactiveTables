using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using ReactiveTables.Framework;

namespace ReactiveTables
{
    public class ReactiveObservableCollection : ICollection, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ReactiveTable _table;

        public ReactiveObservableCollection(ReactiveTable table)
        {
            _table = table;
            _table.Subscribe(OnNext);
        }

        private void OnNext(TableUpdate update)
        {
            var onCollectionChanged = CollectionChanged;
            var onPropertyChanged = PropertyChanged;
            if (update.Action == TableUpdate.TableUpdateAction.Add)
            {
                if (onCollectionChanged != null)
                {
                    object obj = _table.GetValue(update.Column.ColumnId, update.RowIndex);
                    onCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, obj, update.RowIndex));
                }
            }
            if (update.Action == TableUpdate.TableUpdateAction.Update)
            {
                if (onPropertyChanged != null)
                {
                    onPropertyChanged(this, new PropertyChangedEventArgs(update.Column.ColumnId));
//                    onCollectionChanged(this, new NotifyCollectionChangedEventArgs(Replace));
                }
            }
            if (update.Action == TableUpdate.TableUpdateAction.Delete)
            {
                if (onCollectionChanged != null)
                {
                    object obj = _table.GetValue(update.Column.ColumnId, update.RowIndex);
                    onCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, obj, update.RowIndex));
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new ReactiveTableEnumerator(_table);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count { get { return _table.RowCount; } }
        public object SyncRoot { get; private set; }
        public bool IsSynchronized { get; private set; }
    }

    public class ReactiveTableEnumerator : IEnumerator<int>
    {
        private readonly ReactiveTable _table;
        private readonly IEnumerator<int> _rows;

        public ReactiveTableEnumerator(ReactiveTable table)
        {
            _table = table;
            _rows = _table.GetRows().GetEnumerator();
        }

        public bool MoveNext()
        {
            return _rows.MoveNext();
        }

        public void Reset()
        {
            _rows.Reset();
        }

        public int Current { get; private set; }

        object IEnumerator.Current
        {
            get { return _rows.Current; }
        }

        public void Dispose()
        {
            _rows.Dispose();
        }
    }
}