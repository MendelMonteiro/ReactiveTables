using System.ComponentModel;

namespace ReactiveTables.Framework.UI
{
    public interface IReactivePropertyNotifiedConsumer
    {
        void OnPropertyChanged(string propertyName);
    }

    public class ReactiveViewModelBase: INotifyPropertyChanged, IReactivePropertyNotifiedConsumer
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}