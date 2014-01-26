using System;
using System.Windows;

namespace ReactiveTables.Demo.Syncfusion
{
    public class SyncfusionWindow<T> : Window where T : IDisposable
    {
        protected override void OnClosed(EventArgs e)
        {
            var viewModel = (T)DataContext;
            viewModel.Dispose();
            base.OnClosed(e);
        }
    }
}