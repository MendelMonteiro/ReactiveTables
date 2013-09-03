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
using System.Linq;
using ReactiveTables.Demo.Utils;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo
{
    public class HumansViewModel: ReactiveViewModelBase
    {
        private readonly IReactiveTable _humans;

        public HumansViewModel(IReactiveTable humans)
        {
            _humans = humans;
            Humans = new ObservableCollection<HumanViewModel>();

            var subscription = _humans.ReplayAndSubscribe(update =>
                                                              {
                                                                  if (update.IsRowUpdate()) Humans.Add(new HumanViewModel(_humans, update.RowIndex));
                                                              });
            
            CurrentHuman = Humans.LastOrDefault();
//            var id = 3;
//            Add = new DelegateCommand(() => { AddHuman(id, "Human #" + id); id++; });
        }

        /*private int CreateTestData()
        {
            var idCol = _humans.AddColumn(new ReactiveColumn<int>(HumanColumns.IdColumn));
            var nameCol = _humans.AddColumn(new ReactiveColumn<string>(HumanColumns.NameColumn));
            var idNameCol = _humans.AddColumn(new ReactiveCalculatedColumn2<string, int, string>(
                                                  HumanColumns.IdNameColumn,
                                                  idCol,
                                                  nameCol,
                                                  (idVal, nameVal) => idVal + nameVal));
            var id = 1;
            var _rowIndex = AddHuman(id++, "Mendel");
            _rowIndex = AddHuman(id++, "Marie");
            return id;
        }*/

        /*private int AddHuman(int id, string name)
        {
            int rowIndex = _humans.AddRow();
            _humans.SetValue(HumanColumns.IdColumn, rowIndex, id);
            _humans.SetValue(HumanColumns.NameColumn, rowIndex, name);
            return rowIndex;
        }*/

        public ObservableCollection<HumanViewModel> Humans { get; private set; }

        public HumanViewModel CurrentHuman { get; private set; }

        public DelegateCommand Add { get; private set; }
    }
}
