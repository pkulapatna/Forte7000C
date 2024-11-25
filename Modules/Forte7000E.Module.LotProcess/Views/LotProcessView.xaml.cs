using Forte7000E.Module.LotProcess.ViewModels;
using Prism.Events;
using System.Windows.Controls;
using System.Windows.Media;

namespace Forte7000E.Module.LotProcess.Views
{
    /// <summary>
    /// Interaction logic for LotProcessView.xaml
    /// </summary>
    public partial class LotProcessView : UserControl
    {

        protected readonly IEventAggregator _ea;

        public LotProcessView(IEventAggregator ea)
        {
            InitializeComponent();
            _ea = ea;
            this.DataContext = new LotProcessViewModel(_ea);

        }

        private void LotMax_dlcick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LotMax.IsReadOnly == false)
            {
                LotMax.Background = Brushes.AntiqueWhite;
                LotMax.IsReadOnly = true;
            }
            else
            {
                LotMax.Background = Brushes.White;
                LotMax.IsReadOnly = false;
            }
        }

        private void CustomLotSize_dlcick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CustomLotSize.IsReadOnly == false)
            {
                CustomLotSize.Background = Brushes.AntiqueWhite;
                CustomLotSize.IsReadOnly = true;
            }
            else
            {
                CustomLotSize.Background = Brushes.White;
                CustomLotSize.IsReadOnly = false;
            }
        }
        private void NumericOnly(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }
        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9.]+");
            return reg.IsMatch(str);
        }


    }
}
