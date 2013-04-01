using System;
using System.Collections.ObjectModel;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;
using ReactiveTables.Utils;

namespace ReactiveTables
{
    public class HumanAccountsViewModel : ReactiveViewModelBase
    {
        private readonly IReactiveTable _humanAccounts;
        private int _currentRowIndex;

        public HumanAccountsViewModel(IReactiveTable humanAccounts, IWritableReactiveTable accounts)
        {
            _humanAccounts = humanAccounts;

            HumanAccounts = new ObservableCollection<HumanAccountViewModel>();
            _humanAccounts.Subscribe(DelegateObserver<RowUpdate>.CreateDelegateObserver(
                update => HumanAccounts.Add(new HumanAccountViewModel(_humanAccounts, update.RowIndex))));

            Change = new DelegateCommand(() =>
                                             {
                                                 accounts.SetValue(AccountColumns.AccountBalance, CurrentRowIndex, (decimal) DateTime.Now.Millisecond);
//                                                 base.OnPropertyChanged("AccountDetails");
                                             });

            _humanAccounts.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, CurrentRowIndex);
        }

        public ObservableCollection<HumanAccountViewModel> HumanAccounts { get; private set; }

        public int CurrentRowIndex
        {
            get { return _currentRowIndex; }
            private set
            {
                _humanAccounts.ChangeNotifier.UnregisterPropertyNotifiedConsumer(this, _currentRowIndex);
                _currentRowIndex = value;
                _humanAccounts.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, _currentRowIndex);
                OnPropertyChanged("AccountDetails");
            }
        }

        public string AccountDetails
        {
            get { return _humanAccounts.GetValue<string>(HumanAccountColumns.AccountDetails, CurrentRowIndex); }
        }

        public DelegateCommand Change { get; private set; }
    }
}