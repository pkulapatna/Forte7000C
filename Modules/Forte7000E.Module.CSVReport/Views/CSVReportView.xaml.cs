using Forte7000E.Module.CSVReport.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace Forte7000E.Module.CSVReport.Views
{
    /// <summary>
    /// Interaction logic for CSVReportView.xaml
    /// </summary>
    public partial class CSVReportView : UserControl, IDisposable
    {
        public static CSVReportView CSVDialog;

        public DataTable _archivetable = new DataTable();

        public CSVReportView(DataTable ArchTable, string ArchtabName)
        {
            InitializeComponent();
            CSVDialog = this;
            this.DataContext = new CSVReportViewModel(ArchTable, ArchtabName);
            
        }

        private void FileName_dclick(object sender, MouseButtonEventArgs e)
        {
            if (FileName.IsReadOnly == false)
            {
                FileName.Background = Brushes.AntiqueWhite;
                FileName.IsReadOnly = true;
            }
            else
            {
                FileName.Background = Brushes.White;
                FileName.IsReadOnly = false;
            }
        }

        public void Dispose()
        {
            CSVReportView.CSVDialog = null;
        }

    }
}
