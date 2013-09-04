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

using System.Collections.ObjectModel;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo
{
    public class AccountViewModel : ReactiveViewModelBase
    {
        private readonly IReactiveTable _accounts;
        private readonly int _rowIndex;

        public AccountViewModel(IReactiveTable accounts, int rowIndex)
        {
            _accounts = accounts;
            _rowIndex = rowIndex;

            _accounts.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, _rowIndex);
        }

        public int AccountId
        {
            get { return _accounts.GetValue<int>(AccountColumns.IdColumn, _rowIndex); }
        }

        public int HumanId
        {
            get { return _accounts.GetValue<int>(AccountColumns.HumanId, _rowIndex); }
        }

        public decimal AccountBalance
        {
            get { return _accounts.GetValue<decimal>(AccountColumns.AccountBalance, _rowIndex); }
        }
    }

    public class AccountsViewModel: ReactiveViewModelBase
    {
        private readonly IReactiveTable _accounts;

        public AccountsViewModel(IReactiveTable accounts)
        {
            _accounts = accounts;

            Accounts = new ObservableCollection<AccountViewModel>();
            var subscription = _accounts.ReplayAndSubscribe(update =>
                                                                {
                                                                    if (update.IsRowUpdate()) Accounts.Add(new AccountViewModel(_accounts, update.RowIndex));
                                                                });
        }

        public ObservableCollection<AccountViewModel> Accounts { get; private set; }
    }
}
