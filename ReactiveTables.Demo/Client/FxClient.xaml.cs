using System.Windows;

namespace ReactiveTables.Demo.Client
{
    /// <summary>
    /// Interaction logic for FxClient.xaml
    /// </summary>
    public partial class FxClient : Window
    {
        public FxClient()
        {
            InitializeComponent();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            ViewModel.Dispose();
            base.OnClosed(e);
        }
    }
}
