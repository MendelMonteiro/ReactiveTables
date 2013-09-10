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
using System.Windows;
using ReactiveTables.Framework;
using Syncfusion.Windows.Controls.Cells;
using Syncfusion.Windows.Controls.Grid;

namespace ReactiveTables.Demo.Syncfusion
{
    public class SyncfusionTestGridControl : GridControlBase
    {
        private IDisposable CurrentToken { get; set; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ISyncfusionViewModel), typeof(SyncfusionTestGridControl),
                                        new PropertyMetadata(default(ISyncfusionViewModel), ViewModelChanged));

        private static void ViewModelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            SyncfusionTestGridControl grid = (SyncfusionTestGridControl) dependencyObject;
            if (grid.CurrentToken != null) grid.CurrentToken.Dispose();
            var viewModel = (ISyncfusionViewModel) args.NewValue;
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
                var rowIndex = ViewModel.GetRowPosition(tableUpdate.RowIndex) + Model.HeaderRows;
                var colIndex = ViewModel.GetColPosition(tableUpdate.Column.ColumnId) + Model.HeaderColumns;
                // TODO: handle updates to multiple columns (i.e. CellSpanInfo)
                if (IsRowVisible(rowIndex) && IsColumnVisible(colIndex))
                {
                    Model.InvalidateCell(new RowColumnIndex(rowIndex, colIndex));
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
            var rowIndex = e.Cell.RowIndex - Model.HeaderRows;
            var columnIndex = e.Cell.ColumnIndex - Model.HeaderColumns;
            e.Style.CellValue = ViewModel.GetValue(rowIndex, columnIndex);
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (CurrentToken != null) CurrentToken.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}