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
    public class PersonViewModel : ReactiveViewModelBase, IDisposable
    {
        private readonly IReactiveTable _people;
        private readonly int _rowIndex;

        public PersonViewModel(IReactiveTable people, int rowIndex)
        {
            _rowIndex = rowIndex;
            _people = people;
//            Change = new DelegateCommand(() => Name += "was changed");
            _people.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, _rowIndex);
        }

        public int PersonId
        {
            get { return _people.GetValue<int>(PersonColumns.IdColumn, _rowIndex); }
        }

        public string Name
        {
            get { return _people.GetValue<string>(PersonColumns.NameColumn, _rowIndex); }
//            set { _People.SetValue(PersonColumns.NameColumn, _rowIndex, value); }
        }

//        public DelegateCommand Change { get; private set; }

        public string IdName
        {
            get { return _people.GetValue<string>(PersonColumns.IdNameColumn, _rowIndex); }
        }

        public void Dispose()
        {
            _people.ChangeNotifier.UnregisterPropertyNotifiedConsumer(this, _rowIndex);
        }
    }
}