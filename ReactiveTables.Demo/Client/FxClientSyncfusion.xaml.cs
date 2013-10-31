using System;
using System.Windows;
using ReactiveTables.Demo.Server;

namespace ReactiveTables.Demo.Client
{
    /// <summary>
    /// Interaction logic for FxClientSyncfusion.xaml
    /// </summary>
    public partial class FxClientSyncfusion : Window
    {
        public FxClientSyncfusion()
        {
            InitializeComponent();
            
            Grid.AddColumn<string>(FxTableDefinitions.FxRates.CcyPairId);
            Grid.AddColumn<double>(FxTableDefinitions.FxRates.Bid);
            Grid.AddColumn<double>(FxTableDefinitions.FxRates.Ask);
            Grid.AddColumn<double>(FxTableDefinitions.FxRates.Open);
            Grid.AddColumn<double>(FxTableDefinitions.FxRates.Close);
            Grid.AddColumn<double>(FxTableDefinitions.FxRates.Change);
            Grid.AddColumn<double>(FxTableDefinitions.FxRates.YearRangeStart);
            Grid.AddColumn<double>(FxTableDefinitions.FxRates.YearRangeEnd);
            Grid.AddColumn<DateTime>(FxTableDefinitions.FxRates.Time);
            Grid.AddColumn<double>(FxTableDefinitions.FxRates.Ticks);
        }

        protected override void OnClosed(EventArgs e)
        {
            var viewModel = (FxClientSyncfusionViewModel)DataContext;
            viewModel.Dispose();
            base.OnClosed(e);
        }
    }
}
