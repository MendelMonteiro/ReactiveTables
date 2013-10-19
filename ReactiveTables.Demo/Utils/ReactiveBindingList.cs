using System;
using System.Collections;
using System.ComponentModel;
using ReactiveTables.Framework;

namespace ReactiveTables.Demo.Utils
{
    public class ReactiveBindingList : IBindingList
    {
        private readonly IReactiveTable _table;

        public ReactiveBindingList(IReactiveTable table)
        {
            _table = table;
            _table.Subscribe(OnNext);
        }

        private void OnNext(TableUpdate update)
        {
            var onListChanged = ListChanged;
            if (onListChanged != null)
            {
                var listChangedEventArgs = GetListChangedEventArgs(update);
                onListChanged(this, listChangedEventArgs);
            }
        }

        private static ListChangedEventArgs GetListChangedEventArgs(TableUpdate update)
        {
            if (update.Action == TableUpdate.TableUpdateAction.Add)
            {
                return new ListChangedEventArgs(ListChangedType.ItemAdded, update.RowIndex);
            }
            if (update.Action == TableUpdate.TableUpdateAction.Delete)
            {
                return new ListChangedEventArgs(ListChangedType.ItemDeleted, update.RowIndex);
            }
            if (update.Action == TableUpdate.TableUpdateAction.Update)
            {
                return new ListChangedEventArgs(ListChangedType.ItemChanged, update.RowIndex);
            }

            throw new NotImplementedException("Unknown update action " + update.Action);
        }

        class MyPropertyDescriptor : PropertyDescriptor
        {
            public MyPropertyDescriptor(string name, Attribute[] attrs) : base(name, attrs)
            {
            }

            public MyPropertyDescriptor(MemberDescriptor descr) : base(descr)
            {
            }

            public MyPropertyDescriptor(MemberDescriptor descr, Attribute[] attrs) : base(descr, attrs)
            {
            }

            public override bool CanResetValue(object component)
            {
                throw new NotImplementedException();
            }

            public override object GetValue(object component)
            {
                throw new NotImplementedException();
            }

            public override void ResetValue(object component)
            {
                throw new NotImplementedException();
            }

            public override void SetValue(object component, object value)
            {
                throw new NotImplementedException();
            }

            public override bool ShouldSerializeValue(object component)
            {
                throw new NotImplementedException();
            }

            public override Type ComponentType
            {
                get { throw new NotImplementedException(); }
            }

            public override bool IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }

            public override Type PropertyType
            {
                get { throw new NotImplementedException(); }
            }
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _table.RowCount; }
        }

        public object SyncRoot { get; private set; }
        public bool IsSynchronized { get; private set; }
        
        public int Add(object value)
        {
            throw new NotImplementedException();            
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public object this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool IsReadOnly { get; private set; }
        public bool IsFixedSize { get; private set; }
        public object AddNew()
        {
            throw new NotImplementedException();
        }

        public void AddIndex(PropertyDescriptor property)
        {
            throw new NotImplementedException();
        }

        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw new NotImplementedException();
        }

        public int Find(PropertyDescriptor property, object key)
        {
            throw new NotImplementedException();
        }

        public void RemoveIndex(PropertyDescriptor property)
        {
            throw new NotImplementedException();
        }

        public void RemoveSort()
        {
            throw new NotImplementedException();
        }

        public bool AllowNew { get { return false; } }
        public bool AllowEdit { get { return false; } }
        public bool AllowRemove { get { return false; } }
        public bool SupportsChangeNotification { get { return true; } }
        public bool SupportsSearching { get { return false; } }
        public bool SupportsSorting { get { return false; } }
        public bool IsSorted { get { return false; } }
        public PropertyDescriptor SortProperty { get; private set; }
        public ListSortDirection SortDirection { get; private set; }
        public event ListChangedEventHandler ListChanged;
    }
}