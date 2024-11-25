using Forte7000E.Module.SerialCom.Properties;
using Forte7000E.Module.SerialCom.ViewModels;
using Prism.Events;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Forte7000E.Module.SerialCom.Views
{
    /// <summary>
    /// Interaction logic for OscillatorView.xaml
    /// </summary>
    public partial class OscillatorView : UserControl
    {
        protected readonly IEventAggregator _eventAggregator;

        public OscillatorView(IEventAggregator EventAggregator)
        {
            InitializeComponent();
            this._eventAggregator = EventAggregator;
            this.DataContext = new OscillatorViewModel(_eventAggregator);
        }

        private void ForteRange_dclick(object sender, MouseButtonEventArgs e)
        {

            if (ForteRange.IsReadOnly == false)
            {
                ForteRange.Background = Brushes.AntiqueWhite;
                ForteRange.IsReadOnly = true;
            }
            else
            {
                ForteRange.Background = Brushes.White;
                ForteRange.IsReadOnly = false;
            }
        }

        private void NumericOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }

        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9.]+");
            return reg.IsMatch(str);
        }

        private void CbRnForte_click(object sender, RoutedEventArgs e)
        {
            if (cbRnForte.IsChecked == true)
            {
                Settings.Default.bOscRandom = true;
                Settings.Default.Save();
            }
        }
    }
}
