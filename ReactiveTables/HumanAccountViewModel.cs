/*This file is part of ReactiveTables.

ReactiveTables is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ReactiveTables is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
*/
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables
{
    public class HumanAccountViewModel : ReactiveViewModelBase
    {
        private readonly IReactiveTable _humanAccounts;
        private readonly int _rowIndex;

        public HumanAccountViewModel(IReactiveTable humanAccounts, int rowIndex)
        {
            _rowIndex = rowIndex;
            _humanAccounts = humanAccounts;

            _humanAccounts.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, rowIndex);
        }

        public int AccountId { get { return _humanAccounts.GetValue<int>(AccountColumns.IdColumn, _rowIndex); } }
        public int HumanId { get { return _humanAccounts.GetValue<int>(HumanColumns.IdColumn, _rowIndex); } }
        public decimal AccountBalance { get { return _humanAccounts.GetValue<decimal>(AccountColumns.AccountBalance, _rowIndex); } }
        public string Name { get { return _humanAccounts.GetValue<string>(HumanColumns.NameColumn, _rowIndex); } }
        public string AccountDetails { get { return _humanAccounts.GetValue<string>(HumanAccountColumns.AccountDetails, _rowIndex); } }
    }
}
