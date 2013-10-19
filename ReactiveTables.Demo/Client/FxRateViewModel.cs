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
using ReactiveTables.Demo.Server;
using ReactiveTables.Demo.Services;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo.Client
{
    internal class FxRateViewModel : ReactiveViewModelBase, IDisposable
    {
        private readonly ReactiveTable _table;
        private readonly int _rowId;

        public FxRateViewModel(ReactiveTable table, int rowId)
        {
            _table = table;
            _rowId = rowId;

            _table.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, _rowId);
        }

        public string CurrencyPair
        {
            get { return _table.GetValue<string>(FxTableDefinitions.FxRates.CcyPairId, _rowId); }
        }

        public double Bid
        {
            get { return _table.GetValue<double>(FxTableDefinitions.FxRates.Bid, _rowId); }
        }

        public double Ask
        {
            get { return _table.GetValue<double>(FxTableDefinitions.FxRates.Ask, _rowId); }
        }

        public double Open
        {
            get { return _table.GetValue<double>(FxTableDefinitions.FxRates.Open, _rowId); }
        }

        public double Close
        {
            get { return _table.GetValue<double>(FxTableDefinitions.FxRates.Close, _rowId); }
        }

        public double YearRangeStart
        {
            get { return _table.GetValue<double>(FxTableDefinitions.FxRates.YearRangeStart, _rowId); }
        }

        public double YearRangeEnd
        {
            get { return _table.GetValue<double>(FxTableDefinitions.FxRates.YearRangeEnd, _rowId); }
        }

        public double Change
        {
            get { return _table.GetValue<double>(FxTableDefinitions.FxRates.Change, _rowId); }
        }

        public DateTime Time
        {
            get { return _table.GetValue<DateTime>(FxTableDefinitions.FxRates.Time, _rowId); }
        }

        public double LongTime
        {
            get { return _table.GetValue<double>(FxDataService.CalculateColumns.FxRates.LongTime, _rowId); }
        }

        public void Dispose()
        {
            _table.ChangeNotifier.UnregisterPropertyNotifiedConsumer(this, _rowId);
        }
    }
}