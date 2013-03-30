using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ReactiveTables
{
    public interface IReactiveConsumer
    {
        void OnPropertyChanged(string propertyName);
    }

    public class ReactiveViewModelBase: INotifyPropertyChanged, IReactiveConsumer
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HumanViewModel: ReactiveViewModelBase
    {
        private readonly IReactiveTable _humans;
        private readonly int _rowIndex;

        public HumanViewModel(IReactiveTable humans, int rowIndex)
        {
            _rowIndex = rowIndex;
            _humans = humans;
//            Change = new DelegateCommand(() => Name += "was changed");
            humans.RegisterConsumer(this, _rowIndex);
        }

        public string Name
        {
            get { return _humans.GetValue<string>(HumanColumns.NameColumn, _rowIndex); }
//            set { _humans.SetValue(HumanColumns.NameColumn, _rowIndex, value); }
        }

        public DelegateCommand Change { get; private set; }

        public string IdName
        {
            get { return _humans.GetValue<string>(HumanColumns.IdNameColumn, _rowIndex); }
        }
    }

    public class HumansViewModel: ReactiveViewModelBase
    {
        private readonly IReactiveTable _humans;

        public HumansViewModel()
        {
            _humans = App.Humans;
            Humans = new ObservableCollection<HumanViewModel>();

            var subscription = _humans.Subscribe(
                DelegateObserver<int>.CreateDelegateObserver(rowIndex => Humans.Add(new HumanViewModel(_humans, rowIndex))));
            
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

    /*public class ReactiveObservableCollection<T> : INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ReactiveTable _table;

        public ReactiveObservableCollection(ReactiveTable table)
        {
            _table = table;
            _table.RegisterConsumer(this);
        }
    }*/
}
