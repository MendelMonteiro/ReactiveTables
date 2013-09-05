using System.Windows;
using ReactiveTables.Demo.Client;

namespace ReactiveTables.Demo
{
    /// <summary>
    /// Interaction logic for TestLauncher.xaml
    /// </summary>
    public partial class TestLauncher : Window
    {
        public TestLauncher()
        {
            InitializeComponent();
        }

        private void RealTimeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
        }

        private void ExternalDataButton_Click(object sender, RoutedEventArgs e)
        {
            FxClient window = new FxClient();
            window.Show();
        }

        private void SyncfusionButton_Click(object sender, RoutedEventArgs e)
        {
            SyncfusionTest window = new SyncfusionTest();
            window.Show();
        }

        private void XceedButton_Click(object sender, RoutedEventArgs e)
        {
            XceedTest window = new XceedTest();
            window.Show();

        }
    }
}
