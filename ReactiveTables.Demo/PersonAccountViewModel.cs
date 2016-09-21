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
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo
{
    public class PersonAccountViewModel : ReactiveViewModelBase, IDisposable
    {
        private readonly IReactiveTable _personAccounts;
        private readonly int _rowIndex;

        public PersonAccountViewModel(IReactiveTable personAccounts, int rowIndex)
        {
            _rowIndex = rowIndex;
            _personAccounts = personAccounts;

            _personAccounts.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, rowIndex);
        }

        public int AccountId => _personAccounts.GetValue<int>(AccountColumns.IdColumn, _rowIndex);

        public int PersonId => _personAccounts.GetValue<int>(PersonColumns.IdColumn, _rowIndex);

        public decimal AccountBalance => _personAccounts.GetValue<decimal>(AccountColumns.AccountBalance, _rowIndex);

        public string Name => _personAccounts.GetValue<string>(PersonColumns.NameColumn, _rowIndex);

        public string AccountDetails => _personAccounts.GetValue<string>(PersonAccountColumns.AccountDetails, _rowIndex);

        public int RowIndex => _rowIndex;

        public void Dispose()
        {
            _personAccounts.ChangeNotifier.UnregisterPropertyNotifiedConsumer(this, _rowIndex);
        }
    }
}