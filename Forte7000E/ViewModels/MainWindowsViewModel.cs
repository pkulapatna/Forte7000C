using ClsErrorLog;
using Forte7000C.Properties;
using Forte7000C.Views;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing.Printing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ClsProducts;
using Forte7000C.Models;
using Prism.Events;
using Forte7000E.Module.LotProcess.Views;
using Forte7000E.Services;
using Forte7000E.Module.SerialCom.Views;
using System.Threading.Tasks;
using Forte7000E.Module.FieldSelect.Views;

namespace Forte7000C.Viewmodels
{
    public class MainWindowsViewModel : BindableBase
    {
        protected readonly IEventAggregator _eventAggregator;

        private LoadingWIndow LoadUpdate;
    
        private readonly ClsProcessModel ProcessModel;
        private ClsLogin.ClsLogin MyLogin;
        private ClsStocks MyStock;

        private Window SharedFileWindow;
        private Window PrintBaleWindow;
        private Window SummaryTableWindow;

        public UserControl LotProcessView
        {
            get { return new LotProcessView(_eventAggregator); }
            set { }
        }

        public UserControl LotConfigView
        {
            get { return new LotConfigView(_eventAggregator); }
            set { }
        }

        public UserControl ScaleView
        {
            get { return new ScaleView(_eventAggregator); }
            set { }
        }

        public UserControl OscillatorView
        {
            get { return new OscillatorView(_eventAggregator); }
            set { }
        }

        public UserControl SerialOneView
        {
            get { return new SerialOneView(_eventAggregator); }
            set { }
        }

        public UserControl LotSummary
        {
            get { return new LotSummary(_eventAggregator); }
            set { }
        }

        private bool _SelectBaletab;
        public bool SelectBaletab
        {
            get => _SelectBaletab; 
            set => SetProperty(ref _SelectBaletab, value); 
        }


        private int _nextLotNumber;
        public int NextLotNumber
        {
            get { return _nextLotNumber; }
            set { SetProperty(ref _nextLotNumber, value); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private bool _LpEnable; 
        public bool LpEnable
        {
            get => _LpEnable; 
            set => SetProperty(ref _LpEnable, value);
        }

        private WindowState _curWindowState;
        public WindowState CurWindowState
        {
            get => _curWindowState; 
            set => SetProperty(ref _curWindowState, value);
        }
        private bool _rbNormScr;
        public bool RbNormScr
        {
            get => _rbNormScr;
            set 
            { 
                SetProperty(ref _rbNormScr, value);
                if (value)
                {
                    CurWindowState = WindowState.Normal;
                    Settings.Default.SCRFull = false;
                    Settings.Default.Save();
                }
            }
        }

        private bool _rbFullScr;
        public bool RbFullScr
        {
            get => _rbFullScr;
            set 
            { 
                SetProperty(ref _rbFullScr, value);
                if(value)
                {
                    CurWindowState = WindowState.Maximized;
                    Settings.Default.SCRFull = true;
                    Settings.Default.Save();
                }
            }
        }


        private DelegateCommand _loadedPageICommand;
        public DelegateCommand LoadedPageICommand =>
        _loadedPageICommand ?? (_loadedPageICommand = new DelegateCommand(LoadPageExecute));
        private void LoadPageExecute()
        {
            if (Settings.Default.SCRFull)
            {
                RbFullScr = true;
            }
            else
            {
                RbNormScr = true;
            }

            ReportArray.SetValue(true, 3);

            LastShutdown = Settings.Default.LastShutDown;
            BPrintDayEnd = false;
            SerialOutOne = new ObservableCollection<DataOutput>();

            //Product tap
            UpdateProdTab();
         
            //Calibration tab
            // CalibrationTable = ProcessModel.GetAccessCalibreationsTable();
            CalibrationTable = ProcessModel.GetSqlCalibreationsTable();
            CalibrationList = GetCalibrationLists();
            LoadUpdate = new LoadingWIndow();

            ShowExpand = Visibility.Hidden;

            if (Settings.Default.bScaleInside)
                UpDateScreenInstruction(ClsUiMsg.ReadUpCount); 
            else
                UpDateScreenInstruction(ClsUiMsg.ReadScaleUpCount); 

            //Setup tap display properties
            SetupSystemsTab();
            SetupSNResetType();

            //Main Tap summary table settings
            Hdrtable = ProcessModel.HdrTable;
            SelectedHdrList = new ObservableCollection<string>();
            SelectedHdrList = ProcessModel.GetXmlSelectedHdrCheckedList();
           
            LvDatatable = ProcessModel.UpdateSummaryDataGrid(_BaleSumDepth);

            ProdSelectedIndex = Settings.Default.ProdSelectIndex; //Last selection
            SelectData = 0;     //Newest data on the top    

            //Output tap
            if (Settings.Default.SharedFileChecked) UpdateOutput(1);
            if (Settings.Default.BalePrintChecked) UpdateOutput(2);

            PrinterSettings settings = new PrinterSettings();
            SelectPrinter = settings.PrinterName;
            ProcessModel.UpdateSerialNumber();
            NewSerialNum = ClassCommon.SerialNumber.ToString();
            NextLotNumber = ProcessModel.GetNextLotNumber();

            ChangeState(ClassCommon.ProcessState.Idle);
        }

        private DelegateCommand _modSummaryTableCommand;
        public DelegateCommand ModSummaryTableCommand =>
       _modSummaryTableCommand ?? (_modSummaryTableCommand = new DelegateCommand(ModSummaryTableCommandExecute));
        private void ModSummaryTableCommandExecute()
        {
            SummaryTableWindow = new Window
            {
                Title = "Data Table Field Selections",
                Width = 970,
                Height = 440,
                Topmost = true,
                Content = new FieldSelect(_eventAggregator,(int)ClassCommon.InstanceType.Summary)
            };
            SummaryTableWindow.ResizeMode = ResizeMode.NoResize;
            SummaryTableWindow.ShowDialog();
        }

        private DelegateCommand _showAboutCommand;
        public DelegateCommand ShowAboutCommand =>
       _showAboutCommand ?? (_showAboutCommand = new DelegateCommand(ShowAboutCommandExecute));
        private void ShowAboutCommandExecute()
        {
            AboutBox aboutBox = new AboutBox()
            {
                Topmost = true,
            };
            aboutBox.ShowDialog();
        }

        private DelegateCommand _closedPageICommand;
        public DelegateCommand ClosedPageICommand =>
       _closedPageICommand ?? (_closedPageICommand = new DelegateCommand(ClosedPageExecute));
        private void ClosedPageExecute()
        {
            ProcessModel.KillAllTimers(_LpEnable);
        }

        /// <summary>
        /// button Initialize
        /// </summary>
        private DelegateCommand _initializeCommand;
        public DelegateCommand InitializeCommand =>
       _initializeCommand ?? (_initializeCommand = 
            new DelegateCommand(InitializeExecute, InitializeCanExecute).ObservesProperty(()
                => BReadReady).ObservesProperty(() => BScaleOnLine));
        private bool InitializeCanExecute()
        {
            return !BReadReady & BScaleOnLine;
        }
        private void InitializeExecute()
        {
            string strstat = string.Empty;

            if (Settings.Default.OscMode == ClassCommon.DevicesMode[ClassCommon.OnLine] 
                | Settings.Default.OscMode == ClassCommon.DevicesMode[ClassCommon.Simulation])
            {
                BActive = true;
                ProcessModel.ChangeState(ClsProcessModel.Events.ReadUpCount);
                ChangeState(ClassCommon.ProcessState.Empty_wait);
            }
            else if (Settings.Default.OscMode == ClassCommon.DevicesMode[ClassCommon.OffLine])
            {
                strstat = "OSC is OffLIne, Check Connections";
            }
            StringStatus = strstat;
        }


        /// <summary>
        /// button Read
        /// </summary>
        private DelegateCommand _readCommand;
        public DelegateCommand ReadCommand =>
        _readCommand ?? (_readCommand =
           new DelegateCommand(ReadExecute, ReadCanExecute).ObservesProperty(() 
               => BReadReady).ObservesProperty(() => BReading));
        private void ReadExecute()
        {
            LoadUpdate.Show();
            BReading = true;
            ProcessModel.ChangeState(ClsProcessModel.Events.ReadDowncount);
        }
        private bool ReadCanExecute()
        {
            return BReadReady & !BReading;
        }

        private DelegateCommand _reSetCommand;
        public DelegateCommand ReSetCommand =>
        _reSetCommand ?? (_reSetCommand =
          new DelegateCommand(ReSetExecute).ObservesCanExecute(() => BActive));
        private void ReSetExecute()
        {
           //
        }

        private DelegateCommand _appExitCommand;
        public DelegateCommand AppExitCommand =>
        _appExitCommand ?? (_appExitCommand =
          new DelegateCommand(AppExitExecute, AppExitCanExecute).ObservesProperty(() => BActive));
        private bool AppExitCanExecute()
        {
            return !BActive;
        }
        private void AppExitExecute()
        {
            if (System.Windows.MessageBox.Show("Are you Sure, you want to Exit ?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Settings.Default.LastShutDown = DateTime.Now;
                Settings.Default.Save();
                Application.Current.Shutdown();
            }
        }

        private DelegateCommand _modifyProdCommand;
        public DelegateCommand ModifyProdCommand =>
        _modifyProdCommand ?? (_modifyProdCommand =
         new DelegateCommand(ModifyProdExecute, ModifyProdCanExecute).ObservesProperty(() => BProCalMod));
        private bool ModifyProdCanExecute()
        {
            return !BProCalMod;
        }
        private void ModifyProdExecute()
        {
            MyStock = new ClsStocks();
            MyStock.ProcessCompleted += MyStock_ProcessCompleted;
            MyStock.ShowStockWindow();
        }

        private DelegateCommand _SerialResetCommand;
        public DelegateCommand SerialResetCommand =>
        _SerialResetCommand ?? (_SerialResetCommand =
            new DelegateCommand(SerialResetCommandExecute));
        private void SerialResetCommandExecute()
        {
            ClassCommon.BSnReset = true;
            //  NewSerialNum = "1";
        }

        #region Setup Outputs tab buttons command

        private DelegateCommand _SharefileConfigCommand;
        public DelegateCommand SharefileConfigCommand =>
        _SharefileConfigCommand ?? (_SharefileConfigCommand =
            new DelegateCommand(SharefileConfigCommandExecute).ObservesCanExecute(() => SharedFileChecked));
        private void SharefileConfigCommandExecute()
        {
            SharedFileWindow = new Window
            {
                Title = "Shared Files",
                Width = 900,
                Height = 420,
                Content = new Forte700E.Module.ItemSelect.Views.ItemSelectView(ClassCommon.outSharedFile, _eventAggregator)
            };
            SharedFileWindow.ResizeMode = ResizeMode.NoResize;
            SharedFileWindow.ShowDialog();
        }

        private DelegateCommand _WriteSharedFileCommand;
        public DelegateCommand WriteSharedFileCommand =>
        _WriteSharedFileCommand ?? (_WriteSharedFileCommand =
            new DelegateCommand(WriteSharedFileCommandExecute).ObservesCanExecute(() => SharedFileChecked));
        private void WriteSharedFileCommandExecute()
        {
            string stringSharedOut = ProcessModel.GetDataFromXmlfile(ClassCommon.outSharedFile);

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(Settings.Default.SharedFileLocation, Settings.Default.SharedFileName)))
            {
                outputFile.WriteAsync(stringSharedOut);
            }
            Outputstatus = "Shared File Output  [" + ProcessModel.GetDataFromXmlfile(ClassCommon.outSharedFile) + "]";
        }

        private DelegateCommand _BrowseLocCommand;
        public DelegateCommand BrowseLocCommand =>
        _BrowseLocCommand ?? (_BrowseLocCommand =
            new DelegateCommand(BrowseLocCommandExecute).ObservesCanExecute(() => SharedFileChecked));
        private void BrowseLocCommandExecute()
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SharedFilePath = dlg.SelectedPath;
                }
                dlg = null;
                FindCreateDir(SharedFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in BrowseExecute " + ex);
            }
        }

        private DelegateCommand _SaveSharedFileCommand;
        public DelegateCommand SaveSharedFileCommand =>
        _SaveSharedFileCommand ?? (_SaveSharedFileCommand =
            new DelegateCommand(SaveSharedFileCommandExecute).ObservesCanExecute(() => SharedFileChecked));
        private void SaveSharedFileCommandExecute()
        {
            Settings.Default.SharedFileLocation = SharedFilePath;
            Settings.Default.Save();
            ShareFileOutString = ProcessModel.GetXmlfile(1);
        }

        #endregion


        #region Setup Time Event and printing buttons command

        //----------------------Reports -----------------------------------------

        private DelegateCommand _PreviewShiftRepCommand;
        public DelegateCommand PreviewShiftRepCommand =>
        _PreviewShiftRepCommand ?? (_PreviewShiftRepCommand =
            new DelegateCommand(PreviewShiftRepCommandExecute, PreviewShiftRepCommandCanExecute).ObservesProperty(() => ShiftRepCheck));
        private bool PreviewShiftRepCommandCanExecute()
        {
            return true;
        }
        private void PreviewShiftRepCommandExecute()
        {
            if (!ProcessModel.ShowReportDialog(SelectedMode))
                MessageBox.Show("No Data found to Show!");
        }

        private DelegateCommand _PrintShiftRepCommand;
        public DelegateCommand PrintShiftRepCommand =>
        _PrintShiftRepCommand ?? (_PrintShiftRepCommand =
            new DelegateCommand(PrintShiftReportExecute).ObservesCanExecute(() => ShiftRepCheck));
        private void PrintShiftReportExecute()
        {
            if (!ProcessModel.PrintSelectReport(SelectedMode))
                MessageBox.Show("No Data found to print!");
        }

        private DelegateCommand _PreviewDayRepCommand;
        public DelegateCommand PreviewDayRepCommand =>
        _PreviewDayRepCommand ?? (_PreviewDayRepCommand =
            new DelegateCommand(PreviewDayRepCommandExecute).ObservesCanExecute(() => DayEndRepCheck));
        private void PreviewDayRepCommandExecute()
        {
            if (!ProcessModel.ShowReportDialog((int)ClassCommon.PrntEvents.DayEnd))
                MessageBox.Show("No Day Data found to Preview!");
        }

        private DelegateCommand _PrintDayRepCommand;
        public DelegateCommand PrintDayRepCommand =>
        _PrintDayRepCommand ?? (_PrintDayRepCommand =
            new DelegateCommand(PrintDayRepCommandExecute).ObservesCanExecute(() => DayEndRepCheck));
        private void PrintDayRepCommandExecute()
        {
            if (!ProcessModel.PrintSelectReport((int)ClassCommon.PrntEvents.DayEnd))
                MessageBox.Show("No Day Data found to print!");
        }

        private DelegateCommand _PreviewMonthRepCommand;
        public DelegateCommand PreviewMonthRepCommand =>
        _PreviewMonthRepCommand ?? (_PreviewMonthRepCommand =
            new DelegateCommand(PreviewMonthRepCommandExecute).ObservesCanExecute(() => MonthEndRepCheck));
        private void PreviewMonthRepCommandExecute()
        {
            if (!ProcessModel.ShowReportDialog((int)ClassCommon.PrntEvents.MonthEnd))
                MessageBox.Show("No Month Data found to Preview!");
        }

        private DelegateCommand _PrintMonthRepCommand;
        public DelegateCommand PrintMonthRepCommand =>
        _PrintMonthRepCommand ?? (_PrintMonthRepCommand =
            new DelegateCommand(PrintMonthRepCommandExecute).ObservesCanExecute(() => MonthEndRepCheck));
        private void PrintMonthRepCommandExecute()
        {
            if (!ProcessModel.PrintSelectReport((int)ClassCommon.PrntEvents.MonthEnd))
                MessageBox.Show("No Month Data found to print!");
        }


        private DelegateCommand _PrintBaleConfigCommand;
        public DelegateCommand PrintBaleConfigCommand =>
        _PrintBaleConfigCommand ?? (_PrintBaleConfigCommand =
            new DelegateCommand(PrintBaleConfigCommandExecute).ObservesCanExecute(() => BalePrintChecked));
        private void PrintBaleConfigCommandExecute()
        {
            PrintBaleWindow = new Window
            {
                Title = "Data Field Selections for printing",
                Width = 900,
                Height = 420,
                Content = new Forte700E.Module.ItemSelect.Views.ItemSelectView(ClassCommon.outBalePrint, _eventAggregator)
            };
            PrintBaleWindow.ResizeMode = ResizeMode.NoResize;
            PrintBaleWindow.ShowDialog();
        }

        private DelegateCommand _PrintBaleCommand;
        public DelegateCommand PrintBaleCommand =>
        _PrintBaleCommand ?? (_PrintBaleCommand = 
            new DelegateCommand(PrintBaleCommandExecute).ObservesCanExecute(() => BalePrintChecked));
        private void PrintBaleCommandExecute()
        {
            int indx = _lvDatatable.Rows[SelectData].Field<int>("serialnumber");
            ProcessModel.PrintBale(indx);
        }

        private DelegateCommand _PrinterDlgCommand;
        public DelegateCommand PrinterDlgCommand =>
        _PrinterDlgCommand ?? (_PrinterDlgCommand = 
            new DelegateCommand(PrinterDlgCommandExecute, PrinterDlgCommandCanExecute));
        private bool PrinterDlgCommandCanExecute()
        {
            return (_DayEndRepCheck | _MonthEndRepCheck | _ShiftRepCheck  | _BalePrintChecked);
        }
        private void PrinterDlgCommandExecute()
        {
            PrintDialog pDialog = new PrintDialog();
            pDialog.ShowDialog();
        }

        //-----------------------------------------------------
        #endregion Report and printing


        #region Menu options command

        private DelegateCommand _LogonCommand;
        public DelegateCommand LogonCommand =>
        _LogonCommand ?? (_LogonCommand =
            new DelegateCommand(LogonExecute).ObservesCanExecute(() => BLogout));
        private void LogonExecute()
        {
            MyLogin = new ClsLogin.ClsLogin();
            MyLogin.EnLogin += MyLogin_EnLogin;
            MyLogin.ShowLoginWindow();
        }

        private DelegateCommand _LogoutCommand;
        public DelegateCommand LogoutCommand =>
        _LogoutCommand ?? (_LogoutCommand =
            new DelegateCommand(LogoutExecute).ObservesCanExecute(() => BLogin));
        private void LogoutExecute()
        {
            BLogin = false;
        }

        private DelegateCommand _ViewLogCommand;
        public DelegateCommand ViewLogCommand =>
        _ViewLogCommand ?? (_ViewLogCommand =
            new DelegateCommand(ViewLogExecute, ViewLogCanExecute));
        private bool ViewLogCanExecute()
        {
            return true;
        }
        private void ViewLogExecute()
        { 
            ClassCommon.MyInfoLog.ShowErrorLog();
        }

        private DelegateCommand _ViewArchivesCommand;
        public DelegateCommand ViewArchivesCommand =>
        _ViewArchivesCommand ?? (_ViewArchivesCommand =
            new DelegateCommand(ViewArchivesCommandExecute));
        private void ViewArchivesCommandExecute()
        {
            Window ArchiveWindow = new Window()
            {
                Title = "Data Archives",
                Width = 1200,
                Height = 800,
                Topmost = true,
                Content = new Forte7000E.Module.Archive.Views.ArchivesView()
            };

            ArchiveWindow.WindowStyle = WindowStyle.None;
            ArchiveWindow.ResizeMode = ResizeMode.NoResize;
            ArchiveWindow.ShowDialog();
        }
        #endregion Menu options command


        //Bale Summary Depth
        private int _BaleSumDepth = Settings.Default.BaleSumDepth;
        public int BaleSumDepth
        {
            get { return _BaleSumDepth; }
            set 
            { 
                if (value >= 1 && value <= 300)
                {
                    SetProperty(ref _BaleSumDepth, value);
                    Settings.Default.BaleSumDepth = value;
                }
                else if (value > 300)
                {
                    SetProperty(ref _BaleSumDepth, 300);
                    Settings.Default.BaleSumDepth = 300;
                }
                else if (value < 1)
                {
                    SetProperty(ref _BaleSumDepth, 20);
                    Settings.Default.BaleSumDepth = 20;
                }
                Settings.Default.Save();
            }
        }
        private bool _SelectTypeTab;
        public bool SelectTypeTab
        {
            get { return _SelectTypeTab; }
            set 
            { 
                SetProperty(ref _SelectTypeTab, value);
                if (value) LoadTypeTab();
            }
        }

        private string _SelectPrinter;
        public string SelectPrinter
        {
            get { return _SelectPrinter; }
            set { SetProperty(ref _SelectPrinter, value); }
        }

        private bool[] _ReportArray = new bool[] { false, false, true, false, false };
        public bool[] ReportArray
        {
            get { return _ReportArray; }
            set { SetProperty(ref _ReportArray, value); }
        }
        public int SelectedMode
        {
            get { return Array.IndexOf(_ReportArray, true); }
        }

        public string _WtUnit;
        public string WtUnit
        {
            get { return _WtUnit; }
            set 
            { 
                SetProperty(ref _WtUnit, value);
                ClassCommon.WtUnit = value;
            }
        }

      
        private bool _bTableMod =  false;
        public bool BTableMod 
        { 
            get { return _bTableMod; }
            set { SetProperty(ref _bTableMod, value); }
        }

        private bool _bSnAll = Settings.Default.bSnAll;
        public bool BSnAll
        {
            get { return _bSnAll; }
            set 
            { 
                SetProperty(ref _bSnAll, value);
                Settings.Default.bSnAll = value;
                Settings.Default.Save();
            }
        }

        private bool _SharedFileChecked = Settings.Default.SharedFileChecked;
        public bool SharedFileChecked
        {
            get { return _SharedFileChecked; }
            set 
            {
                SetProperty(ref _SharedFileChecked, value);
                Settings.Default.SharedFileChecked = value;
                Settings.Default.Save();
                if (value) UpdateOutput(1);
            }
        }
        private bool _BalePrintChecked = Settings.Default.BalePrintChecked;
        public bool BalePrintChecked
        {
            get { return _BalePrintChecked; }
            set
            {
                SetProperty(ref _BalePrintChecked, value);
                Settings.Default.BalePrintChecked = value;
                Settings.Default.Save();

                if (value) UpdateOutput(2);
            }
        }

        private bool _printingon = Settings.Default.bDayEndRepCheck;
        public bool PrintingOn
        {
            get { return _printingon; }
            set { SetProperty(ref _printingon, value); }
        }

        private bool _bSnPt;
        public bool BSnPt
        {
            get { return _bSnPt; }
            set { SetProperty(ref _bSnPt, value); }
        }

        //For Serial output data and format
        private ObservableCollection<DataOutput> _SerialOutOne;
        public ObservableCollection<DataOutput> SerialOutOne
        {
            get { return _SerialOutOne; }
            set { SetProperty(ref _SerialOutOne, value); }
        }


        private DataTable _lvDatatable;
        public DataTable LvDatatable
        {
            get => _lvDatatable;
            set {SetProperty(ref _lvDatatable, value);} 
        }

        private ObservableCollection<DataTable> _summarytable;
        public ObservableCollection<DataTable> SummaryTable
        {
            get => _summarytable;
            set => SetProperty(ref _summarytable, value);
        }

        private ListView _lvData;
        public ListView LvData
        {
            get { return _lvData; }
            set { SetProperty(ref _lvData, value); }
        }

        private DataTable _ProductDefTable;
        public DataTable ProductDefTable
        {
            get { return _ProductDefTable; }
            set { SetProperty(ref _ProductDefTable, value); }
        }

        private DataTable _CalibrationTable;
        public DataTable CalibrationTable
        {
            get { return _CalibrationTable; }
            set { SetProperty(ref _CalibrationTable, value); }
        }

        private string _BigMoistLb;
        public string BigMoistLb
        {
            get { return _BigMoistLb; }
            set { SetProperty(ref _BigMoistLb, value); }
        }

        private string _BigWeighLb;
        public string BigWeightLb
        {
            get { return _BigWeighLb; }
            set { SetProperty(ref _BigWeighLb, value); }
        }

        private string _WeighLb;
        public string WeightLb
        {
            get { return _WeighLb; }
            set { SetProperty(ref _WeighLb, value);}
        }

        private string _TareWeight;
        public string TareWeight
        {
            get { return _TareWeight; }
            set { SetProperty(ref _TareWeight, value); }
        }

        private int _SelectData;
        public int SelectData
        {
            get { return _SelectData; }
            set
            {
                SetProperty(ref _SelectData, value);
                if(value > - 1)
                {
                    if (_lvDatatable.Rows.Count > 0)
                    {
                        WeightDat = _lvDatatable.Rows[value].Field<Single>("Weight").ToString("#0.00");
                        MoistureDat = _lvDatatable.Rows[value].Field<Single>("Moisture").ToString("#0.00");
                    }
                }
            }
        }

        private string _WeightDat;
        public string WeightDat
        {
            get { return _WeightDat; }
            set { SetProperty(ref _WeightDat, value); }
        }

        private string _MoistureDat;
        public string MoistureDat
        {
            get { return _MoistureDat; }
            set { SetProperty(ref _MoistureDat, value); }
        }

        private string _MoistureUnit;
        public string MoistureUnit
        {
            get { return _MoistureUnit; }
            set { SetProperty(ref _MoistureUnit, value); }
        }

        private bool _bActive = false;
        public bool BActive
        {
            get { return _bActive; }
            set
            {
                SetProperty(ref _bActive, value);
                if (value)
                    BIdle = false;
                else
                    BIdle = true;
            }
        }
        private bool _bIdle = true;
        public bool BIdle
        {
            get { return _bIdle; }
            set { SetProperty(ref _bIdle, value); }
        }

        private bool _bLogin = false;
        public bool BLogin
        {
            get { return _bLogin; }
            set 
            { 
                SetProperty(ref _bLogin, value);
                if (value) MainTabWidth = 100;
                else MainTabWidth = 30;
                MainWindow.AppWindows.MainTab.SelectedIndex = 0;
                BLogout = !value;
                ClassCommon.BUserLogon = value;
                _eventAggregator.GetEvent<UserLoginEvent>().Publish(value);
            }
        }

        private bool _bLogout = true;
        public bool BLogout
        {
            get { return _bLogout; }
            set { SetProperty(ref _bLogout, value); }
        }

        private bool _bReadReady = false;
        public bool BReadReady
        {
            get { return _bReadReady; }
            set
            {
                SetProperty(ref _bReadReady, value);
                if (value)
                {
                    StartOpac = 0.3;
                    ReadOpac = 1.0;
                }
                else
                {
                    StartOpac = 1;
                    ReadOpac = 0.3;
                }
            }
        }

        private bool _bReading = false;
        public bool BReading
        {
            get { return _bReading; }
            set { SetProperty(ref _bReading, value); }
        }

        private double _ReadOpac = 0.3;
        public double ReadOpac
        {
            get { return _ReadOpac; }
            set { SetProperty(ref _ReadOpac, value); }
        }

        private double _ResetOpac = 0.3;
        public double ResetOpac
        {
            get { return _ResetOpac; }
            set { SetProperty(ref _ResetOpac, value); }
        }

        private double _StartOpac = 1.0;
        public double StartOpac
        {
            get { return _StartOpac; }
            set { SetProperty(ref _StartOpac, value); }
        }
        
        //Update status on screen
        private string _strStatus;
        public string StringStatus
        {
            get { return _strStatus; }
            set { SetProperty(ref _strStatus, value); }
        }
      
        // Setup System's tab 

        private bool _mcChecked;
        public bool MCChecked
        {
            get { return _mcChecked; }
            set
            {
                SetProperty(ref _mcChecked, value);
                if (value)
                {
                    MoistureUnit = "MC%";
                    BigMoistLb = "Moisture Content %";
                    ClassCommon.MoistureType = 0;
                    LvDatatable = ProcessModel.UpdateSummaryDataGrid(_BaleSumDepth);
                    SelectedHdrList = ProcessModel.GetXmlSelectedHdrCheckedList();
                    UpdateProdTab();
                }
            }
        }
        private bool _mrChecked;
        public bool MRChecked
        {
            get { return _mrChecked; }
            set
            {
                SetProperty(ref _mrChecked, value);
                if (value)
                {
                    MoistureUnit = "MR%";
                    BigMoistLb = "Moisture Regain %";
                    ClassCommon.MoistureType = 1;
                    LvDatatable = ProcessModel.UpdateSummaryDataGrid(_BaleSumDepth);
                    SelectedHdrList = ProcessModel.GetXmlSelectedHdrCheckedList();
                    UpdateProdTab();
                }
                    
            }
        }
        private bool _adChecked;
        public bool ADChecked
        {
            get { return _adChecked; }
            set
            {
                SetProperty(ref _adChecked, value);
                if (value)
                {
                    MoistureUnit = "AD%";
                    BigMoistLb = "AirDry %";
                    ClassCommon.MoistureType = 2;
                    LvDatatable = ProcessModel.UpdateSummaryDataGrid(_BaleSumDepth);
                    SelectedHdrList = ProcessModel.GetXmlSelectedHdrCheckedList();
                    UpdateProdTab();
                }
                  
            }
        }
        private bool _bdChecked;
        public bool BDChecked
        {
            get { return _bdChecked; }
            set
            {
                SetProperty(ref _bdChecked, value);
                if (value)
                {
                    MoistureUnit = "BD%";
                    BigMoistLb = "BoneDry %";
                    ClassCommon.MoistureType = 3;
                    LvDatatable = ProcessModel.UpdateSummaryDataGrid(_BaleSumDepth);
                    SelectedHdrList = ProcessModel.GetXmlSelectedHdrCheckedList();
                    UpdateProdTab();
                }
            }
        }

        private bool _kgChecked;
        public bool KGChecked
        {
            get { return _kgChecked; }
            set
            {
                SetProperty(ref _kgChecked, value);
                if (value)
                {
                    WtUnit = "Kg.";
                    ClassCommon.WeightType = 0;
                    BigWeightLb = "Weight(kg.)";
                    WeightLb = "Kg.";
                    LvDatatable = ProcessModel.UpdateSummaryDataGrid(_BaleSumDepth);
                    SelectedHdrList = ProcessModel.GetXmlSelectedHdrCheckedList();
                    UpdateProdTab();
                }
            }
        }

        private bool _lbChecked;
        public bool LBChecked
        {
            get { return _lbChecked; }
            set
            {
                SetProperty(ref _lbChecked, value);
                if (value)
                {
                    WtUnit = "Lb.";
                    ClassCommon.WeightType = 1;
                    BigWeightLb = "Weight(Lb.)";
                    WeightLb = "Lb.";
                    LvDatatable = ProcessModel.UpdateSummaryDataGrid(_BaleSumDepth);
                    SelectedHdrList = ProcessModel.GetXmlSelectedHdrCheckedList();
                    UpdateProdTab();

                }
            }
        }

        private bool _SMChecked;
        public bool SMChecked
        {
            get { return _SMChecked; }
            set { SetProperty(ref _SMChecked, value); }
        }
        private bool _DMChecked;
        public bool DMChecked
        {
            get { return _DMChecked; }
            set { SetProperty(ref _DMChecked, value); }
        }

        //SMChecked DMChecked
        // End Setup System's tab

        /// <summary>
        /// Do Machine Events here!
        /// </summary>
        private ClassCommon.ProcessState _Currentstates = ClassCommon.ProcessState.Idle;
        public ClassCommon.ProcessState Currentstates
        {
            get { return _Currentstates; }
            set { SetProperty(ref _Currentstates, value); }
        }

        private ObservableCollection<string> _selectedItemList;
        public ObservableCollection<string> SelectedHdrList
        {
            get { return _selectedItemList; }
            set { SetProperty(ref _selectedItemList, value); }
        }

        private string _ShareFileOutString;
        public string ShareFileOutString
        {
            get { return _ShareFileOutString; }
            set { SetProperty(ref _ShareFileOutString, value); }
        }

        private string _BalePrintOutString;
        public string BalePrintOutString
        {
            get { return _BalePrintOutString; }
            set { SetProperty(ref _BalePrintOutString, value); }
        }

        //------------------------------------------------------------------------------
        private string _SharedFilePath = Settings.Default.SharedFileLocation;
        public string SharedFilePath
        {
            get { return _SharedFilePath; }
            set { SetProperty(ref _SharedFilePath, value); }
        }

        private string _SharedFileName = Settings.Default.SharedFileName;
        public string SharedFileName
        {
            get { return _SharedFileName; }
            set { SetProperty(ref _SharedFileName, value); }
        }

        private string _Outputstatus;
        public string Outputstatus
        {
            get { return _Outputstatus; }
            set { SetProperty(ref _Outputstatus, value); }
        }

        //-----------------------------------------------------------------------------

        private DataTable _hdrtable;
        public DataTable Hdrtable
        {
            get { return _hdrtable; }
            set { SetProperty(ref _hdrtable, value); }
        }

        private int _LVModOpc = 1;
        public int LVModOpc
        {
            get { return _LVModOpc; }
            set { SetProperty(ref _LVModOpc, value); }
        }

        private int _MainTabWidth = 30;
        public int MainTabWidth
        {
            get { return _MainTabWidth; }
            set { SetProperty(ref _MainTabWidth, value); }
        }

        private Visibility _Expand;
        public Visibility Expand
        {
            get { return _Expand; }
            set { SetProperty(ref _Expand, value); }
        }
        private Visibility _Collap = Visibility.Hidden;
        public Visibility Collap 
        {
            get { return _Collap; }
            set { SetProperty(ref _Collap, value); }
        }


        private bool _bLoin = false;
        public bool BLoin
        {
            get { return _bLoin; }
            set { SetProperty(ref _bLoin, value); }
        }
        private Visibility _ShowExpand = Visibility.Hidden;
        public Visibility ShowExpand
        {
            get { return _ShowExpand; }
            set { SetProperty(ref _ShowExpand, value); }
        }


        /// <summary>
        /// Scale ////////////////////////////////////////////////////
        /// </summary>
        private string _DevSCaleMode; // Settings.Default.ScaleMode;
        public string DevSCaleMode
        {
            get => _DevSCaleMode;
            set { SetProperty(ref _DevSCaleMode, value); }
        }

        private bool _bScaleOnLine = false;
        public bool BScaleOnLine
        {
            get { return _bScaleOnLine; }
            set { SetProperty(ref _bScaleOnLine, value); }
        }

        private int _SelScaleModeIndex;
        public int SelScaleModeIndex
        {
            get { return _SelScaleModeIndex; }
            set 
            {
                SetProperty(ref _SelScaleModeIndex, value);
                DevSCaleMode = ClassCommon.DevicesMode[value];

                Settings.Default.ScaleModeIndex = value;
                Settings.Default.Save();
                if (value == 0) BScaleOnLine = true;
                if (value == 1) BScaleOnLine = false;
                if (value == 2) 
                {
                    BScaleOnLine = true;
                    BScaleSim = Visibility.Visible;
                }
                else
                    BScaleSim = Visibility.Hidden;
            }
        }

        private bool _scaleAutoChecked; 
        public bool ScaleAutoChecked
        {
            get => _scaleAutoChecked;
            set 
            {
                SetProperty(ref _scaleAutoChecked, value);
                if (value)
                {
                    ClassCommon.SCaleModeAuto = true;
                    ShowScaleEnter = Visibility.Hidden;
                }      
            }
        }

        private bool _scaleManualChecked;
        public bool ScaleManualChecked
        {
            get => _scaleManualChecked;
            set 
            { 
                SetProperty(ref _scaleManualChecked, value);
                if (value)
                {
                    ClassCommon.SCaleModeAuto = false;
                    ShowScaleEnter = Visibility.Visible;
                }
            }
        }

        private Visibility _showScaleEnter;
        public Visibility ShowScaleEnter
        {
            get { return _showScaleEnter; }
            set { SetProperty(ref _showScaleEnter, value); }
        }

        /// <summary>
        /// OSC ///////////////////////////////////////////////////////
        /// </summary>
        private int _SelOscModeIndex;// = Settings.Default.OscModeIndex;
        public int SelOscModeIndex
        {
            get { return _SelOscModeIndex; }
            set 
            { 
                SetProperty(ref _SelOscModeIndex, value);

                DevOscMode = ClassCommon.DevicesMode[value];
                Settings.Default.OscMode = DevOscMode;
                Settings.Default.OscModeIndex = value;
                Settings.Default.Save();

                if (value == 2)
                {
                    BOscSim = Visibility.Visible;
                }
                else
                    BOscSim = Visibility.Hidden;
            }
        }

        private string _DevOscMode; 
        public string DevOscMode
        {
            get => _DevOscMode;
            set => SetProperty(ref _DevOscMode, value); 
        }

        private Visibility _bScaleSim;
        public Visibility BScaleSim
        {
            get { return _bScaleSim; }
            set { SetProperty(ref _bScaleSim, value); }
        }

        private Visibility _bOscSim;
        public Visibility BOscSim
        {
            get { return _bOscSim; }
            set { SetProperty(ref _bOscSim, value); }
        }

        private long _Serialmax = Settings.Default.iSerialMax;
        public long Serialmax
        {
            get { return _Serialmax; }
            set 
            {  
                SetProperty(ref _Serialmax, value);

                if ((value > 0) & (value < 99999))
                {
                    Settings.Default.iSerialMax = value;
                    Settings.Default.Save();
                }
            }
        }

        //----------------------------------------------------------------
        private bool _SNRolloverCheck;
        public bool SNRolloverCheck
        {
            get { return _SNRolloverCheck; }
            set 
            { 
                SetProperty(ref _SNRolloverCheck, value);
                if(value)
                {
                    Settings.Default.SNResetType = 0;
                    Settings.Default.Save();
                }
            }
        }
        private bool _SNDayEndCheck;
        public bool SNDayEndCheck
        {
            get { return _SNDayEndCheck; }
            set 
            { 
                SetProperty(ref _SNDayEndCheck, value);
                if (value)
                {
                    Settings.Default.SNResetType = 1;
                    Settings.Default.Save();
                }
            }
        }
        private bool _SNMonthEndCheck;
        public bool SNMonthEndCheck
        {
            get { return _SNMonthEndCheck; }
            set 
            { 
                SetProperty(ref _SNMonthEndCheck, value);
                if (value)
                {
                    Settings.Default.SNResetType = 2;
                    Settings.Default.Save();
                }
            }
        }
        //-------------------------------------------------------------------

        private DateTime _DayEndTime = Settings.Default.DayEndTime;
        public DateTime DayEndTime
        {
            get { return _DayEndTime; }
            set 
            { 
                SetProperty(ref _DayEndTime, value);
                if(value != null)
                {
                    SetDayEndTime(value);
                }
            }
        }

        private void SetDayEndTime(DateTime value)
        {
            var d3 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DayEndTime.Hour, DayEndTime.Minute, 00);
            var d4 = d3.AddDays(1);
            Settings.Default.DayEndTime = d4;
            Settings.Default.Save();
            ClassCommon.DayEnd = Settings.Default.DayEndTime;
        }

        private DateTime _ShiftOneTime = Settings.Default.ShiftOneTime;
        public DateTime ShiftOneTime
        {
            get { return _ShiftOneTime; }
            set
            {
                SetProperty(ref _ShiftOneTime, value);
                if (value != null)
                {
                    Settings.Default.ShiftOneTime = value;
                    Settings.Default.Save();
                }
            }
        }
        private DateTime _ShiftTwoTime = Settings.Default.ShiftTwoTime;
        public DateTime ShiftTwoTime
        {
            get { return _ShiftTwoTime; }
            set
            {
                SetProperty(ref _ShiftTwoTime, value);
                if (value != null)
                {
                    Settings.Default.ShiftTwoTime = value;
                    Settings.Default.Save();
                }
            }
        }
        private DateTime _ShiftThreeTime = Settings.Default.ShiftThreeTime;
        public DateTime ShiftThreeTime
        {
            get { return _ShiftThreeTime; }
            set
            {
                SetProperty(ref _ShiftThreeTime, value);
                if (value != null)
                {
                    Settings.Default.ShiftThreeTime = value;
                    Settings.Default.Save();
                }
            }
        }

        //--------------------------------------------------------------------------//
        private bool _bPrintDayEnd = false;
        public bool BPrintDayEnd
        {
            get { return _bPrintDayEnd; }
            set
            {
                SetProperty(ref _bPrintDayEnd, value);
            }
        }

        private bool _bMonthEnd = false;
        public bool BMonthEnd
        {
            get { return _bMonthEnd; }
            set
            {
                SetProperty(ref _bMonthEnd, value);  
            }
        }

        private bool _DayEndRepCheck = Settings.Default.bDayEndRepCheck;
        public bool DayEndRepCheck
        {
            get { return _DayEndRepCheck; }
            set 
            { 
                SetProperty(ref _DayEndRepCheck, value);
                Settings.Default.bDayEndRepCheck = value;
                Settings.Default.Save();
            }
        }

        private bool _MonthEndRepCheck = Settings.Default.bMonthEndRepCheck;
        public bool MonthEndRepCheck
        {
            get { return _MonthEndRepCheck; }
            set
            {
                SetProperty(ref _MonthEndRepCheck, value);
                Settings.Default.bMonthEndRepCheck = value;
                Settings.Default.Save();
            }
        }

        private bool _ShiftRepCheck = Settings.Default.bShiftRepCheck;
        public bool ShiftRepCheck
        {
            get { return _ShiftRepCheck; }
            set
            {
                SetProperty(ref _ShiftRepCheck, value);
                Settings.Default.bShiftRepCheck = value;
                Settings.Default.Save();
                if(value == false)
                {
                    //ShiftOneCheck = false;
                    //ShiftTwoCheck = false;
                    //ShiftThreeCheck = false;
                }
            }
        }

        private bool _shiftOneCheck = Settings.Default.bShiftOneCheck;
        public bool ShiftOneCheck
        {
            get { return _shiftOneCheck; }
            set
            {
                SetProperty(ref _shiftOneCheck, value);
                Settings.Default.bShiftOneCheck = value;
                Settings.Default.Save();
                CheckPrintOption();
            }
        }
        private bool _shiftTwoCheck = Settings.Default.bShiftTwoCheck;
        public bool ShiftTwoCheck
        {
            get { return _shiftTwoCheck; }
            set
            {
                SetProperty(ref _shiftTwoCheck, value);
                Settings.Default.bShiftTwoCheck = value;
                Settings.Default.Save();
                CheckPrintOption();
            }
        }
        private bool _shiftThreeCheck = Settings.Default.bShiftThreeCheck;
        public bool ShiftThreeCheck
        {
            get { return _shiftThreeCheck; }
            set
            {
                SetProperty(ref _shiftThreeCheck, value);
                Settings.Default.bShiftThreeCheck = value;
                Settings.Default.Save();
                CheckPrintOption();
            }
        }

        private void CheckPrintOption()
        {
            if (_shiftOneCheck == false && _shiftTwoCheck == false && _shiftThreeCheck == false)
                ShiftRepCheck = false;
        }

        private string _ShiftOneName = Settings.Default.ShiftOneName;
        public string ShiftOneName
        {
            get { return _ShiftOneName; }
            set
            {
                SetProperty(ref _ShiftOneName, value);
                Settings.Default.ShiftOneName = value;
                Settings.Default.Save();
            }
        }
        private string _ShiftTwoName = Settings.Default.ShiftTwoName;
        public string ShiftTwoName
        {
            get { return _ShiftTwoName; }
            set
            {
                SetProperty(ref _ShiftTwoName, value);
                Settings.Default.ShiftTwoName = value;
                Settings.Default.Save();
            }
        }
        private string _ShiftThreeName = Settings.Default.ShiftThreeName;
        public string ShiftThreeName
        {
            get { return _ShiftThreeName; }
            set
            {
                SetProperty(ref _ShiftThreeName, value);
                Settings.Default.ShiftThreeName = value;
                Settings.Default.Save();
            }
        }

       
        private string _OscPortMsg;
        public string OscPortMsg
        {
            get { return _OscPortMsg; }
            set { SetProperty(ref _OscPortMsg, value); }
        }
        private string _ScalePortMsg;
        public string ScalePortMsg
        {
            get { return _ScalePortMsg; }
            set { SetProperty(ref _ScalePortMsg, value); }
        }


        public MainWindowsViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.APPSTART, "Init MainWindowsViewModel ..........................");

            ProcessModel = new ClsProcessModel(_eventAggregator);
            ProcessModel.ChangeState(ClsProcessModel.Events.idle);

            ClassCommon.BSnReset = false;

            LvData = new ListView();
            LvDatatable = new DataTable();
            
            SummaryTable = new ObservableCollection<DataTable>();

            ProductDefTable = new DataTable();
            CalibrationTable = new DataTable();

            ScaleAutoChecked = true;

            _eventAggregator.GetEvent<SendOscdataEvent>().Subscribe(OscDataFromModel);

            _eventAggregator.GetEvent<SetOscMode>().Subscribe(SetNewOscMode);
            _eventAggregator.GetEvent<SetScaleMode>().Subscribe(SetNewScaleMode);

            _eventAggregator.GetEvent<UpdateStringOutput>().Subscribe(UpdateOutput);
            _eventAggregator.GetEvent<LotProcessEnable>().Subscribe(IsLotEnable);
            _eventAggregator.GetEvent<UpdateSqldataEvent>().Subscribe(UpdateScreenData);

            _eventAggregator.GetEvent<SaveFieldsEvent>().Subscribe(SaveModifyFields);
            _eventAggregator.GetEvent<SaveItemSelectEvent>().Subscribe(SaveItemSelectFields);
            _eventAggregator.GetEvent<ScaleRetryEvent>().Subscribe(ScaleDeviceRetry);

            //Check Scale online?
            SelScaleModeIndex = Settings.Default.ScaleModeIndex;
           
            //Check Osc online?
            SelOscModeIndex = Settings.Default.OscModeIndex;            
        }

        private void ScaleDeviceRetry(int obj)
        {
            StringStatus = "Scale Retry " + obj.ToString();
        }

        private void SaveItemSelectFields(int obj)
        {
            if( ClassCommon.outSharedFile == obj)
            {
                if(SharedFileWindow != null)
                {
                    SharedFileWindow.Close();
                    SharedFileWindow = null;
                    ShareFileOutString = ProcessModel.GetXmlfile(obj);
                }
            }
            if(ClassCommon.outBalePrint == obj)
            {
                if(PrintBaleWindow != null)
                {
                    PrintBaleWindow.Close();
                    PrintBaleWindow = null;
                    BalePrintOutString = ProcessModel.GetXmlfile(obj);
                }
            }
        }

        private void SaveModifyFields(int obj)
        {
            if((int)ClassCommon.InstanceType.Summary == obj)
            {
                if (SummaryTableWindow != null)
                {
                    SummaryTableWindow.Close();
                    SummaryTableWindow = null;
                }
                LvDatatable = ProcessModel.UpdateSummaryDataGrid(_BaleSumDepth);
                SelectedHdrList = ProcessModel.GetXmlSelectedHdrCheckedList();
            }
        }

        private void OscDataFromModel(double obj)
        {
            switch (ProcessModel._events)
            {
                case ClsProcessModel.Events.ReadUpCount:

                  //  ClassCommon.dRefernce = obj;
                    Console.WriteLine("----------- Upcount = " + obj, ToString());
                    UpDateScreenInstruction(ClsUiMsg.ReadScaleDownCount); 
                    BActive = false;
                    BReadReady = true;
                    break;

                case ClsProcessModel.Events.ReadDowncount:

                  //  ClassCommon.dReading = obj;
                    Console.WriteLine("----------- Downcount = " + obj, ToString());
                    UpDateScreenInstruction(ClsUiMsg.ReadUpCount); 
                    break;
            }
        }

        private void UpDateScreenInstruction(string strAction)
        {
            instruction = strAction;
        }

        private void SetNewScaleMode(string obj)
        {
            DevSCaleMode = obj;
        }
        private void SetNewOscMode(string obj)
        {
            DevOscMode = obj;
        }

        private void IsLotEnable(bool obj)
        {
            LpEnable = obj;

            if (!obj) SelectBaletab = true;
        }

        private void UpdateOutput(int obj)
        {
            switch (obj)
            {
                case 0:
                //    SerialOneOutString = string.Empty;
                //    SerialOneOutString = ProcessModel.GetXmlfile(0);
                    break;
                case 1:
                    ShareFileOutString = string.Empty;
                    ShareFileOutString = ProcessModel.GetXmlfile(1);
                    break;
                case 2:
                    BalePrintOutString = string.Empty;
                    BalePrintOutString = ProcessModel.GetXmlfile(2);
                    break;
                case 3:
                   // strWeightReq = string.Empty;
                  //  strWeightReq = ProcessModel.GetXmlfile(3);
                    break;
            }
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in findCreateDir " + ex);
            }
        }

        private string _shiftName;
        public string ShiftName
        {
            get { return _shiftName; }
            set { SetProperty(ref _shiftName, value); }
        }

       
        private string _NewSerialNum;
        public string NewSerialNum
        {
            get { return _NewSerialNum; }
            set
            {
                ClassCommon.SerialNumber = value;
                SetProperty(ref _NewSerialNum, value);
            }
        }

        private void MyStock_ProcessCompleted()
        {
            if(MyStock != null)
            {
                MyStock.CloseStockWindow();
                UpdateProdTab();
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.DBSQL, "Update SQL Stock Table!");
            }
        }

        private async void UpdateScreenData(string obj)
        {
            await SetTaskDelay(1000); //Just to make sure that the sql table is updated before calling!

            try
            {
                LvDatatable = UpdateDataGrid(LvDatatable, ProcessModel.UpdateSummaryDataGrid(_BaleSumDepth));
                MoistureDat = LvDatatable.Rows[0].Field<Single>("Moisture").ToString("#0.00");
                WeightDat = LvDatatable.Rows[0].Field<Single>("Weight").ToString("#0.00");

                UpDateScreenInstruction(ClsUiMsg.ReadUpCount);
             
                NewSerialNum = ProcessModel.GetNextSerialNumber();
                NextLotNumber = ClassCommon.NextLotNumber;

            }
            catch (Exception ex)
            {
                StringStatus = "Error! Data Not transfere to the Archive";
                LoadUpdate.Hide();
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.REPORTS, "Error in UpdateScreenData " + ex.Message);
            }
            finally
            {
                LoadUpdate.Hide();
                StringStatus = "UID# " + obj.ToString() + " Data has been transfered to the Archive";
                BReadReady = false;
                BReading = false;
               
                ChangeState(ClassCommon.ProcessState.Complete);
            }
        }

        private async Task SetTaskDelay(int iMsec)
        {
            await Task.Delay(iMsec); 
        }

        private  DataTable UpdateDataGrid(DataTable oldtable, DataTable newtable)
        {
            DataTable xTable = new DataTable();

            try
            {
                DataRow NewRow = oldtable.NewRow();
                NewRow.ItemArray = newtable.Rows[0].ItemArray;

                oldtable.Rows.InsertAt(NewRow, 0);

                if (oldtable.Rows.Count > BaleSumDepth)
                    oldtable.Rows.RemoveAt(oldtable.Rows.Count - 1);

                xTable = oldtable;

            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in UpdateDataGrid " + ex.Message);
            }
            return xTable;
        }

  
        private void BLoginSuccess(bool obj)
        {
            BLogin = obj;
        }

        private void MyLogin_EnLogin()
        {
            BLoginSuccess(MyLogin.GetLoginStatus());
        }

        public void LoadTypeTab()
        {
            //Product tap
            UpdateProdTab();
        }

        private DateTime _LastShutdown;
        public DateTime LastShutdown
        {
            get { return _LastShutdown; }
            set { SetProperty(ref _LastShutdown, value); }
        }

        private int _CurUidNum;
        public int CurUidNum
        {
            get { return _CurUidNum; }
            set { SetProperty(ref _CurUidNum, value); }
        }

        private int _lPreUidNum;
        public int PreUidNum
        {
            get { return _lPreUidNum; }
            set { SetProperty(ref _lPreUidNum, value); }
        }

        private int _PreIdxNum;
        public int PreIdxNum
        {
            get { return _PreIdxNum; }
            set { SetProperty(ref _PreIdxNum, value); }
        }


        
        private void SetupSystemsTab()
        {
            switch (ClassCommon.MoistureType)
            {
                case 0:
                    MCChecked = true;
                    break;
                case 1:
                    MRChecked = true;
                    break;
                case 2:
                    ADChecked = true;
                    break;
                case 3:
                    BDChecked = true;
                    break;

                default:
                    break;
            }

            if (ClassCommon.WeightType == 0)
            {
                KGChecked = true;
                BigWeightLb = "Weight(kg.)";
                WeightLb = "Kg.";
               
            }
            else
            {
                LBChecked = true;
                BigWeightLb = "Weight(Lb.)";
                WeightLb = "Lb.";
            }
                

            if (Settings.Default.bSingleForte == true)
                SMChecked = true;
            else
                DMChecked = true;

            HotKeyChecked = Settings.Default.bHotKeyChecked;
        }

        private void SetupSNResetType()
        {
            switch (Settings.Default.SNResetType)
            {
                case 0:
                    SNRolloverCheck = true;
                    break;
                case 1:
                    SNDayEndCheck = true;
                    break;
                case 2:
                    SNMonthEndCheck = true;
                    break;

                default:
                    SNRolloverCheck = true;
                    break;
            }
        }

        private List<string> GetCalibrationLists()
        {
            List<string> CalList = new List<string>();
           
            if (CalibrationTable.Rows.Count > 0)
            {
                for (int i = 0; i < CalibrationTable.Rows.Count; i++)
                {
                    CalList.Add(CalibrationTable.Rows[i]["Name"].ToString());
                }
            }
            return CalList;
        }



        private List<string> GetProductList()
        {
            List<string> prodlist = new List<string>();

            if (ProductDefTable.Rows.Count > 0)
            {
                for (int i = 0; i < ProductDefTable.Rows.Count; i++)
                {
                    prodlist.Add(ProductDefTable.Rows[i]["Name"].ToString());
                }
            }
            return prodlist;
        }


        public void ChangeState(ClassCommon.ProcessState strState)
        {
            Currentstates = strState;

            //Console.WriteLine("------------ " + Currentstates);

            switch (Currentstates)
            {
                case ClassCommon.ProcessState.Idle: //0

                    break;
                case ClassCommon.ProcessState.Empty_wait: //1 Empty text cell?

                    break;
                case ClassCommon.ProcessState.Full_wait: //3

                    break;
                case ClassCommon.ProcessState.Complete: //4

                    break;
                case ClassCommon.ProcessState.OscTest_Wait: //5 

                    break;

                case ClassCommon.ProcessState.ScaleTest_Wait: //6

                    break;

                case ClassCommon.ProcessState.DGHTest_Wait: //7

                    break;

                case ClassCommon.ProcessState.DGH_Wait: //8

                    break;

                case ClassCommon.ProcessState.ScaleRead: //9
                    break;

                case ClassCommon.ProcessState.Reference_Complete: //10

                    break;

                case ClassCommon.ProcessState.Read1_Complete: //11
                    break;

                case ClassCommon.ProcessState.ReadOscError: //12
                    break;

                case ClassCommon.ProcessState.Full2_Wait: //13

                    break;
            }
        }



        #region Product ----------------------------------------------------------

        /// <summary>
        /// Product tab /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>

        private void UpdateProdTab()
        {
            ProductTable = ProcessModel.GetDatatable("StockTable"); // ProcessModel.GetAccessStockTable();
            ProductDefTable = ProcessModel.CreateProductDefTable();
            ProductList = GetProductList();

            LbFieldOne = "Text Field 1";
            LbFieldTwo = "Text Field 2";
            LbFieldThree = "Text Field 3";
            LbFieldFour = "Text Field 4";

            StockSelectedIndex = ProdSelectedIndex;
        }

        private int _StockSelectedIndex;
        public int StockSelectedIndex
        {
            get { return _StockSelectedIndex; }
            set
            {
                SetProperty(ref _StockSelectedIndex, value);

                if (value != -1)
                {

                    DataColumnCollection columns = ProductTable.Columns;

                    StockSelectValue = ProductTable.Rows[value]["Name"].ToString();

                    ClassCommon.CurrStock = StockSelectValue;

                    MinProdMoisture = ProcessModel.GetMoistureUnit(ProductTable.Rows[value].Field<Single>("MinMoisture")).ToString("#0.00");
                    MaxProdMoisture = ProcessModel.GetMoistureUnit(ProductTable.Rows[value].Field<Single>("MaxMoisture")).ToString("#0.00");

                    CautionMoistLow = ProcessModel.GetMoistureUnit(ProductTable.Rows[value].Field<Single>("CautionMoistLow")).ToString("#0.00");
                    CautionMoistHigh = ProcessModel.GetMoistureUnit(ProductTable.Rows[value].Field<Single>("CautionMoistHigh")).ToString("#0.00");

                    MaxProdWeight = ProductTable.Rows[value].Field<Single>("MaxWeight").ToString("#0.00");
                    MinProdWeight = ProductTable.Rows[value].Field<Single>("MinWeight").ToString("#0.00");

                    CautionWTLow = ProductTable.Rows[value].Field<Single>("CautionWTLow").ToString("#0.00");
                    CautionWTHigh = ProductTable.Rows[value].Field<Single>("CautionWTHigh").ToString("#0.00");

                    PDWeight = ProductTable.Rows[value].Field<Single>("DefaultWeight").ToString("#0.00");
                    PDNtWeight = ProductTable.Rows[value].Field<Single>("DefaultNetWeight").ToString("#0.00");
                    PDForte = ProductTable.Rows[value].Field<Int16>("DefaultForte").ToString();
                    PDMoisture = ProductTable.Rows[value].Field<Single>("DefaultMoisture").ToString("#0.00");

                    TareWeight = ProductTable.Rows[value].Field<Single>("TareWeight").ToString("#0.00");

                    MUseCaution = ProductTable.Rows[value].Field<Boolean>("MUseCaution");
                    WTUseCaution = ProductTable.Rows[value].Field<Boolean>("WTUseCaution");

                    if (columns.Contains("Label1"))
                        FieldOne = ProductTable.Rows[value]["Label1"].ToString();
                    if (columns.Contains("Label2"))
                        FieldTwo = ProductTable.Rows[value]["Label2"].ToString();
                    if (columns.Contains("Label3"))
                        FieldThree = ProductTable.Rows[value]["Label3"].ToString();
                    if (columns.Contains("Label4"))
                        FieldThree = ProductTable.Rows[value]["Label4"].ToString();

                    var CalIdx = ProductTable.Rows[value]["CalIDLn1"].ToString();

                    for (int i = 0; i < CalibrationTable.Rows.Count; i++)
                    {
                        if (CalibrationTable.Rows[i]["ID"].ToString() == CalIdx)
                        {
                            SelCalValue = CalibrationTable.Rows[i]["Name"].ToString();
                            //CalNameIndex = Convert.ToInt32(CalIdx)-1;
                            CalNameIndex = i;
                        }
                    }
                }
            }
        }

        private string _StockSelectValue;
        public string StockSelectValue
        {
            get { return _StockSelectValue; }
            set { SetProperty(ref _StockSelectValue, value); }
        }

        private bool _bCreateNewStock = false;
        public bool BCreateNewStock
        {
            get { return _bCreateNewStock; }
            set { SetProperty(ref _bCreateNewStock, value); }
        }

        private List<string> _CalibrationList;
        public List<string> CalibrationList
        {
            get { return _CalibrationList; }
            set { SetProperty(ref _CalibrationList, value); }
        }

        private string _SelCalValue;
        public string SelCalValue
        {
            get { return _SelCalValue; }
            set { SetProperty(ref _SelCalValue, value); }
        }

        private int _CalNameIndex = 0;
        public int CalNameIndex
        {
            get { return _CalNameIndex; }
            set
            {
                SetProperty(ref _CalNameIndex, value);

                Aval = CalibrationTable.Rows[value].Field<Double>("A").ToString("#0.000");
                Bval = CalibrationTable.Rows[value].Field<Double>("B").ToString("#0.000");
                Cval = CalibrationTable.Rows[value].Field<Double>("C").ToString("#0.000");
            }
        }

        private bool _bProCalMod = false;
        public bool BProCalMod
        {
            get { return _bProCalMod; }
            set { SetProperty(ref _bProCalMod, value); }
        }

        private string _Aval;
        public string Aval
        {
            get { return _Aval; }
            set { SetProperty(ref _Aval, value); }
        }
        private string _Bval;
        public string Bval
        {
            get { return _Bval; }
            set { SetProperty(ref _Bval, value); }
        }

        private string _Cval;
        public string Cval
        {
            get { return _Cval; }
            set { SetProperty(ref _Cval, value); }
        }

        
        /// <summary>
        /// Product 
        /// </summary>
        private DataTable _ProductTable;
        public DataTable ProductTable
        {
            get { return _ProductTable; }
            set { SetProperty(ref _ProductTable, value); }
        }

        private List<string> _ProductList;
        public List<string> ProductList
        {
            get { return _ProductList; }
            set { SetProperty(ref _ProductList, value); }
        }

        private string _MinProdMoisture;
        public string MinProdMoisture
        {
            get { return _MinProdMoisture; }
            set { SetProperty(ref _MinProdMoisture, value); }
        }
        private string _MaxProdMoisture;
        public string MaxProdMoisture
        {
            get { return _MaxProdMoisture; }
            set { SetProperty(ref _MaxProdMoisture, value); }
        }

        private string _MaxProdWeight;
        public string MaxProdWeight
        {
            get { return _MaxProdWeight; }
            set { SetProperty(ref _MaxProdWeight, value); }
        }

        private string _CautionMoistLow;
        public string CautionMoistLow
        {
            get { return _CautionMoistLow; }
            set { SetProperty(ref _CautionMoistLow, value); }
        }

        private string _CautionMoistHigh;
        public string CautionMoistHigh
        {
            get { return _CautionMoistHigh; }
            set { SetProperty(ref _CautionMoistHigh, value); }
        }


        private string _MinProdWeight;
        public string MinProdWeight
        {
            get { return _MinProdWeight; }
            set { SetProperty(ref _MinProdWeight, value); }
        }

        private string _CautionWTLow;
        public string CautionWTLow
        {
            get { return _CautionWTLow; }
            set { SetProperty(ref _CautionWTLow, value); }
        }

        private string _CautionWTHigh;
        public string CautionWTHigh
        {
            get { return _CautionWTHigh; }
            set { SetProperty(ref _CautionWTHigh, value); }
        }

        private String _instruction;
        public string instruction
        {
            get { return _instruction; }
            set { SetProperty(ref _instruction, value); }
        }

        private bool _HotKeyChecked = false;
        public bool HotKeyChecked
        {
            get { return _HotKeyChecked; }
            set
            {
                SetProperty(ref _HotKeyChecked, value);
                Settings.Default.bHotKeyChecked = value;
                Settings.Default.Save();
            }
        }
        
       
      

        //Default Values--------------------------------------
        private string _pDWeight;
        public string PDWeight
        {
            get { return _pDWeight; }
            set { SetProperty(ref _pDWeight, value); }
        }
        private string _pDNtWeight;
        public string PDNtWeight
        {
            get { return _pDNtWeight; }
            set { SetProperty(ref _pDNtWeight, value); }
        }
        private string _pDForte;
        public string PDForte
        {
            get { return _pDForte; }
            set { SetProperty(ref _pDForte, value); }
        }
        private string _pDMoisture;
        public string PDMoisture
        {
            get { return _pDMoisture; }
            set { SetProperty(ref _pDMoisture, value); }
        }
        //-----------------------------------------------------
        // Stock Labels----------------------------------------

        private string _lbFieldOne;
        public string LbFieldOne
        {
            get { return _lbFieldOne; }
            set { SetProperty(ref _lbFieldOne, value); }
        }
        private string _lbFieldTwo;
        public string LbFieldTwo
        {
            get { return _lbFieldTwo; }
            set { SetProperty(ref _lbFieldTwo, value); }
        }
        private string _lbFieldThree;
        public string LbFieldThree
        {
            get { return _lbFieldThree; }
            set { SetProperty(ref _lbFieldThree, value); }
        }
        private string _lbFieldFour;
        public string LbFieldFour
        {
            get { return _lbFieldFour; }
            set { SetProperty(ref _lbFieldFour, value); }
        }

        private string _FieldOne;
        public string FieldOne
        {
            get { return _FieldOne; }
            set { SetProperty(ref _FieldOne, value); }
        }
        private string _FieldTwo;
        public string FieldTwo
        {
            get { return _FieldTwo; }
            set { SetProperty(ref _FieldTwo, value); }
        }
        private string _FieldThree;
        public string FieldThree
        {
            get { return _FieldThree; }
            set { SetProperty(ref _FieldThree, value); }
        }
        private string _FieldFour;
        public string FieldFour
        {
            get { return _FieldFour; }
            set { SetProperty(ref _FieldFour, value); }
        }


        private bool _bMtCauLimit;
        public bool BMtCauLimit
        {
            get { return _bMtCauLimit; }
            set { SetProperty(ref _bMtCauLimit, value); }
        }

        private bool _MUseCaution;
        public bool MUseCaution
        {
            get { return _MUseCaution; }
            set
            {
                SetProperty(ref _MUseCaution, value);
                BMtCauLimit = value;
                if (value) CoutionMtOpa = "1";
                else CoutionMtOpa = ".3";
            }
        }

        private string _CoutionMtOpa = "1";
        public string CoutionMtOpa
        {
            get { return _CoutionMtOpa; }
            set { SetProperty(ref _CoutionMtOpa, value); }
        }

        private string _CoutionWtOpa = "1";
        public string CoutionWtOpa
        {
            get { return _CoutionWtOpa; }
            set { SetProperty(ref _CoutionWtOpa, value); }
        }

        //CoutionWtOpa

        private bool _bWtCauLimit;
        public bool BWtCauLimit
        {
            get { return _bWtCauLimit; }
            set { SetProperty(ref _bWtCauLimit, value); }
        }

        private bool _WTUseCaution;
        public bool WTUseCaution
        {
            get { return _WTUseCaution; }
            set
            {
                SetProperty(ref _WTUseCaution, value);
                BWtCauLimit = value;

                if (value) CoutionWtOpa = "1";
                else CoutionWtOpa = "0.3";
            }
        }

      
        private int _ProdSelectedIndex;
        public int ProdSelectedIndex
        {
            get { return _ProdSelectedIndex; }
            set
            {
                SetProperty(ref _ProdSelectedIndex, value);
                StockSelectedIndex = ProdSelectedIndex;

                if (value > -1)
                {
                    Settings.Default.ProdSelectIndex = value;
                    Settings.Default.Save();
                }
            }
        }
        /// <summary>
        /// END Product tab /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>

        #endregion Product -----------------------------------------------------



        

     


    }


}
