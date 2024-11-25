using Forte7000E.Module.CSVReport.Views;
using Forte7000E.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Windows;

namespace Forte7000E.Module.Archive.Models
{
    public class BaleArchivesModel
    {
        //CSVReportView CSVView;

        public Xmlhandler Xmlhandler { get; set; }
        public Sqlhandler SqlHandler { get; set; }

        private Window CsvWindow;

        public DataTable ArchiveDataTable { get; set; }
        public string ArchiveTableName { get; set; }

        private ObservableCollection<CheckedListItem> AvailableItemList { get; set; }

        public BaleArchivesModel()
        {
            Xmlhandler = Xmlhandler.Instance; // new Xmlhandler();
            SqlHandler = Sqlhandler.Instance; // new Sqlhandler();
        }
        internal List<string> GetSqlTableList()
        {
            return SqlHandler.GetSqltableList();
        }

        internal DataTable GetBaleArchiveTable(string strClause)
        {
            ArchiveDataTable = new DataTable();
            ArchiveDataTable = SqlHandler.GetSqlArchivetable(strClause);

            return ArchiveDataTable;
        }

        private string _XmlGdvDirectory;
        public string XMLRealTimeGdvFile
        {
            get { return _XmlGdvDirectory + Path.Combine(Xmlhandler.SettingsGdvFile); }
            set
            {
                if (value != null)
                    _XmlGdvDirectory = value;
            }
        }

        internal ObservableCollection<string> GetAllItemsListModel()
        {
            ObservableCollection<string> XmlCheckedList = new ObservableCollection<string>();
           
            try
            {
                DataTable HdrTable = SqlHandler.GetSqlTableHdr();
                List<string>  XmlColumnList = Xmlhandler.ReadXmlGridView(XMLRealTimeGdvFile);
                
                if ((HdrTable.Rows.Count > 0) && (XmlColumnList.Count > 0))
                {
                    AvailableItemList = new ObservableCollection<CheckedListItem>();

                    foreach (DataRow item in HdrTable.Rows)
                    {
                        if (AllowField(item[1].ToString()))
                        {
                            if (XmlColumnList.Contains(item[1].ToString()))
                                AvailableItemList.Add(new CheckedListItem(Convert.ToInt32(item[0]), item[1].ToString(), true, item[2].ToString()));
                            else
                                AvailableItemList.Add(new CheckedListItem(Convert.ToInt32(item[0]), item[1].ToString(), false, item[2].ToString()));
                        }
                    }
                    foreach (var item in XmlColumnList)
                    {
                        XmlCheckedList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in GetAllItemsListModel  " + ex.Message);              
            }
            return XmlCheckedList;
        }

        private bool AllowField(string strItem)
        {
            foreach (var item in SqlHandler.RemoveFieldsList)
            {
                if (item == strItem) return false;
            }
            return true;
        }

        public void WriteCSVAllExecute(DataTable archiveDataTable)
        {
            if(ArchiveDataTable.Rows.Count > 0)
            {
                try
                {
                    int iEnd = ArchiveDataTable.Rows.Count;
                    if (iEnd > 0)
                    {
                        using (CSVReportView CSVView = new CSVReportView(archiveDataTable, ArchiveTableName))
                        {
                            CsvWindow = new Window()
                            {
                                Title = "CSV Window",
                                Width = 400,
                                Height = 300,
                                Topmost = true,
                                Content = CSVView 
                            };
                            CsvWindow.ResizeMode = ResizeMode.NoResize;
                            CsvWindow.ShowDialog();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROR in WriteCSVAllExecute " + ex);
                }   
            }
        }

        internal DataTable GetBaleArchiveDataTable(string quaryString)
        {
            return SqlHandler.GetForteDataTable(quaryString);
        }

        internal List<string> GetSqlStockList(string selectTableValue)
        {
            return SqlHandler.GetUniqueStrItemlist("StockName", selectTableValue);
        }

        internal List<string> GetSqlGradeList(string selectTableValue)
        {
            return SqlHandler.GetUniqueStrItemlist("GradeName", selectTableValue);
        }

        internal List<string> GetSqlLineList(string selectTableValue)
        {
            return SqlHandler.GetUniqueStrItemlist("LineID", selectTableValue);
        }

        internal List<string> GetSqlSourceList(string selectTableValue)
        {
            return SqlHandler.GetUniqueStrItemlist("SourceID", selectTableValue);
        }

        internal List<string> GetSqlLotList(string selectLotTableValue)
        {
            return SqlHandler.GetUniqueStrItemlist("LotNum", selectLotTableValue);
        }

        internal List<string> GetSqlLotTableList()
        {
            return SqlHandler.GetLotTableList();
        }

        internal DataTable GetCustomLotArchiveTable(int LotType)
        {
           return SqlHandler.GetLotDataTable(LotType);
        }
    }
}
