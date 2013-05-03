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
using System;
using System.Collections.ObjectModel;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;
using ReactiveTables.Utils;

namespace ReactiveTables
{
    public class HumanAccountsViewModel : ReactiveViewModelBase
    {
        private readonly IReactiveTable _humanAccounts;
        private int _currentRowIndex;

        public HumanAccountsViewModel(IReactiveTable humanAccounts, IWritableReactiveTable accounts)
        {
            _humanAccounts = humanAccounts;

            HumanAccounts = new ObservableCollection<HumanAccountViewModel>();
            _humanAccounts.ReplayAndSubscribe(update =>
                                                  {
                                                      if (update.IsRowUpdate()) HumanAccounts.Add(new HumanAccountViewModel(_humanAccounts, update.RowIndex));
                                                  });

            Change = new DelegateCommand(() => accounts.SetValue(AccountColumns.AccountBalance, CurrentRowIndex, (decimal) DateTime.Now.Millisecond));

            _humanAccounts.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, CurrentRowIndex);
        }

        public ObservableCollection<HumanAccountViewModel> HumanAccounts { get; private set; }

        public int CurrentRowIndex
        {
            get { return _currentRowIndex; }
            private set
            {
                _humanAccounts.ChangeNotifier.UnregisterPropertyNotifiedConsumer(this, _currentRowIndex);
                _currentRowIndex = value;
                _humanAccounts.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, _currentRowIndex);
                OnPropertyChanged("AccountDetails");
            }
        }

        public string AccountDetails
        {
            get { return _humanAccounts.GetValue<string>(HumanAccountColumns.AccountDetails, CurrentRowIndex); }
        }

        public IReactiveTable Table { get { return _humanAccounts; } }

        public DelegateCommand Change { get; private set; }
    }
}
