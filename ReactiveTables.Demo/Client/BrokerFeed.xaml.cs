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
using ReactiveTables.Demo.Server;

namespace ReactiveTables.Demo.Client
{
    /// <summary>
    /// Interaction logic for BrokerFeed.xaml
    /// </summary>
    public partial class BrokerFeed
    {
        public BrokerFeed()
        {
            InitializeComponent();

            Grid.AddColumn<string>(BrokerTableDefinition.BrokerColumns.CcyPairColumn);
            Grid.AddColumn<double>(BrokerTableDefinition.BrokerColumns.BidColumn);
            Grid.AddColumn<double>(BrokerTableDefinition.BrokerColumns.AskColumn);
            Grid.AddColumn<string>(BrokerTableDefinition.BrokerColumns.BrokerNameColumn);
            Grid.AddColumn<string>(BrokerTableDefinition.BrokerColumns.MaturityColumn);
        }
    }
}
