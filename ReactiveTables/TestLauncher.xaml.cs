using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ReactiveTables
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
//            window.Show();
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
