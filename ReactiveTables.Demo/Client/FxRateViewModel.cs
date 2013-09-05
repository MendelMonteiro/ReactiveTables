using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo.Client
{
    class FxRateViewModel : ReactiveViewModelBase
    {
        private ReactiveTable _table;
        private int _rowId;

        public FxRateViewModel(ReactiveTable table, int rowId)
        {
            _table = table;
            _rowId = rowId;
        }
    }
}
