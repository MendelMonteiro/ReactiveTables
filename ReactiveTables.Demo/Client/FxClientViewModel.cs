using System;
using System.Collections.ObjectModel;
using System.Windows;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo.Client
{
    class FxClientViewModel : ReactiveViewModelBase, IDisposable
    {
        private readonly FxDataService _dataService = new FxDataService();
        private readonly IDisposable _subscription;

        public FxClientViewModel()
        {
            CurrencyPairs = new ObservableCollection<FxCurrencyPairViewModel>();
            
            _subscription = _dataService.Currencies.ReplayAndSubscribe(update =>
            {
                if (update.IsRowUpdate())
                {
                    CurrencyPairs.Add(new FxCurrencyPairViewModel(_dataService.Currencies, update.RowIndex));
                }
            });

            _dataService.Start(Application.Current.Dispatcher);
        }

        public ObservableCollection<FxRateViewModel> Rates { get; set; }
        public ObservableCollection<FxCurrencyPairViewModel> CurrencyPairs { get; set; }
        
        public void Dispose()
        {
            _dataService.Stop();
            if (_subscription != null) _subscription.Dispose();
        }
    }
}
