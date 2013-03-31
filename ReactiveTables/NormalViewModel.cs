using System.ComponentModel;
using ReactiveTables.Utils;

namespace ReactiveTables
{
    public class NormalViewModel:INotifyPropertyChanged
    {
        private string _name;

        public NormalViewModel()
        {
            Change = new DelegateCommand(() => Name = "Marie");
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public DelegateCommand Change { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}