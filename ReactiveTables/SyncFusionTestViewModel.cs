using ReactiveTables.Framework;
using Syncfusion.Windows.Controls.Grid;

namespace ReactiveTables
{
    public class SyncfusionTestViewModel
    {
        private readonly IReactiveTable _table;

        public SyncfusionTestViewModel(IReactiveTable table)
        {
            _table = table;
        }
    }

    public class SyncfusionTestGridControl : GridControlBase
    {
        protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            //e.Style.CellValue = ;
            base.OnQueryCellInfo(e);
        }
    }
}