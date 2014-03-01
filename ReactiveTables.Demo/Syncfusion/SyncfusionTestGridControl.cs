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
using System.Windows;
using ReactiveTables.Framework;
using Syncfusion.Windows.Controls.Cells;
using Syncfusion.Windows.Controls.Grid;

namespace ReactiveTables.Demo.Syncfusion
{
    public class SyncfusionTestGridControl : GridControlBase
    {
        readonly Dictionary<string, IViewModelAccessor> _viewModelAccessors = new Dictionary<string, IViewModelAccessor>(); 

        private IDisposable CurrentToken { get; set; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ISyncfusionViewModel), typeof(SyncfusionTestGridControl),
                                        new PropertyMetadata(default(ISyncfusionViewModel), ViewModelChanged));

        public SyncfusionTestGridControl()
        {
            Model.HeaderColumns = 0;
        }

        private static void ViewModelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var grid = (SyncfusionTestGridControl) dependencyObject;
            if (grid.CurrentToken != null) grid.CurrentToken.Dispose();
            var viewModel = (ISyncfusionViewModel) args.NewValue;
            if (viewModel.RowPositionsUpdated != null)
            {
                grid.RowPositionToken = viewModel.RowPositionsUpdated.Subscribe(grid.RowPositionsUpdated);
            }
            grid.CurrentToken = viewModel.Subscribe(grid.OnNext);
            grid.ColumnNames = viewModel.ColumnNames;
        }

        protected IList<string> ColumnNames { get; private set; }

        private IDisposable RowPositionToken { get; set; }

        private void RowPositionsUpdated(bool b)
        {
            // Invalidate the whole grid when it's being re-sorted
            Model.InvalidateCell(_tableRangeInfo);
        }

        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register("ColumnCount", typeof (int), typeof (SyncfusionTestGridControl),
                                        new PropertyMetadata(default(int), ColumnCountChanged));

        private readonly GridRangeInfo _tableRangeInfo = GridRangeInfo.Table();

        private static void ColumnCountChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var grid = (SyncfusionTestGridControl) dependencyObject;
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
                // Get the col position first as it's much cheaper than row position.
                var colIndex = ViewModel.GetColPosition(tableUpdate.Column.ColumnId) + Model.HeaderColumns;
                // TODO: handle updates to multiple columns (i.e. CellSpanInfo)
                if (IsColumnVisible(colIndex))
                {
                    var rowIndex = ViewModel.GetRowPosition(tableUpdate.RowIndex) + Model.HeaderRows;
                    if (IsRowVisible(rowIndex))
                    {
                        Model.InvalidateCell(new RowColumnIndex(rowIndex, colIndex));
                    }
                }
            }
        }

        public ISyncfusionViewModel ViewModel
        {
            get { return (ISyncfusionViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            var columnIndex = e.Cell.ColumnIndex - Model.HeaderColumns;
            if (e.Cell.RowIndex < Model.HeaderRows)
            {
                if (ColumnNames != null) e.Style.CellValue = ColumnNames[columnIndex];
                return;
            }

            var rowIndex = e.Cell.RowIndex - Model.HeaderRows;
            if (rowIndex >= 0 && columnIndex >= 0)
            {
                var columnId = ViewModel.GetColumnId(columnIndex);
                IViewModelAccessor accessor;
                if (_viewModelAccessors.TryGetValue(columnId, out accessor))
                {
                    accessor.SetCellValue(e.Style, rowIndex, columnIndex);

                    // TODO: move cell rendering to custom cell renderers
                    if (accessor.Type == typeof(bool))
                    {
                        e.Style.CellType = "CheckBox";
                        e.Style.Description = "Enabled";
                    } 
                }
            }
        }

        protected override void OnCommitCellInfo(GridCommitCellInfoEventArgs e)
        {
            var columnIndex = e.Cell.ColumnIndex - Model.HeaderColumns;
            var rowIndex = e.Cell.RowIndex - Model.HeaderRows;
            if (columnIndex >= 0 && rowIndex >= 0)
            {
                var columnId = ViewModel.GetColumnId(columnIndex);
                IViewModelAccessor accessor;
                if (_viewModelAccessors.TryGetValue(columnId, out accessor))
                {
                    accessor.SetModelValue(e.Style, rowIndex, columnIndex);
                }
            }
            base.OnCommitCellInfo(e);
        }

        protected override void OnSaveCellText(GridCellTextEventArgs e)
        {
            base.OnSaveCellText(e);
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (CurrentToken != null) CurrentToken.Dispose();
                if (RowPositionToken != null) RowPositionToken.Dispose();
            }
            base.Dispose(disposing);
        }

        public void AddColumn<T>(string columnId)
        {
            // TODO: create the accessors dynamically
            _viewModelAccessors.Add(columnId, new ViewModelAccessor<T>(ViewModel, columnId));
        }
    }

    public interface IViewModelAccessor
    {
        void SetCellValue(GridStyleInfo style, int rowIndex, int columnIndex);
        void SetModelValue(GridStyleInfo style, int rowIndex, int columnIndex);
        
        Type Type { get; }
    }

    public class ViewModelAccessor<T> : IViewModelAccessor
    {
        private readonly ISyncfusionViewModel _viewModel;

        public ViewModelAccessor(ISyncfusionViewModel viewModel, string columnId)
        {
            _viewModel = viewModel;
            ColumnId = columnId;
        }

        public string ColumnId { get; private set; }

        public void SetCellValue(GridStyleInfo style, int rowIndex, int columnIndex)
        {
            // TODO: Need to check with syncfusion if there's another way of setting the value which does not incur boxing.
            style.CellValue = _viewModel.GetValue<T>(rowIndex, columnIndex);
        }

        public void SetModelValue(GridStyleInfo style, int rowIndex, int columnIndex)
        {
            _viewModel.SetValue(rowIndex, columnIndex, (T) style.CellValue);
        }

        public Type Type { get { return typeof (T); } }
    }
}