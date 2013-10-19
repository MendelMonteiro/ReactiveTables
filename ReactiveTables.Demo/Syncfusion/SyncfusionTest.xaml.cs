// This file is part of ReactiveTables.
// 
// ReactiveTables is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReactiveTables is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.

using System.Windows;

namespace ReactiveTables.Demo.Syncfusion
{
    /// <summary>
    /// Interaction logic for SyncfusionTest.xaml
    /// </summary>
    public partial class SyncfusionTest : Window
    {
        public SyncfusionTest()
        {
            InitializeComponent();

            Grid.AddColumn<int>(AccountColumns.IdColumn);
            Grid.AddColumn<decimal>(AccountColumns.AccountBalance);
            Grid.AddColumn<int>(AccountColumns.PersonId);

            Grid.AddColumn<int>(PersonColumns.IdColumn);
            Grid.AddColumn<string>(PersonColumns.IdNameColumn);
            Grid.AddColumn<string>(PersonColumns.NameColumn);

            Grid.AddColumn<string>(PersonAccountColumns.AccountDetails);
        }

        protected override void OnClosed(System.EventArgs e)
        {
            var viewModel = (SyncfusionTestViewModel)DataContext;
            viewModel.Dispose();
            base.OnClosed(e);
        }
    }
}