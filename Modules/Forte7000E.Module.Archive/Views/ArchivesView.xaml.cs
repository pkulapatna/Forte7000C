using Forte7000E.Module.Archive.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Forte7000E.Module.Archive.Views
{
    /// <summary>
    /// Interaction logic for ArchivesView.xaml
    /// </summary>
    public partial class ArchivesView : UserControl
    {
        public static ArchivesView archivesView;

        private double wdCoef = 0.0;

        public ArchivesView()
        {
            InitializeComponent();
            this.DataContext = new ArchivesViewModel();
            archivesView = this;
        }

        private void GridView_sidechanged(object sender, SizeChangedEventArgs e)
        {
            double dColHdrHeight = e.NewSize.Width * 0.03;
            double iGVFontsize = e.NewSize.Width * 0.01;
            double iGVRowHeight = e.NewSize.Width * 0.020;
            wdCoef = e.NewSize.Width * 0.08;

            GridBaleArchive.ColumnHeaderHeight = dColHdrHeight;
            GridBaleArchive.FontSize = iGVFontsize;
            GridBaleArchive.RowHeight = iGVRowHeight;
            GridBaleArchive.UpdateLayout();
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            GridBaleArchive.Columns[0].Visibility = Visibility.Collapsed;


            if (e.PropertyName.StartsWith("Finish"))
                e.Column.Header = "Viscosity";
            if (e.PropertyName.StartsWith("Package"))
                e.Column.Header = "Wrap";
            if (e.PropertyName.StartsWith("Brightness"))
                e.Column.Header = "Bright";
            if (e.PropertyName.StartsWith("ForteStatus"))
                e.Column.Header = "FtMsg";
            if (e.PropertyName.StartsWith("MoistureStatus"))
                e.Column.Header = "McMsg";
            if (e.PropertyName.StartsWith("TareWeight"))
                e.Column.Header = "Tare kg";
            if (e.PropertyName.StartsWith("FC_LotIdentString"))
                e.Column.Header = "Batch ID";
            if (e.PropertyName.StartsWith("LotBaleNumber"))
                e.Column.Header = "Bale #";
            if (e.PropertyName.StartsWith("Position"))
                e.Column.Header = "Pos";
            if (e.PropertyName.StartsWith("SpareSngFld3"))
                e.Column.Header = "CV %";

            if ((e.PropertyType == typeof(System.Single)) || (e.PropertyType == typeof(System.Double)))
            {
                e.Column.ClipboardContentBinding.StringFormat = "{0:0.##}";
                e.Column.Width = e.Column.Header.ToString().Length + wdCoef;
            }
            else if ((e.PropertyType == typeof(System.Single)) || (e.PropertyType == typeof(System.DateTime)))
            {
                e.Column.ClipboardContentBinding.StringFormat = "MM-dd-yyyy HH:mm";
                e.Column.Width = e.Column.Header.ToString().Length + wdCoef * 1.7;
            }
            else
                e.Column.Width = e.Column.Header.ToString().Length + wdCoef;// * 10;
        }

        private void Grid_mouseDown(object sender, MouseButtonEventArgs e)
        {
            Window parent = Window.GetWindow(ArchivesView.archivesView);
            parent.DragMove();
        }

    }
}
