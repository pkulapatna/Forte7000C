using ClsErrorLog;
using Forte7000C.Properties;
using Forte7000C.Reports;
using Forte7000E.Module.LotProcess.Models;
using Forte7000E.Module.SerialCom.Models;
using Forte7000E.Services;
using Microsoft.VisualBasic;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// Events: Scale is inside
/// 1. Read Empty cell for Up count, no bale on scale
/// 2. Bale on Scale, read weight and down count
/// 
/// Events: Scale is outside
/// 1. Bale on scale, Read Weight and Upcount
/// 2. Put Bale in the test Cell read the down count
/// 
/// (options) Settings.Default.bForte2Option
/// a. Rotate bale and  Read Downcount2
/// b. Temperature measurements
/// 
/// 5 Calculate Moiosture. 
/// 6 Add Bale informations ( serial number, lot, etc...) 
/// 7 Write Data to Sql Server
/// 8 Send all data output (send to out)
/// 9 Update screen, Read sql table
/// 
/// 
/// </summary>
namespace Forte7000C.Models
{
    public class ClsProcessModel 
    {
        public static ClsProcessModel ProcessWindows;

        protected readonly Prism.Events.IEventAggregator _eventAggregator;
        private readonly EventsTimers _eventsTimer;
        private readonly SerialDevicesModel _serailDeviceModel;
        private readonly LotProcessModel _lotModel;
        private string _strDataOneOut;
        private string _strSharedOut;
        private CrystalReport _cryReport;
        private CrystalReport _cryBaleReport;
        private List<string> _xmlColumnList = new List<string>();

       // private QuartzTimer MyQtimer;

        private ObservableCollection<CheckedListItem> AvailableItemList { get; set; }
        private bool DayEnded { get; set; }
        private bool MonthEnded { get; set; }

        private string _XmlGdvDirectory;
        public string XMLRealTimeGdvFile
        {
            get { return _XmlGdvDirectory + Path.Combine(MyXml.SettingsGdvFile); } 
            set
            {
                if (value != null)
                    _XmlGdvDirectory = value;
            }
        }
        
        public string XMLSerialOneOut
        {
            get { return _XmlGdvDirectory + Path.Combine(MyXml.SettingsGdvFile);  }
        }
        
        public DataTable HdrTable;
        public enum Events
        {
            idle,
            Initialize,
            ReadUpCount,
            ReadScaleOutside,
            ReadDowncount,
            ReadDowncount2,
            PrepDowncount2,
            ReadScaleInside,
            ReadTemperature,
            InitCompleted,
            ProcessData,
            SendtoOutput,
            UpdateData,
            TestOsc,
            ReadOscError,
            TestScale,
            TestSerialOne,
            SendSerialOneOut
        }
        public Events _events;

        private readonly AccessHandler AccessDbHandler;

        private DataTable ProductTable;
        private DataTable CalibrationTable;
        private DataTable ProductDefTable;

        private double _dDowncount1 = 0;
        private double _dDowncount2 = 0;
        private double _dScaleWeightUp = 0;
        private double _dScaleWeightDn = 0;
     
        private bool BPrintDayEnd { get; set; }
     
        private ObservableCollection<string> SelectedHdrList;
        private string ReportTitle = string.Empty;

        public Sqlhandler SqlHandler { get; set; }
        public Xmlhandler MyXml { get; set; }

        public ClsProcessModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            ProcessWindows = this;
            ClassCommon.BDefaultWeight = false;

            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.APPSTART, "Init ClsProcessModel ...................");
            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.APPSTART, "Init ClsProcessModel ...................");

            //Always using ---------------------------------------------
            AccessDbHandler = new AccessHandler();
            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.APPSTART, "Init AccessHandler .....................");

            _cryReport = new CrystalReport();
            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.APPSTART, "Init _cryReport .........................");

            MyXml = Xmlhandler.Instance; //new Xmlhandler();
            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.APPSTART, "Init XML files .........................");

            SqlHandler = Sqlhandler.Instance; // new Sqlhandler();
            SqlHandler.SetRemoveFields();
            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.APPSTART, "Init Sqlhandler ........................");

            //Setup Serial Devices.(new) using SerialCom project
            _serailDeviceModel = new SerialDevicesModel(_eventAggregator);
            _serailDeviceModel.SetUpSerialDevices();

            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.APPSTART, "Init SerialDevices .....................");

            _eventsTimer = new EventsTimers(_eventAggregator);
            _lotModel = new LotProcessModel(_eventAggregator);

            if (ClassCommon.LotEnable)
            {
                _lotModel.InitLotProcess();
            }

            //MyQtimer = new QuartzTimer(_eventAggregator);
           // MyQtimer.StartDayEndQuart("15", "10");

            //MyQtimer.StartPeriodQuart();

            ClassCommon.DayEnd = Settings.Default.DayEndTime;

            ProductDefTable = new DataTable();
            CalibrationTable = new DataTable();
            ProductTable = new DataTable();

            AvailableItemList = new ObservableCollection<CheckedListItem>();

            SetStartUpSerialNumber();
            GetprevData();

           _eventsTimer.InitializeMainEventsTimer("mainTimer");

            _eventAggregator.GetEvent<UpdateMainTimerEvents>().Subscribe(UpdateMainTimeEvent);
            _eventAggregator.GetEvent<UpdateOscdataEvent>().Subscribe(OscDataReceived);
            _eventAggregator.GetEvent<UpdateScaledataEvent>().Subscribe(ScaleDataReceived);
            _eventAggregator.GetEvent<QuartzScheduler>().Subscribe(QuartzEvents);
            _eventAggregator.GetEvent<PrintScheduleEvent>().Subscribe(PrintEvents);
            _eventAggregator.GetEvent<ScaleErrorEvent>().Subscribe(ScaleReadEvents);

            _eventAggregator.GetEvent<QuartzDayEbdEvents>().Subscribe(ReceiveQuartzDayEndEvents);

            _eventAggregator.GetEvent<QuartzEvents>().Subscribe(ReceiveQuartzEvents);

            _events = Events.idle;   
        }

        private void ReceiveQuartzEvents(string obj)
        {
            Console.WriteLine(obj);
        }

        private void ReceiveQuartzDayEndEvents(string obj)
        {
            Console.WriteLine(obj);
        }

        public ClsProcessModel(bool dayEnded)
        {
            DayEnded = dayEnded;
        }

        private void GetprevData()
        {
           DataTable CurSqlDatTable = GetCurArchivesTable();
            
            int PreUidNum;

            if (CurSqlDatTable.Rows.Count == 0)
            {
                DataTable PreSqlDatTable = GetPreArchivestable();

                if (PreSqlDatTable.Rows.Count > 0)
                {
                    PreUidNum = PreSqlDatTable.Rows[0].Field<int>("UID");
                    ClassCommon.SerialNumber = PreSqlDatTable.Rows[0].Field<int>("SerialNumber").ToString();
                    ClassCommon.IIndex = PreSqlDatTable.Rows[0].Field<int>("index");
                }
                else //should only happened the first time around, when the table is empty.
                {
                    PreUidNum = 0;
                    ClassCommon.SerialNumber = "1";
                    ClassCommon.IIndex = 0;
                }
            }
            else
            {
                PreUidNum = CurSqlDatTable.Rows[0].Field<int>("UID");
                ClassCommon.SerialNumber = CurSqlDatTable.Rows[0].Field<int>("SerialNumber").ToString();
                ClassCommon.IIndex = CurSqlDatTable.Rows[0].Field<int>("index");
            }

            //UID turn over after  1,999,999 32 Bit int MAX = 2,147,483,647
            if (PreUidNum == 1999999) ClassCommon.UID = 0; 
            else ClassCommon.UID = PreUidNum;

            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.APPSTART, "GetprevData Done ..........................");
        }

        private void ScaleReadEvents(int obj)
        {
            switch (obj)
            {
                case (int)ClassCommon.ScaleRead.ReadBAD:
                    
                    ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DEVICE, "Scale is not connected, Use Deault Weight!");
                   
                    //Use default weight from product type here
                    DataTable MyTab = GetMaterialData(ClassCommon.CurrStock);

                    if (_events == Events.ReadScaleInside) //Final state
                    {
                        _dScaleWeightDn = MyTab.Rows[0].Field<Single>("DefaultNetWeight");
                        ClassCommon.BDefaultWeight = true;
                        ChangeState(Events.ProcessData);
                    }
                    else if (_events == Events.ReadScaleOutside)
                    {
                        _dScaleWeightUp = MyTab.Rows[0].Field<Single>("DefaultNetWeight");
                        ClassCommon.BDefaultWeight = true;
                    }
                    break;
            }
        }

        private void PrintEvents(int prnId)
        {
            if (SetUpRepTable(prnId))
            {
                _ = _cryReport.PrintReportAsync();
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.REPORTS, " Print " + ReportTitle + DateTime.Now);
                BPrintDayEnd = false;
            }  
        }

        /// <summary>
        /// Time events from system timer
        /// </summary>
        /// <param name="timenow"></param>
        private void UpdateMainTimeEvent(DateTime timenow)
        {
            //string archTable = SqlHandler.m_CurrentAchivesTable;

            SetTimeandEvents(timenow);

            //Case 2 _dayEnded 
            if (ClassCommon.HourNow == ClassCommon.DayEndHrMn)
            {
                DayEnded = true;
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.SYSEVENT, "DayEnd Occurred @ " + DateTime.Now);

                //Send Dayend Msg.
                _eventAggregator.GetEvent<UpdateAllEvent>().Publish((int)ClassCommon.TimeEvents.dayend);

                // Handle Day Report
                if ((Settings.Default.bDayEndRepCheck) & (BPrintDayEnd == false))
                {
                    if (SetUpRepTable((int)ClassCommon.PrntEvents.DayEnd))
                        _eventsTimer.InitializePrintEventTimer((int)ClassCommon.PrntEvents.DayEnd);
                }
            }

            //Case 3 _monthEnded  
            if ((ClassCommon.DateNow == "01") & (ClassCommon.HourNow == ClassCommon.DayEndHrMn))
            {
                MonthEnded = true;
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.SYSEVENT, "MonthEnd Occurred @ " + DateTime.Now);

                //Send Month end Msg.
                _eventAggregator.GetEvent<UpdateAllEvent>().Publish((int)ClassCommon.TimeEvents.monthend);

                // Handle Month Report
                if (Settings.Default.bMonthEndRepCheck)
                {
                    if (SetUpRepTable((int)ClassCommon.PrntEvents.MonthEnd))
                        _eventsTimer.InitializePrintEventTimer((int)ClassCommon.PrntEvents.MonthEnd);
                }

                //create new Archive table
                SqlHandler.InitialSetupSqlDataBase();
                //Reset 
                ClassCommon.IIndex = 0;
            }

            //Case 4 Shift one 
            if (ClassCommon.HourNow == ClassCommon.ShiftOneHrMn)
            {
                //Send Shift one end Msg.
                _eventAggregator.GetEvent<UpdateAllEvent>().Publish((int)ClassCommon.TimeEvents.shiftone);

                ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.SYSEVENT, "Shift One @ " + DateTime.Now);

                //Handle Shift one print
                if (Settings.Default.bShiftOneCheck & Settings.Default.bShiftRepCheck)
                {
                    if (SetUpRepTable((int)ClassCommon.PrntEvents.ShiftOne))
                        _eventsTimer.InitializePrintEventTimer((int)ClassCommon.PrntEvents.ShiftOne);
                }
            }

            //Case 5 Shift two
            if (ClassCommon.HourNow == ClassCommon.ShiftTwoHrMn)
            {
                //Send Shift Two Msg.
                _eventAggregator.GetEvent<UpdateAllEvent>().Publish((int)ClassCommon.TimeEvents.shifttwo);

                ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.SYSEVENT, "Shift Two @ " + DateTime.Now);

                //Handle Shift two print
                if (Settings.Default.bShiftTwoCheck & Settings.Default.bShiftRepCheck)
                {
                    if (SetUpRepTable((int)ClassCommon.PrntEvents.ShiftTwo))
                        _eventsTimer.InitializePrintEventTimer((int)ClassCommon.PrntEvents.ShiftTwo);
                }
            }

            //Case 6 Shift three
            if (ClassCommon.HourNow == ClassCommon.ShiftThreeHrMn)
            {
                //Send Shift Three Msg.
                _eventAggregator.GetEvent<UpdateAllEvent>().Publish((int)ClassCommon.TimeEvents.shiftthree);

                ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.SYSEVENT, "Shift Three @ " + DateTime.Now);

                //Handle Shift three print
                if (Settings.Default.bShiftThreeCheck & Settings.Default.bShiftRepCheck)
                {
                    if (SetUpRepTable((int)ClassCommon.PrntEvents.ShiftThree))
                        _eventsTimer.InitializePrintEventTimer((int)ClassCommon.PrntEvents.ShiftThree);
                }
            }

            Console.WriteLine("HourNow= " + ClassCommon.HourNow + "  DayEndHour= " + ClassCommon.DayEndHrMn + "  Today= " + ClassCommon.DateNow);
        }

        private void QuartzEvents(DateTime obj)
        {
          //  throw new NotImplementedException();
        }

        internal int GetNextLotNumber()
        {
           return _lotModel.GetNextLotNumber();
        }

        private int GetPreSerialNumber()
        {
            int curSerNum = 0;
           // int ilotstatus = 0;
            //DataTable MyPreDataTab = new DataTable();

            string strquery = "SELECT top 2 * from " + CurrentArchivestable() + " ORDER BY UID DESC;";
            DataTable MyPreDataTab = SqlHandler.GetForteDataTable(strquery);

            if (MyPreDataTab.Rows.Count > 0)
            {
                curSerNum = MyPreDataTab.Rows[0].Field<int>("SerialNumber");
             //   ilotstatus = Convert.ToInt16(MyPreDataTab.Rows[0]["LotNumberStatus"]); 
            }
            /*
            switch (ilotstatus)
            {
                case 0:
                    ClassCommon.LotStatus = "Open";
                    break;
                case 1:
                    ClassCommon.LotStatus = "Closed";
                    break;
                case 2:
                    ClassCommon.LotStatus = "LotReset";
                    break;
            }
            */
            return curSerNum;
        }

        /// <summary>
        /// There are 2 reading for scale weight
        /// only one of the 2 is good
        /// depended where the scale is outside or inside of the test cell.
        /// </summary>
        /// <param name="obj"></param>
        private void ScaleDataReceived(string scaleDataReceive)
        {

            if ((scaleDataReceive != null) && (scaleDataReceive.Length > 0))
            {
                var doubleArray = Regex.Split(scaleDataReceive, @"[^0-9\.]+")
                                    .Where(c => !String.IsNullOrEmpty(c) && c != ".").ToArray();
                string WeightDat = doubleArray[0].ToString();

                if (_events == Events.ReadScaleInside) //Final state
                {
                    _dScaleWeightDn = Convert.ToDouble(doubleArray[0]);
                    ChangeState(Events.ProcessData);

                }
                else if (_events == Events.ReadScaleOutside)
                {
                    _dScaleWeightUp = Convert.ToDouble(doubleArray[0]);
                }
                else if (_events == Events.TestScale)
                {
                    //_eA.GetEvent<UpdateScaledataEvent>().Publish(ScaleDataReceive.Trim());
                }
            }
            else
            {
                _dScaleWeightDn = 10.00;
                _dScaleWeightUp = 20.00;
            }

        }

        /// <summary>
        /// 
        /// There are 2 readings for OSC UpCount and down count
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void OscDataReceived(string oscDataReceive)
        {
            if (oscDataReceive.Length > 17)
            {
                string strUp = oscDataReceive.Substring(2 - 0, 7);
                string strDn = oscDataReceive.Substring(10 - 1, 6);

                double dUpCount = Conversion.Val("&H" + strUp);
                double dDownCount = Conversion.Val("&H" + strDn);

                ClassCommon.DDCount = dDownCount;

                switch (_events)
                {
                    case Events.ReadUpCount:

                        ClassCommon.DUpCount = (dUpCount + dDownCount) / 2;
                        if (ClassCommon.DUpCount > 100)
                        {
                            ClassCommon.DRefernce = ClassCommon.DUpCount;
                            _eventAggregator.GetEvent<SendOscdataEvent>().Publish(ClassCommon.DUpCount);

                            if (!Settings.Default.bScaleInside)
                                ChangeState(Events.ReadScaleOutside);
                            else
                            {   // Tell UI that it is done reading.
                                _eventAggregator.GetEvent<SendOscdataEvent>().Publish(0);
                            }
                        }
                        break;

                    case Events.ReadDowncount:

                        _dDowncount1 = (dUpCount + dDownCount) / 2;
                        if (_dDowncount1 > 100)
                        {
                            ClassCommon.DReading = _dDowncount1;
                            _eventAggregator.GetEvent<SendOscdataEvent>().Publish(_dDowncount1);

                            // if reading is good continue
                            ChangeState(Events.ReadScaleInside);
                        }
                        //else
                        //try one more time
                        break;

                    case Events.ReadDowncount2:
                        _dDowncount2 = (dUpCount + dDownCount) / 2;
                        if (_dDowncount2 > 100)
                        {
                            ClassCommon.DReading = _dDowncount2;
                            _eventAggregator.GetEvent<SendOscdataEvent>().Publish(_dDowncount2);

                            // if reading is good continue
                            ChangeState(Events.ProcessData);
                        }
                        break;

                    case Events.TestOsc:

                        break;
                }
            }
            else
            {
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.CRITICAL, MsgSources.BALEPROC, "ERROR in Reading Osc Data ");
            }
        }

        /// <summary>
        /// 5 Calculate Moiosture. 
        /// </summary>
        private void CalculateMoisture()
        {
            ClassCommon.MoistMes = string.Empty;
            ClassCommon.WtMes = string.Empty;

            try
            {
                if (_dScaleWeightUp > _dScaleWeightDn) ClassCommon.DScaleWeight = _dScaleWeightUp;
                else ClassCommon.DScaleWeight = _dScaleWeightDn;

                int Prodindex = Settings.Default.ProdSelectIndex;

                ClassCommon.DAConst = ProductDefTable.Rows[Prodindex].Field<Double>("A");
                ClassCommon.DBConst = ProductDefTable.Rows[Prodindex].Field<Double>("B");
                ClassCommon.DCConst = ProductDefTable.Rows[Prodindex].Field<Double>("C");

                ClassCommon.DTareWeight = ProductDefTable.Rows[Prodindex].Field<Single>("TareWeight");
                ClassCommon.DOrigWeight = ClassCommon.DScaleWeight;
                ClassCommon.DNetWeight = ClassCommon.DScaleWeight + ClassCommon.DTareWeight;

                ClassCommon.DCalcForte1 = ClassCommon.DUpCount - _dDowncount1;

                //Incase Rotate bale 90 degree to get another down count
                if (Settings.Default.bForte2Option)
                {
                    ClassCommon.DCalcForte2 = ClassCommon.DUpCount - _dDowncount2;
                    ClassCommon.DCalcForte = (ClassCommon.DCalcForte1 + ClassCommon.DCalcForte2) / 2;
                }
                else
                    ClassCommon.DCalcForte = ClassCommon.DCalcForte1;

                double dMoisture = (ClassCommon.DAConst * Math.Log10(ClassCommon.DCalcForte / ClassCommon.DScaleWeight)) + ClassCommon.DBConst;

                double dMoistureA = ClassCommon.DAConst * Math.Log10(ClassCommon.DCalcForte / ClassCommon.DScaleWeight) + ClassCommon.DBConst;

                ClassCommon.RealMoisture = dMoisture;

                double dRealMoisture = GetMoistureUnit(dMoisture);
                double dlowlimit = ProductDefTable.Rows[Prodindex].Field<Single>("MinMoisture");
                double dHighLoimit = ProductDefTable.Rows[Prodindex].Field<Single>("MaxMoisture");

                if ((dRealMoisture > 0) & (dRealMoisture < 1000))
                {
                    switch (ClassCommon.MoistureType)
                    {
                        case 0: // %MC == moisture from Sql database
                                if (dRealMoisture < dlowlimit)
                                ClassCommon.MoistMes = "Dry!";
                                else if (dRealMoisture > dHighLoimit)
                                ClassCommon.MoistMes = "Wet!";
                        break;

                        case 1: // %MR  = Moisture / ( 1- Moisture / 100)
                                if (dRealMoisture < dlowlimit)
                                ClassCommon.MoistMes = "Dry!";
                                else if (dRealMoisture > dHighLoimit)
                                ClassCommon.MoistMes = "Wet!";
                        break;

                        case 2: // %AD = (100 - moisture) / 0.9
                                if (dRealMoisture > dlowlimit)
                                ClassCommon.MoistMes = "Dry!";
                                else if (dRealMoisture < dHighLoimit)
                                ClassCommon.MoistMes = "Wet!";
                        break;

                        case 3: // %BD  = 100 - moisture
                                if(dRealMoisture > dlowlimit)
                                ClassCommon.MoistMes = "Dry!";
                                else if (dRealMoisture < dHighLoimit)
                                ClassCommon.MoistMes = "Wet!";
                        break;
                    }

                    ClassCommon.DMoisture = dRealMoisture; // After moisture type selections.
                }
                else
                {
                    ClassCommon.DMoisture = ProductDefTable.Rows[Prodindex].Field<Single>("DefaultMoisture");
                    ClassCommon.MoistMes = "Def.";
                }

                if(ClassCommon.BDefaultWeight)
                {
                    ClassCommon.WtMes = "Default";
                }
                else
                {
                    if (ClassCommon.DScaleWeight > ProductDefTable.Rows[Prodindex].Field<Single>("MaxWeight"))
                        ClassCommon.WtMes = "Heavy";

                    if (ClassCommon.DScaleWeight < ProductDefTable.Rows[Prodindex].Field<Single>("MinWeight"))
                        ClassCommon.WtMes = "Light";
                }

                ClassCommon._timestamp = DateTime.Now;

                ClassCommon.StockName = ProductDefTable.Rows[Prodindex][0].ToString();
                ClassCommon.CalibrationName = ProductDefTable.Rows[Prodindex][1].ToString();

                ClassCommon.UID +=1; //Update UID
                ClassCommon.IIndex += 1; //Update Index

                ClassCommon.MonthCode = "JUL";

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("ERROR in CalculateMoisture " + ex);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.BALEPROC, "ERROR in CalculateMoisture " + ex.Message);
            }
        }

        private void BaleDataFunction()
        {
            //1
            CalculateMoisture(); //5 Calculate Moiosture. 

            //2 UpdateSerialNumber();

            //3 
         
            ChangeState(Events.UpdateData);
        }

       
        public double GetMoistureUnit(double mVal)
        {
            double MoistureDat = mVal;

            switch (ClassCommon.MoistureType)
            {
                case 0: // %MC == moisture from Sql database
                    MoistureDat = mVal;
                    break;

                case 1: // %MR  = Moisture / ( 1- Moisture / 100)
                    MoistureDat = (mVal / (1 - mVal / 100));
                    break;

                case 2: // %AD = (100 - moisture) / 0.9
                    MoistureDat = ((100 - mVal) / 0.9);
                    break;

                case 3: // %BD  = 100 - moisture
                    MoistureDat = (100 - mVal);
                    break;
            }
            return MoistureDat; 
        }


        public enum States
        {
            idle, Initialize, ReadScale, ReadOsc, Processdata, UpdateSql, SendOutput
        }

        public enum Triggers
        {
            Startup, ReadScaleCommand, ReadOscCommand, TestScale, TestOsc, SendData, SendReport
        }

        public void ChangeState(Events _state)
        {
            _events = _state;

            switch (_state)
            {
                case Events.idle:
                    InitMe();
                    break;

                case Events.Initialize:

                   // SqlHandler.InitialSetupSqlDataBase();
                    break;

                case Events.InitCompleted:

                    break;

                case Events.ReadUpCount:
                    ClassCommon.EventsState = ClassCommon.State[ClassCommon.ReadUpCount];
                    ReadOsc();
                    break;
                case Events.ReadScaleOutside:
                    ReadSale();
                    break;

                case Events.ReadDowncount:
                    ClassCommon.EventsState = ClassCommon.State[ClassCommon.ReadDowncount];
                    ReadOsc();
                    break;

                case Events.ReadDowncount2:
                    ReadOsc();
                    break;

                case Events.ReadScaleInside:
                    ReadSale();
                    break;

                case Events.ReadTemperature:
                    ReadTemperature();
                    break;
                    
                case Events.ProcessData:
                    BaleDataFunction();
                    break;

                case Events.UpdateData:
                    UpdateBaleData();
                    break;

                case Events.SendtoOutput:
                    HandleDataOutput();
                    break;

                case Events.TestOsc:
                    //TestOsc();
                    ReadOsc();
                    break;

                case Events.TestScale:
                    ReadSale();
                    break;

                case Events.TestSerialOne:
                    TestSerialOneout(_strDataOneOut);
                    break;

                case Events.ReadOscError:
                    System.Windows.MessageBox.Show("ERROR");
                    break;
            }
        }

        private void TestSerialOneout(string strDataOut)
        {
            _serailDeviceModel.SendSerialOneOut(strDataOut);
        }

        /// <summary>
        /// 
        /// </summary>
        internal void UpdateSerialNumber()
        {
            int preSn = GetPreSerialNumber();

            if(ClassCommon.BSnReset)
            {
                ClassCommon.SerialNumber = "1";
                ClassCommon.BSnReset = false;
            }
            else
            {
                switch (Settings.Default.SNResetType)
                {
                    case ClassCommon.SNRollOver:
                        if (preSn - 1 > Settings.Default.iSerialMax)
                            ClassCommon.SerialNumber = "1";
                        else
                        {
                            if (Convert.ToInt32(ClassCommon.SerialNumber) == preSn)
                                ClassCommon.SerialNumber = (preSn + 1).ToString();
                        }
                        break;

                    case ClassCommon.SNDayEnd:
                        if (DayEnded)
                        {
                            ClassCommon.SerialNumber = "1";
                            DayEnded = false;
                        }
                        else
                        {
                            //If not change S/N on Screen Continue
                            if (Convert.ToInt32(ClassCommon.SerialNumber) == preSn)
                                ClassCommon.SerialNumber = (preSn + 1).ToString();
                        }
                        break;

                    case ClassCommon.SNMonthEnd:
                        if (MonthEnded)
                        {
                            ClassCommon.SerialNumber = "1";
                            MonthEnded = false;
                        }
                        else
                        {
                            //If not change S/N on Screen Continue
                            if (Convert.ToInt32(ClassCommon.SerialNumber) == preSn)
                                ClassCommon.SerialNumber = (preSn + 1).ToString();
                        }
                        break;

                    default:
                        ClassCommon.SerialNumber = "1";
                        break;
                }

            }     
        }

        /// <summary>
        /// Do Bale Process for each bale
        /// update revolving variables such as Serial Number, lot# 
        /// </summary>
        private void UpdateBaleData()
        {
            //1
            UpdateShiftEvents();  
            //2
            UpdateSerialNumber(); 
            
            //3  $LOT only if lot process is enabled
            if (ClassCommon.LotEnable)
            {
                _lotModel.ChangeLotState(LotProcessModel.LotEvents.NewBaleArrive);
            }

            //4 Update SQL BaleArchive, screen UI and Lot data
            if (SqlHandler.UpdateBaleArchiveTableAsy(true)) 
            {

                if(ClassCommon.LotEnable) _lotModel.UpdateBaleLotData();
                _eventAggregator.GetEvent<UpdateSqldataEvent>().Publish(ClassCommon.UID.ToString());
            }

            // 5 If Lot closed! //ClassCommon.CloseTD = DateTime.Now;
            if (ClassCommon.LotEnable && ClassCommon.LotStatus == "Closed")
            {
                _lotModel.ChangeLotState(LotProcessModel.LotEvents.UpdateLotArchiveTable);
            }
            ChangeState(Events.SendtoOutput);
        }


        private void HandleDataOutput()
        {
            if (_serailDeviceModel.CheckSerialOneEnable())
            {
                _strDataOneOut = GetDataFromXmlfile(ClassCommon.outSerialOne);
                TestSerialOneout(_strDataOneOut);
            }

            if(Settings.Default.SharedFileChecked)
            {
                _strSharedOut = GetDataFromXmlfile(ClassCommon.outSharedFile);
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(Settings.Default.SharedFileLocation, Settings.Default.SharedFileName)))
                {
                    outputFile.WriteAsync(_strSharedOut);
                }
            }

            if(Settings.Default.BalePrintChecked)
            {
                PrintBale(0);
            }

            ChangeState(Events.idle);
        }

        public string GetDataFromXmlfile(int idtype)
        {
            string strOut = string.Empty;

            SelectedHdrList = new ObservableCollection<string>();
            try
            {
                ObservableCollection<DataOutput> DataOut = ReadXmlSerialOneOut(idtype);
                DataTable lvDatatable = UpdateSummaryDataGrid(20);
                DataColumnCollection columns = lvDatatable.Columns;

                for (int i = 0; i < DataOut.Count; i++)
                {
                    if (DataOut[i].FieldType == "Ascii")
                    {
                        DataOut[i].StrValue = ClassCommon.Asciilist[DataOut[i].Id].Item2.ToString();
                        strOut += DataOut[i].StrValue;
                    } 
                    else if (columns.Contains(DataOut[i].Name))
                    {
                        DataOut[i].StrValue = lvDatatable.Rows[0][DataOut[i].Name].ToString();
                        //Console.WriteLine(lvDatatable.Rows[0][DataOut[i].Name]);
                        strOut += DataOut[i].StrValue;
                    }
                    else
                    {
                        strOut += DataOut[i].Name;
                        //Console.WriteLine(DataOut[i].Name);
                    }   
                }
            }
            catch (Exception ex)
            {
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.CRITICAL, MsgSources.BALEPROC, "ERROR IN GetDataFromXmlfile " + ex);
                throw new Exception("ERROR IN GetDataFromXmlfile " + ex);
            }
            return strOut;
        }

        private void UpdateShiftEvents()
        {
            if ((DateTime.Now.Hour > Settings.Default.DayEndTime.Hour) 
                && (DateTime.Now.Hour < Settings.Default.ShiftOneTime.Hour) 
                && Settings.Default.bShiftOneCheck)
                ClassCommon.ShiftName = Settings.Default.ShiftOneName;
            else if ((DateTime.Now.Hour >= Settings.Default.ShiftOneTime.Hour) 
                && (DateTime.Now.Hour < Settings.Default.ShiftTwoTime.Hour) 
                && Settings.Default.bShiftTwoCheck)
                ClassCommon.ShiftName = Settings.Default.ShiftTwoName;
            else if ((DateTime.Now.Hour >= Settings.Default.ShiftTwoTime.Hour) 
                && (DateTime.Now.Hour < Settings.Default.ShiftThreeTime.Hour) 
                && Settings.Default.bShiftThreeCheck)
                ClassCommon.ShiftName = Settings.Default.ShiftThreeName;
            else
                ClassCommon.ShiftName = string.Empty;
        }   

        private void ReadTemperature()
        {
           // throw new NotImplementedException();
        }

        private void InitMe()
        {
           //Console.WriteLine("xxxxxxxxxxxxx InitMe");
        }
        /// <summary>
        /// Read Osc Empty cell used as Up count.
        /// </summary>
        private void ReadOsc()
        {
            _serailDeviceModel.SendOscRequestString("T");
        }

        private void ReadSale()
        {
            if (ClassCommon.SCaleModeAuto)
                _serailDeviceModel.GetScaleWeight();
            else ReadWeightDirect();
        }

        private void ReadWeightDirect()
        {
            _eventAggregator.GetEvent<UpdateScaledataEvent>().Publish(ClassCommon.DSWeightEntry);
        }

        internal DataTable CreateProductDefTable()
        {
            DataTable NewTable = new DataTable();
            DataTable TempTable;

            DataColumn colString = new DataColumn("Name")
            {
                DataType = System.Type.GetType("System.String")
            };
            NewTable.Columns.Add(colString);

            DataColumn CalName = new DataColumn("CalName")
            {
                DataType = System.Type.GetType("System.String")
            };
            NewTable.Columns.Add(CalName);

            DataColumn CalID = new DataColumn("CalID")
            {
                DataType = System.Type.GetType("System.Int16")
            };
            NewTable.Columns.Add(CalID);

            DataColumn CalA = new DataColumn("A")
            {
                DataType = System.Type.GetType("System.Double")
            };
            NewTable.Columns.Add(CalA);

            DataColumn CalB = new DataColumn("B")
            {
                DataType = System.Type.GetType("System.Double")
            };
            NewTable.Columns.Add(CalB);

            DataColumn CalC = new DataColumn("C")
            {
                DataType = System.Type.GetType("System.Double")
            };
            NewTable.Columns.Add(CalC);

            DataColumn DefaultMoisture = new DataColumn("DefaultMoisture")
            {
                DataType = System.Type.GetType("System.Single")
            };
            NewTable.Columns.Add(DefaultMoisture);

            DataColumn DefaultWeight = new DataColumn("DefaultWeight")
            {
                DataType = System.Type.GetType("System.Single")
            };
            NewTable.Columns.Add(DefaultWeight);

            DataColumn MinWeight = new DataColumn("MinWeight")
            {
                DataType = System.Type.GetType("System.Single")
            };
            NewTable.Columns.Add(MinWeight);

            DataColumn MaxWeight = new DataColumn("MaxWeight")
            {
                DataType = System.Type.GetType("System.Single")
            };
            NewTable.Columns.Add(MaxWeight);

            DataColumn MinMoisture = new DataColumn("MinMoisture")
            {
                DataType = System.Type.GetType("System.Single")
            };
            NewTable.Columns.Add(MinMoisture);

            DataColumn MaxMoisture = new DataColumn("MaxMoisture")
            {
                DataType = System.Type.GetType("System.Single")
            };
            NewTable.Columns.Add(MaxMoisture);

            DataColumn TareWeight = new DataColumn("TareWeight")
            {
                DataType = System.Type.GetType("System.Single")
            };
            NewTable.Columns.Add(TareWeight);

            ProductTable = SqlHandler.GetSqlDatatable("StockTable");
            CalibrationTable = GetSqlCalibreationsTable();// AccessDbHandler.GetAccessCalTable();

            try
            {
                for (int i = 0; i < ProductTable.Rows.Count; i++)
                {
                   // TempTable = AccessDbHandler.GetAccessCalParamsByProdIdTable(ProductTable.Rows[i]["CalIDLn1"].ToString());
                    TempTable = GetSqlCalParamsByProdIdTable(ProductTable.Rows[i].Field<Int16>("CalIDLn1").ToString());
                    var row = NewTable.NewRow();
                    row["Name"] = ProductTable.Rows[i].Field<String>("Name");
                    row["CalName"] = TempTable.Rows[0].Field<String>("Name");
                    row["CalID"] = ProductTable.Rows[i].Field<Int16>("CalIDLn1");
                    row["A"] = TempTable.Rows[0].Field<Double>("A"); 
                    row["B"] = TempTable.Rows[0].Field<Double>("B");
                    row["C"] = TempTable.Rows[0].Field<Double>("C");
                    row["DefaultMoisture"] = ProductTable.Rows[i].Field<Single>("DefaultMoisture");
                    row["DefaultWeight"] = ProductTable.Rows[i].Field<Single>("DefaultWeight");
                    row["MinWeight"] = ProductTable.Rows[i].Field<Single>("MinWeight");
                    row["MaxWeight"] = ProductTable.Rows[i].Field<Single>("MaxWeight");
               
                    row["MinMoisture"] = GetMoistureUnit(ProductTable.Rows[i].Field<Single>("MinMoisture"));
                    row["MaxMoisture"] = GetMoistureUnit(ProductTable.Rows[i].Field<Single>("MaxMoisture"));
                    row["TareWeight"] = ProductTable.Rows[i].Field<Single>("TareWeight");

                    NewTable.Rows.Add(row);
                    TempTable.Clear();
                }

                if(NewTable.Rows.Count > 0)
                        ProductDefTable = NewTable;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("ERROR IN GetProductDef " + ex);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.CRITICAL, MsgSources.BALEPROC, "ERROR IN GetProductDef " + ex);
            }
            return NewTable;
        }

        private DataTable GetSqlCalParamsByProdIdTable(string CalId)
        {
            return SqlHandler.GetSqlCalParamsByProdIdTable( CalId);
        }

        internal DataTable GetAccessCalibreationsTable()
        {
            return AccessDbHandler.GetAccessCalTable();
        }

        internal DataTable GetSqlCalibreationsTable()
        {
            return SqlHandler.GetCalibrationTable();
        }

        internal DataTable GetStockTable()
        {
            return AccessDbHandler.GetAccessStockTable();
        }
        internal DataTable GetAccDataTable()
        {
            return AccessDbHandler.GetAccDataTable();
        }

        private string CurrentArchivestable()
        {
            return SqlHandler.GetCurrentArchivesTable();
        }
        private string PreviousArchivestable()
        {
            return SqlHandler.GetPreviousBaleTable();
        }
        private string CurrentLotArchivestable()
        {
            return SqlHandler.GetCurrentLotArchivesTable();
        }


        internal DataTable GetCurArchivesTable( )
        {
            //DataTable MyData = new DataTable();

            string strquery = "SELECT top 2 * from " + CurrentArchivestable() + " ORDER BY UID DESC;" ;
            DataTable MyData = SqlHandler.GetForteDataTable(strquery);

            return MyData;
        }

        internal DataTable GetPreArchivestable()
        {
           // DataTable MyPreDataTab = new DataTable();

            string strquery = "SELECT top 2 * from " + PreviousArchivestable() + " ORDER BY UID DESC;";
            DataTable MyPreDataTab = SqlHandler.GetForteDataTable(strquery);

            return MyPreDataTab;
        }


        private DataTable GetMaterialData(string Material)
        {
            return  SqlHandler.GetMaterialData(Material);

           
        }

        internal List<string> GetTableitemList()
        {
            return SqlHandler.GetTableItemLists(CurrentArchivestable());
        }

        internal DataTable UpdateSummaryDataGrid(int baleSumDepth)
        {
            string strquery = GetQueryStrFromList(GetXmlSelectedHdrCheckedList(), baleSumDepth);

            DataTable oldData = SqlHandler.GetForteDataTable(strquery);
            DataColumnCollection columns = oldData.Columns;

            if (oldData.Rows.Count > 0)
            {
                DataRow[] rows = oldData.Select();
                for (int i = 0; i < rows.Length; i++)
                {
                    if ((columns.Contains("Moisture")) & (rows[i]["Moisture"] != null))
                    {
                        string newtype = ClassCommon.CalulateMoisture(rows[i].Field<float>("Moisture").ToString(), ClassCommon.MoistureType);           
                        oldData.Rows[i]["Moisture"] = string.Format("{0:0.00}", newtype);
                    }

                    if (columns.Contains("Weight") & rows[i]["Moisture"] != null)
                    {
                        if (ClassCommon.WeightType == ClassCommon.WtEnglish) //English Unit lb
                        {
                            if ((columns.Contains("Weight")) & (rows[i]["Weight"] != null))
                                rows[i]["Weight"] = rows[i].Field<float>("Weight") * 2.20462; //Lb
                            if (columns.Contains("BDWeight"))
                            {
                                if (rows[i]["BDWeight"] != null)
                                    rows[i]["BDWeight"] = rows[i].Field<float>("BDWeight") * 2.20462; //Lb.
                            }
                            if (columns.Contains("NetWeight"))
                            {
                                if (rows[i]["NetWeight"] != null)
                                    rows[i]["NetWeight"] = rows[i].Field<float>("NetWeight") * 2.20462; //Lb.
                            }
                        }
                        oldData.Rows[i]["Weight"] = String.Format("{0:0.00}", oldData.Rows[i]["Weight"]);
                        oldData.AcceptChanges();
                    }
                }
            }
            oldData.AcceptChanges();
            return oldData; // SqlHandler.GetForteDataTable(strquery);
        }

        internal async Task<DataTable> UpdateSummaryDataGridAsync(int baleSumDepth)
        {
            string strquery = GetQueryStrFromList(GetXmlSelectedHdrCheckedList(), baleSumDepth);
            return await SqlHandler.GetForteDataTableAsync(strquery);
        }


        private string GetQueryStrFromList(ObservableCollection<string> selectedHdrList, int BaleSumDept)
        {
            string strItems = string.Empty;
            char charsToTrim = ',';

            foreach (var Item in selectedHdrList)
            {
                strItems += Item + ",";
            }
            return "SELECT top " + BaleSumDept.ToString() + " " + strItems.TrimEnd(charsToTrim) + " from " + CurrentArchivestable() + "  ORDER BY UID DESC;";
        }

        private string GetLotQueryStrFromList(List<string> lotitems)
        {
            string strItems = string.Empty;
            char charsToTrim = ',';

            foreach (var Item in lotitems)
            {
                strItems += Item + ",";
            }
            return "SELECT " + strItems.TrimEnd(charsToTrim) + " from " + CurrentLotArchivestable() + "  ORDER BY UID DESC;"; 
        }

        public ObservableCollection<string> GetXmlSelectedHdrCheckedList()
        {
            ObservableCollection<string> XmlCheckedList = new ObservableCollection<string>();
            HdrTable = new DataTable();
            
            try
            {
                HdrTable = SqlHandler.GetSqlTableHdr();
                _xmlColumnList.Clear();

                _xmlColumnList = GetXmlcolumnList(XMLRealTimeGdvFile);
                AvailableItemList.Clear();

                if(( HdrTable.Rows.Count > 0) && (_xmlColumnList.Count > 0))
                {
                    foreach (DataRow item in HdrTable.Rows)
                    {
                        // Console.WriteLine("  ------   " + item[1].ToString());
                        if (AllowField(item[1].ToString()))
                        {
                            if (_xmlColumnList.Contains(item[1].ToString()))
                                AvailableItemList.Add(new CheckedListItem(Convert.ToInt32(item[0]), item[1].ToString(), true, item[2].ToString()));
                            else
                                AvailableItemList.Add(new CheckedListItem(Convert.ToInt32(item[0]), item[1].ToString(), false, item[2].ToString()));
                        }
                    }
                    foreach (var item in _xmlColumnList)
                    {
                        XmlCheckedList.Add(item);
                    }
                }
                return XmlCheckedList;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("ERROR in ClsBaleRealTimeModel GetXmlItemsList" + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.APPBALEREALTIME, ex.Message);
            }
            return XmlCheckedList;
        }


        private List<string> GetXmlcolumnList(string xMLDropsGdvFile)
        {
            return MyXml.ReadXmlGridView(xMLDropsGdvFile);
        }


        private bool AllowField(string strItem)
        {
            foreach (var item in SqlHandler.RemoveFieldsList)
            {
                if (item == strItem) return false;
            }
            return true;
        }

        internal ObservableCollection<string> RemoveHdrItem(ObservableCollection<string> orgList, string Removeitem)
        {
            ObservableCollection<string>  tempList = orgList;
            tempList.Remove(Removeitem);
            return tempList;
        }

        internal ObservableCollection<string> AddHdrItem(ObservableCollection<string> orgList, string NewItem)
        {
            ObservableCollection<string>  tempList = orgList;
            tempList.Add(NewItem);
            return tempList;
        }

        internal void SaveModifyFields()
        {
            List<CheckedListItem> CustomHdrList = new List<CheckedListItem>();
           
            foreach (var item in AvailableItemList)
            {
                if (item.IsChecked)
                    CustomHdrList.Add(new CheckedListItem(item.Id, item.Name, item.IsChecked, item.FieldType));
            }
            MyXml.WriteXmlGridView(CustomHdrList, MyXml.SettingsGdvFile);
            CustomHdrList.Clear();
        }


        internal void SaveXmlcolumnList(ObservableCollection<string> selectedHdrList)
        {
            MyXml.UpdateXMlcolumnList(selectedHdrList, XMLRealTimeGdvFile);
        }


        internal DataTable GetDatatable(string strTable)
        {
            return SqlHandler.GetSqlDatatable(strTable);
        }

        internal bool UpdateProductSqltable(string strUpdateQuery)
        {
            return SqlHandler.UpdateProductTable(strUpdateQuery);
        }

        internal List<string> GetSqlTableList()
        {
            return SqlHandler.GetSqltableList();
        }

        internal DataTable GetBaleDataTable(string strClause)
        {
            return SqlHandler.GetSqlArchivetable(strClause);
        }

        internal DataTable GetRepTable(string strquery)
        {
            return SqlHandler.GetForteDataTable(strquery);  
        }

        internal string GetCurrentArchivesTable()
        {
            return SqlHandler.GetCurrentArchivesTable();
        }

        private bool GetCryReportData(string strTimeFrame, string archTable)
        {
            char charsToTrim = ',';
            ObservableCollection<string>  SelectedHdrList = GetXmlSelectedHdrCheckedList();
            bool bAOK = false;

            try
            {
                string CheckItems = _cryReport.GetItemsString();
                CheckItems = CheckItems.TrimEnd(charsToTrim);
                string itemsList = string.Empty;
                
                foreach (var Item in SelectedHdrList)
                {
                    itemsList += Item + ",";
                }
                itemsList = itemsList.TrimEnd(charsToTrim);
                string strquery = "SELECT " + CheckItems + " From " + archTable + strTimeFrame + " ORDER by [index] ASC ";

                using (DataTable CryRepTable = GetRepTable(strquery))
                {
                    _cryReport.UpdateRepTable(CryRepTable);
                    _cryReport.Refresh();
                    if (CryRepTable.Rows.Count > 0) bAOK = true;
                    else ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.REPORTS, "No Record to print. @ " + DateTime.Now);
                }
   
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error in SetCryReport " + ex.Message);
                
            }
            return bAOK;
        }

        internal void KillAllTimers( bool LpEnable)
        {
            _eventsTimer.StopMainEventsTimer();
            if (_cryReport != null) _cryReport = null;

            if (LpEnable) StopLotEventTimer();

        }

        public void SetTimeandEvents(DateTime timenow)
        {
            ClassCommon.HourNow = timenow.ToString("HH:mm");
            ClassCommon.DateNow = timenow.ToString("dd");
            ClassCommon.DayEndHrMn = Settings.Default.DayEndTime.ToString("HH:mm");
            ClassCommon.ShiftOneHrMn = Settings.Default.ShiftOneTime.ToString("HH:mm");
            ClassCommon.ShiftTwoHrMn = Settings.Default.ShiftTwoTime.ToString("HH:mm");
            ClassCommon.ShiftThreeHrMn = Settings.Default.ShiftThreeTime.ToString("HH:mm");

            var d3 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Settings.Default.DayEndTime.Hour, Settings.Default.DayEndTime.Minute, 00);
            var d4 = d3.AddDays(-1);

            ClassCommon.StartDate = d4.ToString();
            ClassCommon.DayEndDate = d3.ToString();

            var dSf1 = new DateTime(d3.Year, d3.Month, d3.Day, Settings.Default.ShiftOneTime.Hour, Settings.Default.DayEndTime.Minute, 00);
            var dSf2 = new DateTime(d3.Year, d3.Month, d3.Day, Settings.Default.ShiftTwoTime.Hour, Settings.Default.DayEndTime.Minute, 00);
            var dSf3 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Settings.Default.ShiftThreeTime.Hour, Settings.Default.DayEndTime.Minute, 00);

            ClassCommon.ShiftOneDate = dSf1.ToString();
            ClassCommon.ShiftTwoDate = dSf2.ToString();
            ClassCommon.ShiftThreeDate = dSf3.ToString();
        }

        internal string GetXmlfile(int obj)
        {
            string xmlfile = string.Empty;
            ObservableCollection<DataOutput> myxmlfile = MyXml.ReadXmlStringOut(obj);

            for (int i = 0; i < myxmlfile.Count; i++)
            {
                xmlfile += myxmlfile[i].Name;
            }
            return xmlfile;
        }

        internal ObservableCollection<DataOutput> ReadXmlSerialOneOut(int ioutType)
        {
            return MyXml.ReadXmlStringOut(ioutType);
        }

        internal void PrintBale(int indx)
        {
            string BalePrintOutString = GetXmlfile(2);
            string itemsList = string.Empty;
            string strquery;

            if (_cryBaleReport != null) _cryBaleReport = null;
            _cryBaleReport = new CrystalReport();

            if (!BalePrintOutString.Contains(","))
            {
                foreach (var Item in BalePrintOutString)
                {
                    itemsList += Item + ",";
                }
            }
            else itemsList = BalePrintOutString + ",[index]" ;

            if(indx==0)
                strquery = "SELECT TOP 1 " + itemsList + " From " + GetCurrentArchivesTable() + " ORDER by [index] DESC ";
            else
                strquery = "SELECT TOP 1 " + itemsList + " From " + GetCurrentArchivesTable() + " WHERE SerialNumber = " + indx.ToString() + ";";
            using (DataTable CryRepTable = GetRepTable(strquery))
            {
                if(CryRepTable.Rows.Count > 0)
                {
                    CryRepTable.Columns.Remove("index");
                    _cryBaleReport.UpdateRepTable(CryRepTable);
                    _cryBaleReport.Refresh();
                    _cryBaleReport.SetReportTitle("Bale Report");
                    //_cryBaleReport.PrintReport();
                }
            }
        }

        /// <summary>
        /// Reset S/N Day end or end of month
        /// </summary>
        internal void SetStartUpSerialNumber()
        {
            SetTimeandEvents(DateTime.Now);

            if (Settings.Default.LastShutDown.DayOfYear < DateTime.Now.DayOfYear)
            {
                if (Settings.Default.LastShutDown.Hour >= Settings.Default.DayEndTime.Hour)
                {
                    DayEnded = true;
                    Settings.Default.LastShutDown = DateTime.Now;
                    Settings.Default.Save();
                }
            }
            else if (DateTime.Now.DayOfYear == 1) //Same as from the day before.
            {
                if (Settings.Default.LastShutDown.Hour >= Settings.Default.DayEndTime.Hour)
                {
                    MonthEnded = true;
                    Settings.Default.LastShutDown = DateTime.Now;
                    Settings.Default.Save();
                }
            }
            else
            {
                ClassCommon.SerialNumber = (GetPreSerialNumber() + 1).ToString();
            }
            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.APPSTART, "SetStartUpSerialNumber Done ....................");
        }

        internal void DeleteSingleStockRecord(string stockSelectValue)
        {
            SqlHandler.DeleteSingleStockRecord(stockSelectValue);
        }


        /// <summary>
        /// Setup Report Table
        /// </summary>
        /// <param name="prnId"></param>
        /// <returns></returns>
        private bool SetUpRepTable(int prnId)
        {
            SetTimeandEvents(DateTime.Now);

            string strTimeFrame;
            bool bAOK = false;
            char charsToTrim = ',';
            string archTable = GetCurrentArchivesTable();

            switch (prnId)
            {
                case (int)ClassCommon.PrntEvents.DayEnd:
                    strTimeFrame = " WHERE (TimeComplete BETWEEN '" + ClassCommon.StartDate + "' and '" + ClassCommon.DayEndDate + "') ";
                    ReportTitle = "Forté Day End Report ";
                    break;

                case (int)ClassCommon.PrntEvents.MonthEnd:
                    strTimeFrame = string.Empty;
                    ReportTitle = "Forté Month Report";
                    break;

                case (int)ClassCommon.PrntEvents.ShiftOne:
                    strTimeFrame = " WHERE (TimeComplete BETWEEN '" + ClassCommon.DayEndDate + "' and '" + ClassCommon.ShiftOneDate + "') ";
                    ReportTitle = "Forté Shift One Report";
                    break;

                case (int)ClassCommon.PrntEvents.ShiftTwo:
                    strTimeFrame = " WHERE (TimeComplete BETWEEN '" + ClassCommon.ShiftOneDate + "' and '" + ClassCommon.ShiftTwoDate + "') ";
                    ReportTitle = "Forté Shift Two Report";
                    break;

                case (int)ClassCommon.PrntEvents.ShiftThree:
                    strTimeFrame = " WHERE (TimeComplete BETWEEN '" + ClassCommon.ShiftTwoDate + "' and '" + ClassCommon.ShiftThreeDate + "') ";
                    ReportTitle = "Forté Shift Three Report";
                    break;

                case (int)ClassCommon.PrntEvents.LotClose:
                    strTimeFrame = " WHERE (TimeComplete BETWEEN '" + ClassCommon.ShiftTwoDate + "' and '" + ClassCommon.ShiftThreeDate + "') ";
                    ReportTitle = "Forté Lot Report";
                    break;

                default:
                    strTimeFrame = " WHERE (TimeComplete BETWEEN '" + ClassCommon.StartDate + "' and '" + ClassCommon.DayEndDate + "') ";
                    break;
            }
            try
            {
                string itemsList = string.Empty;
                string CheckItems = _cryReport.GetItemsString();

                CheckItems = CheckItems.TrimEnd(charsToTrim);
                SelectedHdrList = GetXmlSelectedHdrCheckedList();

                foreach (var Item in SelectedHdrList)
                {
                    itemsList += Item + ",";
                }
                itemsList = itemsList.TrimEnd(charsToTrim);
                string strquery = "SELECT " + CheckItems + " From " + archTable + strTimeFrame + " ORDER by [index] ASC ";

                using (DataTable CryRepTable = GetRepTable(strquery))
                {
                    _cryReport.UpdateRepTable(CryRepTable);
                    _cryReport.Refresh();
                    if (CryRepTable.Rows.Count > 0) bAOK = true;
                    else ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.REPORTS, "No Record found! @ " + DateTime.Now);
                }
                _cryReport.Refresh();
                _cryReport.SetReportTitle(ReportTitle);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error in SetUpRepTable " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.REPORTS, "Error in SetUpRepTable " + ex.Message);
            }
            return bAOK;
        }

        internal string GetNextSerialNumber()
        {
            return (Convert.ToInt32(ClassCommon.SerialNumber) + 1).ToString();
        }

        internal bool PrintSelectReport(int selectedMode)
        {
            bool aok = false;
            try
            {
                if(SetUpRepTable(selectedMode))
                {
                    _ = _cryReport.PrintReportAsync();
                    aok = true;
                    Console.WriteLine("Print Report = " + selectedMode.ToString());
                }
            }
            catch (Exception ex )
            {
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.REPORTS, "Error in PrintSelectReport " + ex.Message);
            }
            return aok;
        }

        internal bool ShowReportDialog(int iMode)
        {
            bool bshowOK = false;
            if (_cryReport != null) _cryReport = null;
            using (_cryReport = new CrystalReport())
            {
                if(SetUpRepTable(iMode))
                {
                    _cryReport.Refresh();
                    _cryReport.ShowDialog();
                    bshowOK = true;
                }
            }
            return bshowOK;
        }

        internal void StopLotEventTimer()
        {
            _eventsTimer.StopAllEventsTimers();
        }

        
    }
}
