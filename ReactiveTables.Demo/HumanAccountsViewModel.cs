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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveTables.Demo.Utils;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;
using System.Linq;

namespace ReactiveTables.Demo
{
    public class HumanAccountsViewModel : ReactiveViewModelBase, IDisposable
    {
        private readonly IReactiveTable _humanAccounts;
        private int _currentRowIndex;
        private readonly IDisposable _subscription;

        public HumanAccountsViewModel(IReactiveTable humanAccounts, IWritableReactiveTable accounts)
        {
            _humanAccounts = humanAccounts;

            HumanAccounts = new ObservableCollection<HumanAccountViewModel>();
            _subscription = _humanAccounts.ReplayAndSubscribe(OnNext);

            Change =
                new DelegateCommand(
                    () => accounts.SetValue(AccountColumns.AccountBalance, CurrentRowIndex, (decimal) DateTime.Now.Millisecond));

            _humanAccounts.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, CurrentRowIndex);
        }

        private void OnNext(TableUpdate update)
        {
            if (update.Action == TableUpdate.TableUpdateAction.Add)
            {
                HumanAccounts.Add(new HumanAccountViewModel(_humanAccounts, update.RowIndex));
            }
            else if (update.Action == TableUpdate.TableUpdateAction.Delete)
            {
                HumanAccounts.RemoveAt(HumanAccounts.TakeWhile(h => h.RowIndex != update.RowIndex).Count());
            }
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

        public IReactiveTable Table
        {
            get { return _humanAccounts; }
        }

        public DelegateCommand Change { get; private set; }

        public void Dispose()
        {
            _subscription.Dispose();
            _humanAccounts.ChangeNotifier.UnregisterPropertyNotifiedConsumer(this, _currentRowIndex);
            foreach (var humanAccountViewModel in HumanAccounts)
            {
                humanAccountViewModel.Dispose();
            }
        }
    }
}