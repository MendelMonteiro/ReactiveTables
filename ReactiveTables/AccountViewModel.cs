using System.Collections.ObjectModel;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables
{
    public class AccountViewModel : ReactiveViewModelBase
    {
        private readonly IReactiveTable _accounts;
        private readonly int _rowIndex;

        public AccountViewModel(IReactiveTable accounts, int rowIndex)
        {
            _accounts = accounts;
            _rowIndex = rowIndex;

            _accounts.RegisterPropertyNotifiedConsumer(this, _rowIndex);
        }

        public int AccountId
        {
            get { return _accounts.GetValue<int>(AccountColumns.IdColumn, _rowIndex); }
        }

        public int HumanId
        {
            get { return _accounts.GetValue<int>(AccountColumns.HumanId, _rowIndex); }
        }

        public decimal AccountBalance
        {
            get { return _accounts.GetValue<decimal>(AccountColumns.AccountBalance, _rowIndex); }
        }
    }

    public class AccountsViewModel: ReactiveViewModelBase
    {
        private readonly IReactiveTable _accounts;

        public AccountsViewModel(IReactiveTable accounts)
        {
            _accounts = accounts;

            Accounts = new ObservableCollection<AccountViewModel>();
            var subscription = _accounts.Subscribe(
                DelegateObserver<RowUpdate>.CreateDelegateObserver(rowIndex => Accounts.Add(new AccountViewModel(_accounts, rowIndex.RowIndex))));
        }

        public ObservableCollection<AccountViewModel> Accounts { get; private set; }
    }
}
