using System.Collections.ObjectModel;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables
{
    class MainViewModel
    {
        public MainViewModel()
        {
            Humans = new HumansViewModel(App.Humans);
            Accounts = new AccountsViewModel(App.Accounts);
            HumanAccounts = new HumanAccountsViewModel(App.AccountHumans);
        }

        public AccountsViewModel Accounts { get; private set; }
        public HumansViewModel Humans { get; private set; }
        public HumanAccountsViewModel HumanAccounts { get; private set; }
    }

    public class HumanAccountViewModel : ReactiveViewModelBase
    {
        private readonly IReactiveTable _humanAccounts;
        private readonly int _rowIndex;

        public HumanAccountViewModel(IReactiveTable humanAccounts, int rowIndex)
        {
            _rowIndex = rowIndex;
            _humanAccounts = humanAccounts;

            _humanAccounts.RegisterPropertyNotifiedConsumer(this, rowIndex);
        }

        public int AccountId { get { return _humanAccounts.GetValue<int>(AccountColumns.IdColumn, _rowIndex); } }
        public int HumanId { get { return _humanAccounts.GetValue<int>(HumanColumns.IdColumn, _rowIndex); } }
        public decimal AccountBalance { get { return _humanAccounts.GetValue<decimal>(AccountColumns.AccountBalance, _rowIndex); } }
        public string Name { get { return _humanAccounts.GetValue<string>(HumanColumns.NameColumn, _rowIndex); } }
    }

    public class HumanAccountsViewModel : ReactiveViewModelBase
    {
        private readonly IReactiveTable _humanAccounts;

        public HumanAccountsViewModel(IReactiveTable humanAccounts)
        {
            _humanAccounts = humanAccounts;

            HumanAccounts = new ObservableCollection<HumanAccountViewModel>();
            _humanAccounts.Subscribe(DelegateObserver<RowUpdate>.CreateDelegateObserver(
                update => HumanAccounts.Add(new HumanAccountViewModel(_humanAccounts, update.RowIndex))));
        }

        public ObservableCollection<HumanAccountViewModel> HumanAccounts { get; private set; }
    }
}
