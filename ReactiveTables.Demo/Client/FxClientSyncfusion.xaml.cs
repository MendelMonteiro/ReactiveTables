using System;
using System.Windows;

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
        }

        protected override void OnClosed(EventArgs e)
        {
            ViewModel.Dispose();
            base.OnClosed(e);
        }
    }
}
