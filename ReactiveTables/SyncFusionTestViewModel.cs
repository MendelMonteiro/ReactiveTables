using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Windows;
using ReactiveTables.Framework;
using Syncfusion.Windows.Controls.Cells;
using Syncfusion.Windows.Controls.Grid;
using System.Linq;

namespace ReactiveTables.Demo
{
    public class SyncfusionTestViewModel : IObservable<TableUpdate>
    {
        private readonly IReactiveTable _table;
        private readonly List<IObserver<TableUpdate>> _observers = new List<IObserver<TableUpdate>>();
        private IDisposable _token;
        readonly Subject<TableUpdate> _subject = new Subject<TableUpdate>();

        public SyncfusionTestViewModel()
        {
            _table = App.AccountHumans;
            _token = _table.Subscribe(OnNext);
        }

        public SyncfusionTestViewModel(IReactiveTable table)
        {
            _table = table;
        }

        public object GetValue(int rowIndex, int columnIndex)
        {
            var row = _table.GetRowAt(rowIndex);
            if (row >= 0 && columnIndex >= 0)
            {
                var reactiveColumn = _table.Columns.Values.ElementAt(columnIndex);
                return _table.GetValue(reactiveColumn.ColumnId, row);
            }

            return null;
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            return _subject.Subscribe(observer);
        }

        private void OnNext(TableUpdate tableUpdate)
        {
            _subject.OnNext(tableUpdate);
        }

        public int GetRowPosition(int rowIndex)
        {
            return _table.GetPositionOfRow(rowIndex);
        }

        public int GetColPosition(string columnId)
        {
            // TODO: Nasty - should keep a list of columns that are actually used by the grid and a map of their indeces
            return _table.Columns.Keys.IndexOf(columnId);
        }
    }

    public static class CollectionExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> collection, T item)
        {
            var i = 0;
            foreach (var foo in collection)
            {
                if (foo.Equals(item))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }

    public class SyncfusionTestGridControl : GridControlBase
    {
        private IDisposable CurrentToken { get; set; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (SyncfusionTestViewModel), typeof (SyncfusionTestGridControl),
            new PropertyMetadata(default(SyncfusionTestViewModel), ViewModelChanged));

        private static void ViewModelChanged(DependencyObject dependencyObject, 
            DependencyPropertyChangedEventArgs args)
        {
            SyncfusionTestGridControl grid = (SyncfusionTestGridControl)dependencyObject;
            if (grid.CurrentToken != null) grid.CurrentToken.Dispose();
            var viewModel = (SyncfusionTestViewModel)args.NewValue;
            grid.CurrentToken = viewModel.Subscribe(grid.OnNext);
        }

        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register("ColumnCount", typeof (int), typeof (SyncfusionTestGridControl),
            new PropertyMetadata(default(int), ColumnCountChanged));

        private static void ColumnCountChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            SyncfusionTestGridControl grid = (SyncfusionTestGridControl) dependencyObject;
            grid.Model.ColumnCount = (int) args.NewValue + grid.Model.HeaderColumns;
        }

        public int ColumnCount
        {
            get { return (int) GetValue(ColumnCountProperty); }
            set { SetValue(ColumnCountProperty, value); }
        }

        private void OnNext(TableUpdate tableUpdate)
        {
            if (tableUpdate.IsRowUpdate())
            {
                Model.RowCount += tableUpdate.Action == TableUpdate.TableUpdateAction.Add ? 1 : -1;
            }
            else
            {
                var rowIndex = ViewModel.GetRowPosition(tableUpdate.RowIndex);
                var colIndex = ViewModel.GetColPosition(tableUpdate.Column.ColumnId);
                // TODO: handle updates to multiple columns
                Model.InvalidateCell(new RowColumnIndex(rowIndex, colIndex));
            }
        }

        public SyncfusionTestViewModel ViewModel
        {
            get { return (SyncfusionTestViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            var rowIndex = e.Cell.RowIndex - Model.HeaderRows;
            var columnIndex = e.Cell.ColumnIndex - Model.HeaderColumns;
            e.Style.CellValue = ViewModel.GetValue(rowIndex, columnIndex);
        }
    }
}