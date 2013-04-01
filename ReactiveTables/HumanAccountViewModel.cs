using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables
{
    public class HumanAccountViewModel : ReactiveViewModelBase
    {
        private readonly IReactiveTable _humanAccounts;
        private readonly int _rowIndex;

        public HumanAccountViewModel(IReactiveTable humanAccounts, int rowIndex)
        {
            _rowIndex = rowIndex;
            _humanAccounts = humanAccounts;

            _humanAccounts.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, rowIndex);
        }

        public int AccountId { get { return _humanAccounts.GetValue<int>(AccountColumns.IdColumn, _rowIndex); } }
        public int HumanId { get { return _humanAccounts.GetValue<int>(HumanColumns.IdColumn, _rowIndex); } }
        public decimal AccountBalance { get { return _humanAccounts.GetValue<decimal>(AccountColumns.AccountBalance, _rowIndex); } }
        public string Name { get { return _humanAccounts.GetValue<string>(HumanColumns.NameColumn, _rowIndex); } }
        public string AccountDetails { get { return _humanAccounts.GetValue<string>(HumanAccountColumns.AccountDetails, _rowIndex); } }
    }
}