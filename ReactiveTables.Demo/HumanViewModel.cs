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
    public class HumanViewModel : ReactiveViewModelBase, IDisposable
    {
        private readonly IReactiveTable _humans;
        private readonly int _rowIndex;

        public HumanViewModel(IReactiveTable humans, int rowIndex)
        {
            _rowIndex = rowIndex;
            _humans = humans;
//            Change = new DelegateCommand(() => Name += "was changed");
            _humans.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, _rowIndex);
        }

        public int HumanId
        {
            get { return _humans.GetValue<int>(HumanColumns.IdColumn, _rowIndex); }
        }

        public string Name
        {
            get { return _humans.GetValue<string>(HumanColumns.NameColumn, _rowIndex); }
//            set { _humans.SetValue(HumanColumns.NameColumn, _rowIndex, value); }
        }

//        public DelegateCommand Change { get; private set; }

        public string IdName
        {
            get { return _humans.GetValue<string>(HumanColumns.IdNameColumn, _rowIndex); }
        }

        public void Dispose()
        {
            _humans.ChangeNotifier.UnregisterPropertyNotifiedConsumer(this, _rowIndex);
        }
    }
}