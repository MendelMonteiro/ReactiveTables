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
            var notifier = new PropertyChangedNotifier(_humans);
            notifier.RegisterPropertyNotifiedConsumer(this, _rowIndex);
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
