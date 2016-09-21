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
using ReactiveTables.Demo.Utils;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo
{
    public class PersonAccountsViewModel : ReactiveViewModelBase, IDisposable
    {
        private readonly IReactiveTable _personAccounts;
        private int _currentRowIndex;
        private readonly IDisposable _subscription;

        public PersonAccountsViewModel(IReactiveTable personAccounts, IWritableReactiveTable accounts)
        {
            _personAccounts = personAccounts;

            PersonAccounts = new IndexedObservableCollection<PersonAccountViewModel, int>(h=>h.RowIndex);
            _subscription = _personAccounts.ReplayAndSubscribe(OnNext);

            Change = new DelegateCommand(
                    () => accounts.SetValue(AccountColumns.AccountBalance, CurrentRowIndex, (decimal) DateTime.Now.Millisecond));

            _personAccounts.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, CurrentRowIndex);
        }

        private void OnNext(TableUpdate update)
        {
            if (update.Action == TableUpdateAction.Add)
            {
                PersonAccounts.Add(new PersonAccountViewModel(_personAccounts, update.RowIndex));
            }
            else if (update.Action == TableUpdateAction.Delete)
            {
                PersonAccounts.RemoveAt(PersonAccounts.GetIndexForKey(update.RowIndex));
            }
        }

        public IndexedObservableCollection<PersonAccountViewModel, int> PersonAccounts { get; }

        public int CurrentRowIndex
        {
            get { return _currentRowIndex; }
            private set
            {
                _personAccounts.ChangeNotifier.UnregisterPropertyNotifiedConsumer(this, _currentRowIndex);
                _currentRowIndex = value;
                _personAccounts.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, _currentRowIndex);
                OnPropertyChanged("AccountDetails");
            }
        }

        public string AccountDetails => _personAccounts.GetValue<string>(PersonAccountColumns.AccountDetails, CurrentRowIndex);

        public IReactiveTable Table => _personAccounts;

        public DelegateCommand Change { get; private set; }

        public void Dispose()
        {
            _subscription.Dispose();
            _personAccounts.ChangeNotifier.UnregisterPropertyNotifiedConsumer(this, _currentRowIndex);
            foreach (var personAccountViewModel in PersonAccounts)
            {
                personAccountViewModel.Dispose();
            }
        }
    }
}