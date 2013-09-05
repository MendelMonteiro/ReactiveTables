using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveTables.Demo.Server;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo.Client
{
    class FxCurrencyPairViewModel : ReactiveViewModelBase
    {
        private ReactiveTable _table;
        private int _rowId;

        public FxCurrencyPairViewModel(ReactiveTable table, int rowId)
        {
            _table = table;
            _rowId = rowId;
        }

        public string Currency1
        {
            get { return _table.GetValue<string>(FxTableDefinitions.CurrencyPair.Ccy1, _rowId); }
        }

        public string Currency2
        {
            get { return _table.GetValue<string>(FxTableDefinitions.CurrencyPair.Ccy2, _rowId); }
        }
    }
}
