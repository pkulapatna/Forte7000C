using Forte7000E.Module.LotProcess.Properties;
using Forte7000E.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using Prism.Events;
using System.Linq;

namespace Forte7000E.Module.LotProcess.Models
{
    public class LotProcessModel 
    {
        protected readonly Prism.Events.IEventAggregator _eventAggregator;
        public LotData _lotData;

        public bool BLotTimer { get; set; }
        public int CurrentLotNumber { get; set; }
        public int CurrentLotBaleNumber { get; set; }
        public int NextLotNumber { get; set; }
        public DataTable OpenLotTable { get; set; }
        public DataTable CloseLotTable { get; set; }
        //public DataTable LotTaransTable { get; set; }
        //public int ILotNumberStatus { get; set; }


        public int LotType 
        { 
            get => Settings.Default.LotType;
            set 
            {
                Settings.Default.LotType = value;
                Settings.Default.Save();
            } 
        }
        public int OpenLot 
        { 
            get => Settings.Default.OpenLot;
            set
            {
                Settings.Default.OpenLot = value;
                Settings.Default.Save();
            }
        }
        public int LotSequence 
        { 
            get => Settings.Default.LotSequence;
            set 
            {
                Settings.Default.LotSequence = value;
                Settings.Default.Save();
            } 
        }
        public int LotClose 
        { 
            get => Settings.Default.LotClose;
            set
            {
                Settings.Default.LotClose = value;
                Settings.Default.Save();
            }
        }
        public int LotReset 
        { 
            get => Settings.Default.LotReset;
            set
            {
                Settings.Default.LotReset = value;
                Settings.Default.Save();
            }
        }


        public List<string> LotStatus;
        public LotEvents _lotevents;

        private double LotDiviation;
        private double LotStd;
        private double LotVariance;

        private readonly List<double> WtLotList = new List<double>();
        private double WtLotStd = 0;
        private double WtLotVariance = 0;
        private double WtLotAverage = 0;
        private double WtLotLotDiviation = 0;
        private double WtSumDerivation = 0;

        private readonly List<double> MCLotList = new List<double>();
        private double MCLotStd = 0;
        private double MCLotVariance = 0;
        private double MCLotAverage = 0;
        private double MCLotLotDiviation = 0;
        private double MCSumDerivation = 0;

        private const int SingleLot = 0;
        private const int MultipleLot = 1;
        private const int LCRollOver = 0;
        private const int LCDayEnd = 1;
        private const int LCMonthEnd = 2;
        private const int LCLotSize = 3;

        private readonly List<double> BaleWeightLst;
        private readonly List<double> BaleMoistureLst;
        private double CurrBaleWeight;
        private double CurrBaleMoisture;

        public enum LotEvents
        {
            idle,
            OpenLot,
            NewBaleArrive,
            CloseLot,
            CloseResetLot,
            IncrementLotNum,
            IncBaleinLotNum,
            ResetLotNum,
            ClearOpenLot,
            ClearCloseLot,
            UpdateLotArchiveTable,
            ComputeOpenLotArchiveData,
            UpdateOpenLotArchiveTable
        }

       
        internal DataTable GetSqlLotTable(int Lotid)
        {
            DataTable Lottable = new DataTable();
            string fileLoc = ClassCommon._lotHdrXmlLoclst[Lotid].ToString();
          
            try
            {
                List<string> lotitems = Xmlhandler.ReadXmlGridView(fileLoc);

                if (lotitems.Count > 0)
                {
                    string strquery = GetLotQueryStrFromList(lotitems, Lotid);
                    Lottable = SqlHandler.GetLotArchiveTable(strquery);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in GetSqlLotTable " + ex.Message);
            }
            return Lottable;
        }


        internal DataTable GetCustomLotArchiveTable(int closeLot)
        {
            return SqlHandler.GetLotDataTable((int)ClassCommon.InstanceType.CloseLot);
        }


        public Sqlhandler SqlHandler { get; set; }
        public Xmlhandler Xmlhandler { get; set; }

        public LotProcessModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;

            LotStatus = new List<string>
            {
                "OpenLot",
                "ClosedLot"
            };

            BaleWeightLst = new List<double>();
            BaleMoistureLst = new List<double>();

            Xmlhandler = Xmlhandler.Instance; 
            SqlHandler = Sqlhandler.Instance; 
                                        
            _lotData = new LotData();
            BLotTimer = false;

            _eventAggregator.GetEvent<UpdateAllEvent>().Subscribe(TimeEventActions);

            CloseLotTable = GetCustomLotArchiveTable((int)ClassCommon.InstanceType.CloseLot);
            
            ClassCommon.LotBaleNumber = GetPreLotNumber("LotBaleNumber");   
        }

       
        private void TimeEventActions(int obj)
        {
            switch (obj)
            {
                case (int)ClassCommon.TimeEvents.dayend:

                    if((ClassCommon.BCloseByTime) & (Settings.Default.LpResetMode == LCDayEnd))
                    {
                        CloseOpenLot();
                        ClassCommon.BCloseByTime = true;
                    }  
                    Console.WriteLine("Day Ended");
                    break;

                case (int)ClassCommon.TimeEvents.monthend:

                    if ((ClassCommon.BCloseByTime) & (Settings.Default.LpResetMode == LCMonthEnd))
                    {
                        CloseOpenLot();
                        ClassCommon.BCloseByTime = true;
                    }
                    Console.WriteLine("Month Ended");
                    break;

                case (int)ClassCommon.TimeEvents.shiftone:
                    Console.WriteLine("Shift One");
                    break;

                case (int)ClassCommon.TimeEvents.shifttwo:
                    Console.WriteLine("Shift Two");
                    break;

                case (int)ClassCommon.TimeEvents.shiftthree:
                    Console.WriteLine("Shift Three");
                    break;
                default:
                    break;
            }
        }

        public DataTable GetOpenLotsTable(int Lotid)
        {
            DataTable Lottable = new DataTable();
            string fileLoc = ClassCommon._lotHdrXmlLoclst[Lotid].ToString();
            
            try
            {
                List<string> lotitems = Xmlhandler.ReadXmlGridView(fileLoc);
                if (lotitems.Count > 0)
                {
                    string strquery = GetLotQueryStrFromList(lotitems, Lotid);
                    Lottable = SqlHandler.GetOpenLotTable(strquery);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in GetSqlLotTable " + ex.Message);
            }
            return Lottable;
        }


        private string GetLotQueryStrFromList(List<string> lotitems, int LotType)
        {
            string lottype;
            char charsToTrim = ',';
            string strItems = string.Empty;
            string quanLoc = string.Empty;
            /*
            if (LotType == (int)ClassCommon.InstanceType.OpenLot)
            {
                lottype = " WHERE  LotStatus = 'Open' ";
                quanLoc = " TOP 1 ";
            }
            else if (LotType == (int)ClassCommon.InstanceType.CloseLot)
            {
                lottype = " WHERE  LotStatus = 'Closed' ";
            }
            else
            {
                lottype = string.Empty;
            }
              */
            quanLoc = " TOP 1 ";
            lottype = string.Empty;

            foreach (var Item in lotitems)
            {
                strItems += Item + ",";
            }
            return "SELECT " + quanLoc + strItems.TrimEnd(charsToTrim) + " from " + Sqlhandler.SQLOPENLOTSTABLE + lottype + " ORDER BY UID DESC;";
        }

        /// <summary>
        /// Need Work!
        /// </summary>
        /// <param name="lotClose"></param>
        /// <returns></returns>
        internal bool ShowReportDialog(int lotClose)
        {
            bool bshowOK = false;
           
            return bshowOK;
        }

        public void InitLotProcess()
        {
            ChangeLotState(LotEvents.idle);

      //      _ = QuartzSch.CloseLot();
         
        }

        public void ChangeLotState(LotEvents _state)
        {
            _lotevents = _state;

            switch (_state)
            {
                case LotEvents.idle:
                    
                    break;

                case LotEvents.NewBaleArrive: //$LOT
                    ProcessNewBale();
                    break;

                case LotEvents.OpenLot:

                    OpenNewLot();
                    break;

                case LotEvents.CloseLot:

                    ClosePreviousLot();
                    Console.WriteLine(" Closed CurrBaleWeight= " + CurrBaleWeight.ToString("#000.00"));
                    BaleWeightLst.Clear();

                    break;

                case LotEvents.CloseResetLot:

                    CloseAndResetLotNumber();
                    break;

                case LotEvents.IncrementLotNum:

                    break;

                case LotEvents.IncBaleinLotNum:

                    break;
                case LotEvents.ResetLotNum:

                    break;

                case LotEvents.ClearOpenLot:

                    break;

                case LotEvents.ClearCloseLot:

                    break;

                case LotEvents.UpdateLotArchiveTable:
                    ComputeLotArchiveData();
                    if (SqlHandler.UpdateLotTableAsy(true, SqlHandler.GetCurrentLotArchivesTable()))
                    {
                        _eventAggregator.GetEvent<UpdateLotData>().Publish((int)ClassCommon.InstanceType.CloseLot);
                        Console.WriteLine("Update LotArchive table");
                    }
                    break;

                default:

                    break;
            }
        }

        /// <summary>
        /// Check for LotArchive Table
        /// Update LotArchive Table
        /// </summary>
        private void ComputeLotArchiveData()
        {
            List<double> wtList = new List<double>();
            List<double> mcList = new List<double>();
            double sumOfDerivation = 0;
            DataTable TempTable = new DataTable();

            switch (Settings.Default.LpResetMode)
            {
                case LCRollOver:

                    break;

                case LCDayEnd:

                    break;

                case LCMonthEnd:

                    break;

                case LCLotSize:
                    TempTable = SqlHandler.GetDatabyLotNumber(ClassCommon.LotNumber, (long)ClassCommon.BaleInLot);
                    break;

                default:

                    break;
            }

            if(TempTable.Rows.Count > 0)
            {
                for(int i =0; i < TempTable.Rows.Count; i++ )
                {
                    wtList.Add(TempTable.Rows[i].Field<Single>("Weight"));
                    mcList.Add(TempTable.Rows[i].Field<Single>("Moisture"));
                }
            }
        
            if(wtList.Count > 0)
            {
                ClassCommon.TotalNetWeight = wtList.Sum();
                ClassCommon.MinNetWeight = wtList.Min();
                ClassCommon.MaxNetWeight = wtList.Max();
            }

            if (mcList.Count > 0)
            {
                ClassCommon.TotalMC = mcList.Sum();
                ClassCommon.MaxMC = mcList.Max();
                ClassCommon.MinMC = mcList.Min();

                double lotBaleAvg = mcList.Average();

                foreach (var value in mcList)
                {
                    sumOfDerivation += (value - lotBaleAvg) * (value - lotBaleAvg);
                }
                //std
                LotVariance = sumOfDerivation / (mcList.Count - 1);
                LotStd = Math.Sqrt(LotVariance);
                LotDiviation = (LotStd / lotBaleAvg) * 100;

                //bool bDone = SqlHandler.UpdateLotArchiveTable();
            }
        }
        
        public void CloseAndResetLotNumber()
        {
            ClassCommon.BLotReset = true;
            ClassCommon.LotStatus = "LotReset";
            ClassCommon.CloseTD = DateTime.Now;
            ClassCommon.NextLotNumber = ClassCommon.LotNumber + 1;
        }

        private void ClosePreviousLot()
        {
            ClassCommon.BLotClose = true;
            ClassCommon.CloseTD = DateTime.Now;
            ClassCommon.LotStatus = "Closed";

            ClassCommon.OpenTD = DateTime.Now;
        }

        private void OpenNewLot()
        {
            ClassCommon.LotNumber += 1;
            ClassCommon.LotBaleNumber = 1;
            ClassCommon.LotStatus = "Open";
            ClassCommon.OpenTD = DateTime.Now;
            ClassCommon.NextLotNumber = ClassCommon.LotNumber + 1;
        }

        /// <summary>
        /// 1. Get current LotNumber (CurrentLotNumber)
        /// 2. Get current LotBaleNumber (CurrentLotBaleNumber)
        /// </summary>
        /// <returns>
        /// 3. return current LotNumber + 1 (GetNextLotNumber)
        /// </returns>
        public int GetNextLotNumber()
        {
            try
            {
                using (DataTable Archtable = GetArchiveTable())
                {
                    CurrentLotNumber = Archtable.Rows[0].Field<int>("LotNumber");
                    CurrentLotBaleNumber = Archtable.Rows[0].Field<int>("LotBaleNumber");

                    ClassCommon.LotNumber = CurrentLotNumber;

                   // CurrBaleWeight = Archtable.Rows[0].Field<Single>("Weight");
                   // CurrBaleMoisture = Archtable.Rows[0].Field<Single>("Moisture");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in GetNextLotNumber " + ex.Message);
            }
            return CurrentLotNumber + 1;
        }


        //$LOT
        /// <summary>
        /// At this time Archive table have been updated with new bale data
        /// Get current LotNumber
        /// Get current LotBaleNumber
        /// </summary>
        private void ProcessNewBale()
        {  
            try
            {
                GetNextLotNumber();  

                if (ClassCommon.BLotReset)
                {
                    ClassCommon.LotNumber = 1;
                    ClassCommon.LotStatus = "Open";
                    ClassCommon.LotBaleNumber = 0;
                    ClassCommon.BLotReset = false;
                }
                else if (ClassCommon.BLotClose)
                {
                    ClassCommon.LotNumber = CurrentLotNumber +1;
                    ClassCommon.LotStatus = "Open";
                    ClassCommon.LotBaleNumber = 0;
                    ClassCommon.BLotClose = false;
                }
                else
                {
                    ClassCommon.LotNumber = CurrentLotNumber;
                    CurrBaleWeight += ClassCommon.DOrigWeight;

                    BaleWeightLst.Add(ClassCommon.DOrigWeight);
                    CurrBaleMoisture = ClassCommon.DMoisture;

                    Console.WriteLine ("ClassCommon.LotStatus = " + ClassCommon.LotStatus);

                    //Console.WriteLine("CurrBaleWeight= " + CurrBaleWeight.ToString("#000.00"));
                    // Console.WriteLine("CurrBaleMoisture= " + CurrBaleMoisture.ToString("#000.00"));
                }

                ClassCommon.NextLotNumber = ClassCommon.LotNumber + 1;

                switch (Settings.Default.LotType)
                {
                    case SingleLot:
                     //   if (CurrentLotBaleNumber == 0) ClassCommon.LotBaleNumber = 1;
                     //   else ClassCommon.LotBaleNumber += 1;

                        SetNewLotAndBaleNumber();
                          
                        break;

                    case MultipleLot:

                        break;

                    default:
                        if (CurrentLotBaleNumber == 0) ClassCommon.LotBaleNumber = 1;
                        else ClassCommon.LotBaleNumber += 1;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in ProcessNewBale " + ex.Message);
            }
        }

        internal void CloseOpenLot()
        {
            ChangeLotState(LotEvents.CloseLot);
        }

        internal void CloseResetLot()
        {
            ChangeLotState(LotEvents.CloseResetLot);
        }

        private bool SetNewLotAndBaleNumber()
        {
            bool iRet = false;

            if (CurrentLotBaleNumber == 0) ClassCommon.LotBaleNumber = 1;
            else ClassCommon.LotBaleNumber += 1;

            switch (Settings.Default.LpResetMode)
            {
                case LCRollOver:

                    if (ClassCommon.LotBaleNumber >= Settings.Default.iLotMax)
                    {
                        ClassCommon.BaleCount = Settings.Default.iLotMax;
                        iRet = true;
                        ChangeLotState(LotEvents.CloseLot);
                    }
                    else if (ClassCommon.LotBaleNumber > Settings.Default.iLotMax)
                    {
                        ChangeLotState(LotEvents.OpenLot);
                    }

                    break;

                case LCDayEnd:

                    ClassCommon.BCloseByTime = true;

                    break;

                case LCMonthEnd:

                    ClassCommon.BCloseByTime = true;

                    break;

                case LCLotSize:

                    if (ClassCommon.LotBaleNumber == Settings.Default.iCustomLotSize)
                    {
                        ClassCommon.BCloseBySize = true;
                        ClassCommon.BaleCount = Settings.Default.iCustomLotSize;
                        iRet = true;
                        ClassCommon.BaleInLot = Settings.Default.iCustomLotSize;
                        ChangeLotState(LotEvents.CloseLot);
                    }
                    else if (ClassCommon.LotBaleNumber > Settings.Default.iCustomLotSize)
                    {
                        ChangeLotState(LotEvents.OpenLot);
                    }
                    break;

                default:

                    break;
            }
            return iRet;
        }

        private DataTable GetArchiveTable()
        {
            string strquery = "SELECT top 2 * from " + SqlHandler.GetCurrentArchivesTable() + " ORDER BY UID DESC;";
            DataTable Mytable = SqlHandler.GetForteDataTable(strquery);
            return Mytable;
        }

        private int GetPreLotNumber(string fieldName)
        {
            int curSerNum = 0;
           // DataTable MyPreDataTab = new DataTable();

            string strquery = "SELECT top 2 * from " + SqlHandler.GetCurrentArchivesTable() + " ORDER BY UID DESC;";
            DataTable MyPreDataTab = SqlHandler.GetForteDataTable(strquery);

            if (MyPreDataTab.Rows.Count > 0)
            {
                curSerNum = MyPreDataTab.Rows[0].Field<int>(fieldName);
            }
            return curSerNum;
        }


        /// <summary>
        /// Update LotTrainsition Table
        /// Accumulate newest openlot data from LotTransition table. 
        /// Start from 1st Bale to the the last.  
        /// </summary>
        public void UpdateBaleLotData()
        {
            try
            {
                //Update LotTransaction Table
               SqlHandler.UpdateLotTableAsy(true, Sqlhandler.SQLLOTTRANSTABLE);

                DataTable LotTransTable = SqlHandler.GetLotTransTable();
                if (LotTransTable.Rows.Count > 50)
                    SqlHandler.DeleteLotTransRows();




                //Update OpenLot Table
                UpdateOpenLotTable(LotTransTable);
                //Tell UI
                _eventAggregator.GetEvent<UpdateLotData>().Publish((int)ClassCommon.InstanceType.OpenLot);


            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR  in UpdateBaleLotData " + ex.Message);
            }
        }

        private void UpdateOpenLotTable(DataTable LotTransTable)
        {
            string strvalues = "(";
            char[] charsToTrim = { ',' };
           
           // Console.WriteLine("1----------Lot number = " + LotTransTable.Rows[0]["LotNum"].ToString());
          //  Console.WriteLine("2----------Lot Status = " + LotTransTable.Rows[0]["LotStatus"].ToString());
          //  Console.WriteLine("3----------Lot TotNW  = " + ClassCommon.DNetWeight.ToString("#0.00")); // LotTransTable.Rows[0]["TotNW"].ToString());
          //  Console.WriteLine("4----------Lot TotMC  = " + LotTransTable.Rows[0]["TotMC"].ToString());
          //  Console.WriteLine("5----------Lot Bale Count  = " + LotTransTable.Rows[0]["BaleCount"].ToString());

            List<ArchiveData> LotFieldsList = SqlHandler.m_LotOpenFieldsList;
            
            if(LotTransTable.Rows[0]["BaleCount"].ToString() == "1")
            {
                WtLotList.Clear();
                WtLotList.Add(ClassCommon.DNetWeight);
                MCLotList.Clear();
                MCLotList.Add(ClassCommon.DMoisture);
            }
            else
            {
                WtLotList.Add(ClassCommon.DNetWeight);
                WtLotAverage = WtLotList.Average();
                foreach (var value in WtLotList)
                {
                    WtSumDerivation  += (value - WtLotAverage) * (value - WtLotAverage);
                }
                //std
                WtLotVariance = WtSumDerivation / (WtLotList.Count - 1);
                WtLotStd = Math.Sqrt(WtLotVariance);
                WtLotLotDiviation = (WtLotStd / WtLotAverage) * 100;

                if (WtLotAverage.Equals(double.NaN)) WtLotAverage = 0;
                if (WtLotVariance.Equals(double.NaN)) WtLotVariance = 0;
                if (WtLotStd.Equals(double.NaN)) WtLotStd = 0;
                if (WtLotLotDiviation.Equals(double.NaN)) WtLotLotDiviation = 0;

                MCLotList.Add(ClassCommon.DMoisture);
                MCLotAverage = MCLotList.Average();
                foreach (var value in MCLotList)
                {
                    MCSumDerivation += (value - MCLotAverage) * (value - MCLotAverage);
                }
                //std
                MCLotVariance = MCSumDerivation / (MCLotList.Count - 1);
                MCLotStd = Math.Sqrt(MCLotVariance);
                MCLotLotDiviation = (MCLotStd / MCLotAverage) * 100;

                if (MCLotAverage.Equals(double.NaN)) MCLotAverage = 0;
                if (MCLotVariance.Equals(double.NaN)) MCLotVariance = 0;
                if (MCLotStd.Equals(double.NaN)) MCLotStd = 0;
                if (MCLotLotDiviation.Equals(double.NaN)) MCLotLotDiviation = 0;

            }
            try
            {
                if(LotTransTable.Rows.Count > 0 )
                {
                    int i = 0;

                    for (int x = 0; x < LotFieldsList.Count; x++)
                    {
                        switch (LotFieldsList[x].Datatype)
                        {
                            case "int":

                                if (LotFieldsList[x].Name == "Index")
                                    LotFieldsList[x].Value = ClassCommon.IIndex.ToString();
                                else if (LotFieldsList[x].Name == "LotNum")
                                    LotFieldsList[x].Value = ClassCommon.LotNumber.ToString();
                                else if (LotFieldsList[x].Name == "JobNum")
                                    LotFieldsList[x].Value = ClassCommon.JobNumber.ToString();
                                else if (LotFieldsList[x].Name == "BaleCount")
                                    LotFieldsList[x].Value = ClassCommon.BaleCount.ToString();
                                else if (LotFieldsList[x].Name == "MinNWBale")
                                    LotFieldsList[x].Value = ClassCommon.MinNetWtBale.ToString();
                                else if (LotFieldsList[x].Name == "MaxNWBale")
                                    LotFieldsList[x].Value = ClassCommon.MaxNetWtBale.ToString();
                                else if (LotFieldsList[x].Name == "MinMCBale")
                                    LotFieldsList[x].Value = ClassCommon.MinMCBale.ToString();
                                else if (LotFieldsList[x].Name == "MaxMCBale")
                                    LotFieldsList[x].Value = ClassCommon.MaxMCBale.ToString();
                                else if (LotFieldsList[x].Name == "NextBaleNumber")
                                    LotFieldsList[x].Value = ClassCommon.NextBaleNum.ToString();
                                else if (LotFieldsList[x].Name == "UID")
                                    LotFieldsList[x].Value = ClassCommon.UID.ToString();
                                else if (LotFieldsList[x].Name == "QualityUID")
                                    LotFieldsList[x].Value = ClassCommon.QualityUID.ToString();
                                else if (LotFieldsList[x].Name == "BaleCount")
                                    LotFieldsList[x].Value = ClassCommon.LotBaleNumber.ToString();
                                else
                                    LotFieldsList[x].Value = string.Empty;
                                break;

                            case "nvarchar":

                                if (ClassCommon.LotStatus != null && LotFieldsList[x].Name == "LotStatus")
                                    LotFieldsList[x].Value = "'" + ClassCommon.LotStatus.ToString() + "'";
                                else if (LotFieldsList[x].Name == "StockName")
                                    LotFieldsList[x].Value = "'" + ClassCommon.StockName.ToString() + "'";
                                else if (LotFieldsList[x].Name == "MonthCode")
                                    LotFieldsList[x].Value = "'" + ClassCommon.MonthCode.ToString() + "'";
                                else
                                    LotFieldsList[x].Value = "'" + string.Empty + "'";
                                break;

                            case "bit":

                                if (LotFieldsList[x].Name == "CloseBySize")
                                    LotFieldsList[x].Value = "'" + ClassCommon.BCloseBySize.ToString() + "'";
                                else if (LotFieldsList[x].Name == "CloseByTime")
                                    LotFieldsList[x].Value = "'" + ClassCommon.BCloseByTime.ToString() + "'";
                                else
                                    LotFieldsList[x].Value = "'0'";
                                break;

                            case "real":
                            case "float":

                                if (LotFieldsList[x].Name == "TotNW")
                                    LotFieldsList[x].Value = WtLotList.Sum().ToString("#00.00"); 
                                                          // ClassCommon.TotalNetWeight.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "MinNW")
                                    LotFieldsList[x].Value = WtLotList.Min().ToString("#00.00");  
                                                            //ClassCommon.MinNetWeight.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "MaxNW")
                                    LotFieldsList[x].Value = WtLotList.Max().ToString("#00.00");  
                                                            //ClassCommon.MaxNetWeight.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "NW2N")
                                    LotFieldsList[x].Value = ClassCommon.NetWeightSquare.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "MeanNW")
                                    LotFieldsList[x].Value = WtLotAverage.ToString("#00.00"); 
                                                          // ClassCommon.MeanNetWeight.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "RangeNW")
                                    LotFieldsList[x].Value = ClassCommon.RangeNetWeight.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "StdDevNW")
                                    LotFieldsList[x].Value =  WtLotLotDiviation.ToString("#00.00"); 
                                                            //ClassCommon.StdNetWeight.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "TotTW")
                                    LotFieldsList[x].Value = ClassCommon.TotalTareWeight.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "TotBW")
                                    LotFieldsList[x].Value = ClassCommon.TotalBdWeight.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "TotMC")
                                    LotFieldsList[x].Value = ClassCommon.TotalMC.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "MinMC") //MCLotList
                                    LotFieldsList[x].Value = MCLotList.Min().ToString("#00.00"); 
                                                            //ClassCommon.MinMC.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "MaxMC")
                                    LotFieldsList[x].Value = MCLotList.Max().ToString("#00.00");  
                                                            //ClassCommon.MaxMC.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "MC2M")
                                    LotFieldsList[x].Value = ClassCommon.McSquare.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "RangeMC")
                                    LotFieldsList[x].Value = ClassCommon.RangeMC.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "StdDevMC")
                                    LotFieldsList[x].Value = ClassCommon.StdMC.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "BalesAssigned")
                                    LotFieldsList[x].Value = ClassCommon.BaleAssigned.ToString("#00.00");

                                else if (LotFieldsList[x].Name == "ClosingSize")
                                    LotFieldsList[x].Value = ClassCommon.ClosingSize.ToString();
                                else
                                    LotFieldsList[x].Value = "0";
                                break;

                            case "smallint":
                                if (LotFieldsList[x].Name == "BaleCount")
                                    LotFieldsList[x].Value = ClassCommon.LotBaleNumber.ToString();
                                else
                                    LotFieldsList[x].Value = "0";
                                break;

                            case "datetime":

                                // Console.WriteLine("LotFieldsList[x].Name =" + LotFieldsList[x].Name + " - "+ ClassCommon.OpenTD.ToString("MM/dd/yyyy H:mm"));

                                if (ClassCommon.OpenTD != null && LotFieldsList[x].Name == "OpenTD")
                                    LotFieldsList[x].Value = "'" + ClassCommon.OpenTD.ToString("MM/dd/yyyy H:mm") + "'";
                                else if (ClassCommon.CloseTD != null && LotFieldsList[x].Name == "CloseTD")
                                    LotFieldsList[x].Value = "'" + ClassCommon.CloseTD.ToString("MM/dd/yyyy H:mm") + "'";
                                else
                                    LotFieldsList[x].Value = "'" + DateTime.Now.ToString("MM/dd/yyyy H:mm") + "'";

                                break;

                            default:
                                break;
                        }
                        strvalues += LotFieldsList[x].Value.ToString() + ",";
                    }
                    strvalues = strvalues.Trim(charsToTrim);
                    strvalues += ")";

                    string strQuery = string.Empty;

                    foreach (var item in LotFieldsList)
                    {
                        strQuery += "[" + LotFieldsList[i].Name + "],";
                        i++;
                    }
                    strQuery = strQuery.Trim(charsToTrim);
                    strQuery = "INSERT INTO LotOpen (" + strQuery + ") VALUES " + strvalues + ";";

                    SqlHandler.UpdateLotTransitTableAsy(strQuery, "LotOpen");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR  in UpdateOpenLotTable " + ex.Message);
            }
        }

        private void InitLotVars()
        {
            ClassCommon.IIndex = 0;
            ClassCommon.LotNumber = 0;      
            ClassCommon.JobNumber = 0;
            ClassCommon.BaleCount = 0;
            ClassCommon.MinNetWtBale = 0;
            ClassCommon.MaxNetWtBale = 0;
            ClassCommon.MinMCBale = 0;
            ClassCommon.MaxMCBale = 0;
            ClassCommon.NextBaleNum = 1;
            ClassCommon.LotStatus = "0";
            ClassCommon.BCloseBySize = false;
            ClassCommon.BCloseByTime = false;
            ClassCommon.TotalNetWeight = 0;
            ClassCommon.MinNetWeight = 0;
            ClassCommon.MaxNetWeight = 0;
            ClassCommon.NetWeightSquare = 0;
            ClassCommon.MeanNetWeight = 0;
            ClassCommon.RangeNetWeight = 0;
            ClassCommon.StdNetWeight = 0;
            ClassCommon.TotalTareWeight = 0;
            ClassCommon.TotalBdWeight = 0;
            ClassCommon.TotalMC = 0;
            ClassCommon.MinMC = 0;
            ClassCommon.MaxMC = 0;
            ClassCommon.McSquare = 0;
            ClassCommon.RangeMC = 0;
            ClassCommon.StdMC = 0;
            ClassCommon.BaleAssigned = 0;
            ClassCommon.ClosingSize = 0;
            ClassCommon.OpenTD = DateTime.Now;
            ClassCommon.CloseTD = DateTime.Now;
            ClassCommon.MonthCode = string.Empty;
            ClassCommon.NextLotNumber = 1;
            ClassCommon.LotBaleNumber = 0;
            ClassCommon.LotNumber = 0;
            ClassCommon.BLotReset = false;
            ClassCommon.BLotClose = false;
            ClassCommon.QualityUID = 0;

        }

       
    }
}
