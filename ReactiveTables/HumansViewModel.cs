using System.Collections.ObjectModel;
using System.Linq;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;
using ReactiveTables.Utils;

namespace ReactiveTables
{
    public class HumansViewModel: ReactiveViewModelBase
    {
        private readonly IReactiveTable _humans;

        public HumansViewModel(IReactiveTable humans)
        {
            _humans = humans;
            Humans = new ObservableCollection<HumanViewModel>();

            var subscription = _humans.Subscribe(
                DelegateObserver<RowUpdate>.CreateDelegateObserver(update => Humans.Add(new HumanViewModel(_humans, update.RowIndex))));
            
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