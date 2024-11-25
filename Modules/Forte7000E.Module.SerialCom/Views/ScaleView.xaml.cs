using Forte7000E.Module.SerialCom.Properties;
using Forte7000E.Module.SerialCom.ViewModels;
using Prism.Events;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Forte7000E.Module.SerialCom.Views
{
    /// <summary>
    /// Interaction logic for ScaleView.xaml
    /// </summary>
    public partial class ScaleView : UserControl
    {
        protected readonly IEventAggregator _eventAggregator;

        public ScaleView(IEventAggregator EventAggregator)
        {    
            InitializeComponent();
            this._eventAggregator = EventAggregator;
            this.DataContext  = new ScaleViewModel(_eventAggregator);

            cbRndom.IsChecked = Settings.Default.bSimWtRandom;
        }

        private void SimWeight_dclick(object sender, MouseButtonEventArgs e)
        {
            if (SimWeight.IsReadOnly == false)
            {
                SimWeight.Background = Brushes.AntiqueWhite;
                SimWeight.IsReadOnly = true;
            }
            else
            {
                SimWeight.Background = Brushes.White;
                SimWeight.IsReadOnly = false;
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

        private void ChkRnd_Click(object sender, RoutedEventArgs e)
        {
            if (cbRndom.IsChecked == true) Settings.Default.bSimWtRandom = true;
            else Settings.Default.bSimWtRandom = false;
            Settings.Default.Save();
        }

        
    }
}
