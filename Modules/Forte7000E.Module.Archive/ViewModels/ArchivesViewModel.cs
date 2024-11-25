using Forte7000E.Module.Archive.Models;
using Forte7000E.Module.Archive.Properties;
using Forte7000E.Module.Archive.Views;
using Forte7000E.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace Forte7000E.Module.Archive.ViewModels
{
    public class ArchivesViewModel : BindableBase
    {
        private readonly BaleArchivesModel _baleArchivesModel;

      
        public DelegateCommand WriteCSVAllCommand { get; set; }
        public string StockSelected { get; set; }
        public string GradeSelected { get; set; }
        public string LineSelected { get; set; }
        public string SourceSelected { get; set; }
        public string LotSelected { get; set; }

        private string _strStatus;
        public string StringStatus
        {
            get { return _strStatus; }
            set { SetProperty(ref _strStatus, value); }
        }

        public DelegateCommand ShowGraphCommand { get; set; }
        private DataTable[] splittedtables;

        private DataTable _archiveDataTable;
        public DataTable ArchiveDataTable
        {
            get { return _archiveDataTable; }
            set { SetProperty(ref _archiveDataTable, value); }
        }

        private List<string> _monthtableList;
        public List<string> MonthTableList
        {
            get { return _monthtableList; }
            set { SetProperty(ref _monthtableList, value); }
        }
        private int _selecttableindex;
        public int SelectTableIndex
        {
            get { return _selecttableindex; }
            set { SetProperty(ref _selecttableindex, value); }
        }

        private bool _blockChecked = true;
        public bool BlockChecked
        {
            get { return _blockChecked; }
            set
            {
                if (value) BoxSelectorOpac = 1.0;
                else BoxSelectorOpac = 0.2;
                SetProperty(ref _blockChecked, value);
            }
        }

        private bool _cmbPagesenable;
        public bool CmbPagesEnable
        {
            get { return _cmbPagesenable; }
            set { SetProperty(ref _cmbPagesenable, value); }
        }

        private double _BoxSelectorOpac = 1.0;
        public double BoxSelectorOpac
        {
            get { return _BoxSelectorOpac; }
            set { SetProperty(ref _BoxSelectorOpac, value); }
        }

        private string _selectTableValue;
        public string SelectTableValue
        {

            get { return _selectTableValue; }
            set
            {
                SetProperty(ref _selectTableValue, value);

                string strMonth = SelectTableValue.Substring(11, 3);
                string strYear = SelectTableValue.Substring(14, 2);
                SelectedMonth = strMonth + strYear;
                string startDate = "01" + "/" + strMonth + "/" + strYear;

                DateTime.TryParse(startDate, out DateTime dateValue);

                if (SelectTableIndex == 0)
                {
                    StartDateProp = dateValue;
                    EndDateProp = DateTime.Now;
                }
                else
                {
                    StartDateProp = dateValue;
                    EndDateProp = dateValue;
                }
            }
        }

        private bool _MonthChecked;
        public bool MonthChecked
        {
            get { return _MonthChecked; }
            set { SetProperty(ref _MonthChecked, value); }
        }

        private bool _daychecked;
        public bool DayChecked
        {
            get { return _daychecked; }
            set { SetProperty(ref _daychecked, value); }
        }

        private List<string> _occrlist;
        public List<string> Occrlist
        {
            get { return _occrlist; }
            set { SetProperty(ref _occrlist, value); }
        }
        private int _selectOccr;
        public int SelectOccr
        {
            get { return _selectOccr; }
            set { SetProperty(ref _selectOccr, value); }
        }


        private int _selectLotOccr;
        public int SelectLotOccr
        {
            get { return _selectLotOccr; }
            set { SetProperty(ref _selectLotOccr, value); }
        }




        private int _recCount;
        public int RecCount
        {
            get { return _recCount; }
            set { SetProperty(ref _recCount, value); }
        }

        private bool _quanEnable;
        public bool QuanEnable
        {
            get { return _quanEnable; }
            set { SetProperty(ref _quanEnable, value); }
        }

        private string _eventValue;
        public string EventValue
        {
            get { return _eventValue; }
            set
            {
                if (value == "All")
                    QuanEnable = false;
                else
                    QuanEnable = true;
                SetProperty(ref _eventValue, value);
            }
        }

        private bool _queryOn;
        public bool QueryOn
        {
            get { return _queryOn; }
            set { SetProperty(ref _queryOn, value); }
        }


        private Nullable<DateTime> _endDateProp = null;
        public Nullable<DateTime> EndDateProp
        {
            get
            {
                if (_endDateProp == null)
                    _endDateProp = DateTime.Today;
                return _endDateProp;
            }
            set { SetProperty(ref _endDateProp, value); }
        }

        private Nullable<DateTime> _startDateProp = null;
        public Nullable<DateTime> StartDateProp
        {
            get
            {
                if (_startDateProp == null)
                    _startDateProp = DateTime.Today;

                return _startDateProp;
            }
            set { SetProperty(ref _startDateProp, value); }
        }

        private string _Selectedmonth;
        public string SelectedMonth
        {
            get { return _Selectedmonth; }
            set { SetProperty(ref _Selectedmonth, value); }
        }

     
        private bool _SelectBaleTab;
        public bool SelectBaleTab
        {
            get { return _SelectBaleTab; }
            set
            {
                if (value)
                {
                    QueryOn = false;
                    WriteOpc = 1;
                    BSelbox = true;
                }
                SetProperty(ref _SelectBaleTab, value);
            }
        }
        private bool _bSelbox;
        public bool BSelbox
        {
            get { return _bSelbox; }
            set { SetProperty(ref _bSelbox, value); }
        }

        private bool _SelectLotTab;
        public bool SelectLotTab
        {
            get { return _SelectLotTab; }
            set
            {
                if (value)
                { 
                    QueryOn = false;
                    WriteOpc = 0.2;
                    BSelbox = false; 
                }
                SetProperty(ref _SelectLotTab, value);
            }
        }

        private double _wrtopc;
        public double WriteOpc
        {
            get { return _wrtopc; }
            set { SetProperty(ref _wrtopc, value); }
        }

        private Nullable<DateTime> _endQueryDate = null;
        public Nullable<DateTime> EndQueryDate
        {
            get
            {
                if (_endQueryDate == null)
                    _endQueryDate = DateTime.Today;
                return _endQueryDate;
            }
            set { SetProperty(ref _endQueryDate, value); }
        }

        private Nullable<DateTime> _startQueryDate = null;
        public Nullable<DateTime> StartQueryDate
        {
            get
            {
                if (_startQueryDate == null)
                    _startQueryDate = DateTime.Today;
                return _startQueryDate;
            }
            set { SetProperty(ref _startQueryDate, value); }
        }


        private List<string> _stockList;
        public List<string> StockList
        {
            get { return _stockList; }
            set { SetProperty(ref _stockList, value); }
        }

        private List<string> _gradeList;
        public List<string> GradeList
        {
            get { return _gradeList; }
            set { SetProperty(ref _gradeList, value); }
        }

        private List<string> _lineList;
        public List<string> LineList
        {
            get { return _lineList; }
            set { SetProperty(ref _lineList, value); }
        }

        private List<string> _sourceList;
        public List<string> SourceList
        {
            get { return _sourceList; }
            set { SetProperty(ref _sourceList, value); }
        }

        private bool _stockChecked;
        public bool StockChecked
        {
            get { return _stockChecked; }
            set
            {
                SetProperty(ref _stockChecked, value);
                if (value)
                {
                    if (SelectBaleTab)
                        StockList = _baleArchivesModel.GetSqlStockList(SelectTableValue);
                }
                else
                    StockList.Clear();
            }
        }


        private bool _gradeChecked;
        public bool GradeChecked
        {
            get { return _gradeChecked; }
            set
            {
                SetProperty(ref _gradeChecked, value);
                GradeList = _baleArchivesModel.GetSqlGradeList(SelectTableValue);
            }
        }

        private bool _lineChecked;
        public bool LineChecked
        {
            get { return _lineChecked; }
            set
            {
                SetProperty(ref _lineChecked, value);
                LineList = _baleArchivesModel.GetSqlLineList(SelectTableValue);
            }
        }

        private bool _sourceChecked;
        public bool SourceChecked
        {
            get { return _sourceChecked; }
            set
            {
                SetProperty(ref _sourceChecked, value);
                SourceList = _baleArchivesModel.GetSqlSourceList(SelectTableValue);
            }
        }

        private int _SelectedBaleIndex;
        public int SelectedBaleIndex
        {
            get { return _SelectedBaleIndex; }
            set
            {
                if (value > -1) BLotDataexcist = true;
                SetProperty(ref _SelectedBaleIndex, value);
            }
        }

        private bool _bLotDataexcist = false;
        public bool BLotDataexcist
        {
            get { return _bLotDataexcist; }
            set
            {
                SetProperty(ref _bLotDataexcist, value);
            }
        }

        private bool _bUp = false;
        public bool BUp
        {
            get { return _bUp; }
            set { SetProperty(ref _bUp, value); }
        }

        private bool _bDown = false;
        public bool BDown
        {
            get { return _bDown; }
            set { SetProperty(ref _bDown, value); }
        }

        private int _pagetable;
        public int PageTable
        {
            get { return _pagetable; }
            set { SetProperty(ref _pagetable, value); }
        }

        private int _totalcount;
        public int Totalcount
        {
            get { return _totalcount; }
            set { SetProperty(ref _totalcount, value); }
        }
        private ObservableCollection<string> _pagecount;
        public ObservableCollection<string> PageCount
        {
            get { return _pagecount; }
            set { SetProperty(ref _pagecount, value); }
        }

        private DelegateCommand _applyCommand;
        public DelegateCommand ApplyCommand =>
       _applyCommand ?? (_applyCommand =
           new DelegateCommand(ApplyCommandExecute, ApplyCommandCanExecute)
            .ObservesProperty(() => BDataExcist).ObservesProperty(() => LDataExcist));
        private bool ApplyCommandCanExecute()
        {
            return BDataExcist | LDataExcist;
        }
        private void ApplyCommandExecute()
        {
            int PageDevider = 20;

            if (SelectBaleTab)
            {
                DataTable TempTable = GetArchivesDataTable(SelectTableValue);

                if (TempTable.Rows.Count > 0)
                {

                    if (BlockChecked)
                    {
                        CmbPagesEnable = true;
                        //Split table
                        splittedtables = TempTable.AsEnumerable()
                                        .Select((row, index) => new { row, index })
                                        .GroupBy(x => x.index / PageDevider)  // integer division, the fractional part is truncated
                                        .Select(g => g.Select(x => x.row).CopyToDataTable())
                                        .ToArray();

                        if (splittedtables.Length > 1)
                            PageTable = splittedtables.Length;
                        else
                            PageTable = 1;

                        Totalcount = TempTable.Rows.Count;
                        PageCount.Clear();

                        for (int i = 0; i < PageTable; i++)
                        {
                            PageCount.Add((i + 1).ToString());
                        }

                        PageSelectIdx = 0;
                        if (PageSelectIdx > 0)
                        {
                            ArchiveDataTable = splittedtables[PageSelectIdx];
                        }
                        else
                            ArchiveDataTable = TempTable;
                        StringStatus = string.Empty;
                    }
                    else
                    {
                        ArchiveDataTable = TempTable;
                        _baleArchivesModel.ArchiveTableName = SelectTableValue;
                        QueryOn = true;
                        Totalcount = TempTable.Rows.Count;
                        StringStatus = string.Empty;
                    }
                }
                else StringStatus = "No Bale Archives Record Found!";
            }
            if (SelectLotTab)
            {
                DataTable TempTable = GetLotArchiveDataTable(SelectLotTableValue, (int)ClassCommon.InstanceType.CloseLot);
                BlockChecked = false;

                if (TempTable.Rows.Count > 0)
                {
                    ArchiveDataTable = TempTable;
                    _baleArchivesModel.ArchiveTableName = SelectTableValue;
                }
                else StringStatus = "No Lot Archives Record Found!";
            }
        }

        private void WriteCSVAllCommandExecute()
        {
            _baleArchivesModel.WriteCSVAllExecute(ArchiveDataTable);
        }
        
        private DelegateCommand _btnUpCommand;
        public DelegateCommand BtnUpCommand =>
       _btnUpCommand ?? (_btnUpCommand =
           new DelegateCommand(BtnUpCommandExecute, BtnUpCommandcanxecute)
            .ObservesProperty(() => QueryOn).ObservesProperty(() => BUp));
        private bool BtnUpCommandcanxecute()
        {
            return QueryOn && BUp;
        }
        private void BtnUpCommandExecute()
        {
            if (PageSelectIdx < PageTable - 1)
                PageSelectIdx += 1;
        }

        private DelegateCommand _btnDownCommand;
        public DelegateCommand BtnDownCommand =>
       _btnDownCommand ?? (_btnDownCommand =
           new DelegateCommand(BtnDownCommandExecute, BtnDownCanExecute)
            .ObservesProperty(() => QueryOn).ObservesProperty(() => BDown));
        private bool BtnDownCanExecute()
        {
            return QueryOn && BDown;
        }
        private void BtnDownCommandExecute()
        {
            if (PageSelectIdx > 0)
                PageSelectIdx -= 1;
        }


        private DelegateCommand _appExitCommand;
        public DelegateCommand AppExitCommand =>
        _appExitCommand ?? (_appExitCommand =
          new DelegateCommand(AppExitExecute));
        private void AppExitExecute()
        {
            if (System.Windows.MessageBox.Show("Are you Sure, you want to Exit ?", "Confirmation",
                 MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Window parent = Window.GetWindow(ArchivesView.archivesView);
                parent.Close();
            }
        }


        private int _pageselectidx = 0;
        public int PageSelectIdx
        {
            get { return _pageselectidx; }
            set
            {
                if ((value >= 0) && (value < splittedtables.Length - 1))
                    BUp = true;
                else
                    BUp = false;

                if ((value >= 1) && (value < splittedtables.Length))
                    BDown = true;
                else
                    BDown = false;

                SetProperty(ref _pageselectidx, value);
                if (value > 0)
                    ArchiveDataTable = splittedtables[value];
            }
        }



        // Tab 2 ////////////////////////////////////////////////////////////////////

        //LotTabEnable
        private bool _LotTabEnable;
        public bool LotTabEnable
        {
            get { return _LotTabEnable; }
            set { SetProperty(ref _LotTabEnable, value); }
        }


        private List<string> _lotmonthtableList;
        public List<string> LotMonthTableList
        {
            get { return _lotmonthtableList; }
            set { SetProperty(ref _lotmonthtableList, value); }
        }

        private bool _lotstockChecked;
        public bool LotStockChecked
        {
            get { return _lotstockChecked; }
            set
            {
                SetProperty(ref _lotstockChecked, value);
                StockList = _baleArchivesModel.GetSqlStockList(SelectLotTableValue);
            }
        }

        private List<string> _lotList;
        public List<string> LotList
        {
            get { return _lotList; }
            set
            {
                SetProperty(ref _lotList, value);
            }
        }

        private bool _lotchecked;
        public bool LotChecked
        {
            get { return _lotchecked; }
            set
            {
                SetProperty(ref _lotchecked, value);
                LotList = _baleArchivesModel.GetSqlLotList(SelectLotTableValue);
            }
        }

        private bool _LotGradeChecked;
        public bool LotGradeChecked
        {
            get { return _LotGradeChecked; }
            set
            {
                SetProperty(ref _LotGradeChecked, value);
            }
        }


        private bool _lotlineChecked;
        public bool LotLineChecked
        {
            get { return _lotlineChecked; }
            set
            {
                SetProperty(ref _lotlineChecked, value);
                //LineList = CBaleAchiveModel.GetSqlLineList(SelectLotTableValue);
            }
        }

        private bool _lotsourceChecked;
        public bool LotSourceChecked
        {
            get { return _lotsourceChecked; }
            set
            {
                SetProperty(ref _lotsourceChecked, value);
                //SourceList = CBaleAchiveModel.GetSqlSourceList(SelectLotTableValue);
            }
        }

        private bool _bDataExcist;
        public bool BDataExcist
        {
            get { return _bDataExcist; }
            set { SetProperty(ref _bDataExcist, value); }
        }

        private bool _lDataExcist;
        public bool LDataExcist
        {
            get { return _lDataExcist; }
            set { SetProperty(ref _lDataExcist, value); }
        }

        private string _selectlotTableValue;
        public string SelectLotTableValue
        {
            get { return _selectlotTableValue; }
            set
            {
                SetProperty(ref _selectlotTableValue, value);
                // int iValLength = _selectlotTableValue.Length;

                string strMonth = SelectLotTableValue.Substring(10, 3);
                string strYear = SelectLotTableValue.Substring(13, 2);

                string startDate = "01" + "/" + strMonth + "/" + strYear;

                DateTime.TryParse(startDate, out DateTime dateValue);

                if (SelectTableIndex == 0)
                {
                    StartQueryDate = dateValue;
                    EndQueryDate = DateTime.Now;
                }
                else
                {
                    StartQueryDate = dateValue;
                    EndQueryDate = dateValue;
                }
            }
        }

        private double _iotTabOpa;
        public double LotTabOpa
        {
            get { return _iotTabOpa; }
            set { SetProperty(ref _iotTabOpa, value); }
        }

        private DataTable GetArchivesDataTable(string selectTableValue)
        {
            return _baleArchivesModel.GetBaleArchiveTable(GetBaleArchiveQuery(SelectTableValue));
        }

        private DataTable GetLotArchiveDataTable(string selectTable, int iLotType)
        {
            return _baleArchivesModel.GetCustomLotArchiveTable(iLotType);

           // return _baleArchivesModel.GetLotArchiveTable(GetLotArchiveQuery(selectTable));
        }

        private void GetAndSaveSettings()
        {
            Settings.Default.DayEndTime = "07:00";
            Settings.Default.Save();
        }
      

        public ArchivesViewModel()
        {
            _baleArchivesModel = new BaleArchivesModel();
            GetAndSaveSettings();
            
            ShowGraphCommand = new DelegateCommand(ShowGraphExecute).ObservesCanExecute(() => BLotDataexcist);
            WriteCSVAllCommand = new DelegateCommand(WriteCSVAllCommandExecute).ObservesCanExecute(() => QueryOn);

        
            PageCount = new ObservableCollection<string>();

            Occrlist = new List<string>
            {
                "Latest",
                "Oldest",
                "All"
            };
            SelectOccr = 0;
            SelectLotOccr = 0;

            MonthChecked = true;
            QueryOn = false;
            BlockChecked = false;

            MonthTableList = _baleArchivesModel.GetSqlTableList();
            if(MonthTableList.Count > 0)
            {
                BDataExcist = true;
                SelectTableIndex = 0;
            }
            else BDataExcist = false;

            RecCount = 200;

            LotTabEnable = true;

            if(LotTabEnable)
            {
                LotTabOpa = 1;
                LotMonthTableList = _baleArchivesModel.GetSqlLotTableList();

                if (LotMonthTableList.Count > 0)
                {
                    LDataExcist = true;
                    StringStatus = string.Empty;
                }
                else
                {
                    LDataExcist = false;
                    StringStatus = "NO LOT ARCHIVES";
                }
            }
            else
            {
                LotTabOpa = 0.1;
                LDataExcist = false;
            }       
        }

        private void ShowGraphExecute()
        {
            string LotIdString = string.Empty;
            DateTime Opendate = DateTime.Today;
            DateTime Closedate = DateTime.Today;
            string StrItem;

            int iCloseHour = 0;

            char[] separators = { ':' };

            string[] words = Settings.Default.DayEndTime.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1) iCloseHour = Convert.ToInt32(words[0]);

            if (ArchiveDataTable.Rows.Count > 0)
            {
                if (SelectedBaleIndex == -1)

                    MessageBox.Show("Please Select lot Number to display graph !");
                else
                {
                    if (ArchiveDataTable.Rows[SelectedBaleIndex]["LotNum"] != null)
                        StrItem = ArchiveDataTable.Rows[SelectedBaleIndex]["LotNum"].ToString();

                    if (ArchiveDataTable.Rows[SelectedBaleIndex]["OpenTD"] != null)
                        Opendate = ArchiveDataTable.Rows[SelectedBaleIndex].Field<DateTime>("OpenTD");

                    if (ArchiveDataTable.Rows[SelectedBaleIndex]["CloseTD"] != null)
                        Closedate = ArchiveDataTable.Rows[SelectedBaleIndex].Field<DateTime>("CloseTD");

                    if (ArchiveDataTable.Rows[SelectedBaleIndex]["FC_IdentString"] != null)
                        LotIdString = ArchiveDataTable.Rows[SelectedBaleIndex].Field<String>("FC_IdentString");

                    if (Opendate.Hour < iCloseHour)
                        Opendate = Opendate.AddDays(1);

                    if (Closedate.Hour < iCloseHour)
                        Closedate = Closedate.AddDays(1);

                    //Show Graph
                    //BaleGraph = new Graph01(StrItem, Opendate, Closedate, LotIdString, SelectedMonth);
                   // BaleGraph.ShowDialog();
                }

            }

        }

        private string GetBaleArchiveQuery(string strTable)
        {
            string strClause;
            string strQuantity;
            string strQueryFields;
            string strOrder;
            string strTimeFrame;
            string strsgls = string.Empty;

            List<string> strBoxes = new List<string>();
            ObservableCollection<string> AllItemsList = _baleArchivesModel.GetAllItemsListModel();

            // 1 Occurrences 
            if (EventValue == "All")
                strQuantity = "SELECT ";
            else
                strQuantity = "SELECT TOP " + RecCount;

            if (AllItemsList.Count > 0)
            {
                string AllList = string.Empty;

                foreach (var item in AllItemsList)
                {
                    AllList = AllList + item + ",";
                }
                AllList = AllList.Remove(AllList.Length - 1);

                strQueryFields = AllList + " ";
            }
            else
                strQueryFields = " * ";


            if (strBoxes.Count > 0)
            {
                foreach (var item in strBoxes)
                {
                    strsgls = strsgls + item + " and ";
                }
                strsgls = " WHERE " + strsgls;
                strsgls = strsgls.Remove(strsgls.Length - 5);
            }

            //4 Old new or all
            if (EventValue == "Oldest")
                strOrder = " ORDER BY [UID] ASC;";
            else
                strOrder = " ORDER BY [UID] DESC;";

            if (DayChecked)
            {
                if (EndQueryDate > DateTime.Now)
                    EndQueryDate = DateTime.Now;

                string strStartDate = StartQueryDate.Value.Date.ToString("MM/dd/yyyy");
                string strEndDate = EndQueryDate.Value.Date.AddDays(1).ToString("MM/dd/yyyy");

                if (strBoxes.Count > 0)
                {
                    strTimeFrame = " AND CAST(TimeComplete AS DATETIME) BETWEEN '" + strStartDate + "' and '" + strEndDate + "' ";
                }
                else
                    strTimeFrame = " WHERE CAST(TimeComplete AS DATETIME) BETWEEN '" + strStartDate + "' and '" + strEndDate + "' ";
            }
            else
                strTimeFrame = string.Empty;

            strClause = strQuantity + " " + strQueryFields + " FROM " + strTable + " with (NOLOCK) " + strsgls + strTimeFrame + strOrder;

            // Console.WriteLine("strClause= " + strClause);

            return strClause;
        }

        private string GetLotArchiveQuery(string selectTable)
        {
            return "SELECT * FROM " + selectTable;
        }
    }
}
