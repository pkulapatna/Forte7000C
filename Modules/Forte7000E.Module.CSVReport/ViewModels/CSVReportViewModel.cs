using Forte7000E.Module.CSVReport.Properties;
using Forte7000E.Module.CSVReport.Views;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Forte7000E.Module.CSVReport.ViewModels
{
    public class CSVReportViewModel : BindableBase
    {
        public DataTable MyDataTable { get; set; }

        private string _StrFileName;
        public string StrFileName
        {
            get { return _StrFileName; }
            set { SetProperty(ref _StrFileName, value); }
        }

        private string _strFileLocation;
        public string StrFileLocation
        {
            get { return _strFileLocation; }
            set { SetProperty(ref _strFileLocation, value); }
        }

        private string _strPathFile;
        public string StrPathFile
        {
            get { return _strPathFile; }
            set { SetProperty(ref _strPathFile, value); }
        }

        private bool _bCSVCanwrite;
        public bool BCSVCanwrite
        {
            get { return _bCSVCanwrite; }
            set { SetProperty(ref _bCSVCanwrite, value); }
        }

        private DelegateCommand _WriteCommand;
        public DelegateCommand WriteCommand =>
        _WriteCommand ?? (_WriteCommand =
            new DelegateCommand(WriteCommandExecute).ObservesCanExecute(() => BCSVCanwrite));

        private void WriteCommandExecute()
        {
            StrPathFile = StrFileLocation + "\\" + StrFileName + ".csv";
            try
            {
                if (MyDataTable.Rows.Count > 0)
                {
                    StreamWriter outFile = new StreamWriter(StrPathFile);
                    List<string> headerValues = new List<string>();
                    foreach (DataColumn column in MyDataTable.Columns)
                    {
                        headerValues.Add(QuoteValue("'" + column.ColumnName));
                    }
                    //Header
                    outFile.WriteLine(string.Join(",", headerValues.ToArray()));

                    foreach (DataRow row in MyDataTable.Rows)
                    {
                        string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                        outFile.WriteLine(String.Join(",", fields));
                    }
                    outFile.Close();
                }

                //At the end
                Settings.Default.CsvFileLocation = StrFileLocation;
                Settings.Default.Save();
                BCSVCanwrite = false;
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in WriteExecute " + ex);
            }
            finally
            {
                MessageBox.Show("DONE!");
                Window parent = Window.GetWindow(CSVReportView.CSVDialog);
                parent.Close();

            }
        }


        private DelegateCommand _browseCommand;
        public DelegateCommand BrowseCommand =>
        _browseCommand ?? (_browseCommand =
            new DelegateCommand(BrowseExecute));
        private void BrowseExecute()
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    StrFileLocation = dlg.SelectedPath;
                }
                dlg = null;
                FindCreateDir(StrFileLocation);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in BrowseExecute " + ex);
            }
        }

        public CSVReportViewModel(DataTable ArchiveTable, string archtabName)
        {
            MyDataTable = ArchiveTable;
            StrFileName = archtabName;
            StrFileLocation = Settings.Default.CsvFileLocation;

            if (StrFileLocation.Length > 0)
                        BCSVCanwrite = true;
        }

        public string QuoteValue(string value)
        {
            return string.Concat("" + value + "");
        }

        public void FindCreateDir(string dirname)
        {
            try
            {
                if (!Directory.Exists(dirname))
                {
                    DirectoryInfo Di = Directory.CreateDirectory(dirname);
                    Di.Attributes = FileAttributes.ReadOnly;
                    Di.Refresh();
                    Settings.Default.CsvFileLocation = dirname;
                    Settings.Default.Save();
                }
                
                BCSVCanwrite = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in findCreateDir " + ex);
            }
        }
    }
}
