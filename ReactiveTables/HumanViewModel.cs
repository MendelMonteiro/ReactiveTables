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
along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*/
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;
using ReactiveTables.Utils;

namespace ReactiveTables
{
    public class HumanViewModel: ReactiveViewModelBase
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
    }

    /*public class ReactiveObservableCollection<T> : INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ReactiveTable _table;

        public ReactiveObservableCollection(ReactiveTable table)
        {
            _table = table;
            _table.RegisterPropertyNotifiedConsumer(this);
        }
    }*/
}
