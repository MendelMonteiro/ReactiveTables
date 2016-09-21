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

using ReactiveTables.Demo.Server;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo.Client
{
    internal class FxCurrencyPairViewModel : ReactiveViewModelBase
    {
        private readonly ReactiveTable _table;
        private readonly int _rowId;

        public FxCurrencyPairViewModel(ReactiveTable table, int rowId)
        {
            _table = table;
            _rowId = rowId;
        }

        public string CurrencyPair => _table.GetValue<string>(FxTableDefinitions.CurrencyPair.CcyPair, _rowId);

        public string Currency1 => _table.GetValue<string>(FxTableDefinitions.CurrencyPair.Ccy1, _rowId);

        public string Currency2 => _table.GetValue<string>(FxTableDefinitions.CurrencyPair.Ccy2, _rowId);
    }
}