using System.Windows;

namespace ReactiveTables.Demo
{
    /// <summary>
    /// Interaction logic for SyncfusionTest.xaml
    /// </summary>
    public partial class SyncfusionTest : Window
    {
        public SyncfusionTest()
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
