using System;
using ReactiveTables.Demo.Server;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo.Client
{
    class FxRateViewModel : ReactiveViewModelBase, IDisposable
    {
        private readonly ReactiveTable _table;
        private readonly int _rowId;

        public FxRateViewModel(ReactiveTable table, int rowId)
        {
            _table = table;
            _rowId = rowId;

            _table.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, _rowId);
        }

        public string CurrencyPair { get { return _table.GetValue<string>(FxTableDefinitions.FxRates.CcyPairId, _rowId); } }
        public double Bid { get { return _table.GetValue<double>(FxTableDefinitions.FxRates.Bid, _rowId); } }
        public double Ask { get { return _table.GetValue<double>(FxTableDefinitions.FxRates.Ask, _rowId); } }
        public DateTime Time { get { return _table.GetValue<DateTime>(FxTableDefinitions.FxRates.Time, _rowId); } }

        public void Dispose()
        {
            _table.ChangeNotifier.UnregisterPropertyNotifiedConsumer(this, _rowId);
        }
    }
}
