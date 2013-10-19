using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ReactiveTables.Demo.Utils
{
    public class BatchingObservableCollection<T> : ObservableCollection<T>
    {
        private bool _inBatchUpdate;

        public IDisposable BatchUpdate()
        {
            if (_inBatchUpdate)
            {
                throw new InvalidOperationException("Batch update already in progress");
            }
            _inBatchUpdate = true;
            return new UpdateDisposable(this);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_inBatchUpdate)
            {
                base.OnCollectionChanged(e);
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!_inBatchUpdate)
            {
                base.OnPropertyChanged(e);
            }
        }

        private void EndBatch()
        {
            if (_inBatchUpdate)
            {
                _inBatchUpdate = false;
                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Reset));
            }
        }

        private class UpdateDisposable : IDisposable
        {
            private readonly BatchingObservableCollection<T> _parent;

            public UpdateDisposable(BatchingObservableCollection<T> parent)
            {
                _parent = parent;
            }

            public void Dispose()
            {
                _parent.EndBatch();
            }
        }
    }
}