using System.Collections.ObjectModel;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables
{
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