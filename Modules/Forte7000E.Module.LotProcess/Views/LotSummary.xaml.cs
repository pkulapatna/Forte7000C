using Forte7000E.Module.LotProcess.ViewModels;
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

namespace Forte7000E.Module.LotProcess.Views
{
    /// <summary>
    /// Interaction logic for LotSummary.xaml
    /// </summary>
    public partial class LotSummary : UserControl
    {
        public static LotSummary LotSummaryWindows;
        protected readonly IEventAggregator _ea;
        private readonly LotSummaryViewModel _lotSummaryViewModel;

        public LotSummary(IEventAggregator ea)
        {
            InitializeComponent();
            _ea = ea;

            LotSummaryWindows = this;

            _lotSummaryViewModel = new LotSummaryViewModel(_ea);
            this.DataContext = _lotSummaryViewModel;
           
        }

        private void OnAutoGeneratingLotOpenColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            OpenLotDataGrid.Columns[0].Visibility = Visibility.Collapsed;
            CloseLotDataGrid.Columns[0].Visibility = Visibility.Collapsed;
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
                e.Column.ClipboardContentBinding.StringFormat = "MM:dd:yyyy HH:mm";

            if ((e.PropertyType == typeof(System.Single)) || (e.PropertyType == typeof(System.Double)))
            {
                e.Column.ClipboardContentBinding.StringFormat = "{0:0.##}";
                e.Column.Width = e.Column.Header.ToString().Length * 25;
            }
        }

        private void Screen_SizeChanged(object sender, SizeChangedEventArgs e)
        {
           // double dBoxFontSize = e.NewSize.Width * 0.08;
          //  double dColHdrHeight = e.NewSize.Width * 0.03;
          //  double iGVFontsize = e.NewSize.Width * 0.015;

            double ilotFontsize = e.NewSize.Width * 0.012;
            double dLotColHdrHeight = e.NewSize.Width * 0.025;

            OpenLotDataGrid.FontSize = ilotFontsize;
            OpenLotDataGrid.ColumnHeaderHeight = dLotColHdrHeight;

            CloseLotDataGrid.FontSize = ilotFontsize;
            CloseLotDataGrid.ColumnHeaderHeight = dLotColHdrHeight;
        }

        private void OpenLotDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
