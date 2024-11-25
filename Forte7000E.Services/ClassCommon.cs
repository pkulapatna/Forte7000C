using Forte7000E.Services.Properties;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forte7000E.Services
{
    public static class ClassCommon
    {
        public static int DevScale = 0;
        public static int DevOsc = 1;

        public static ClsErrorLog.ClsErrorLog MyInfoLog = new ClsErrorLog.ClsErrorLog();  
        public static bool BUserLogon { get; set; }

        public static Double DAConst { get; set; }
        public static Double DBConst { get; set; }
        public static Double DCConst { get; set; }
        public static Double DtrCal { get; set; }
        public static Double DUpCount { get; set; }
        public static Double DDCount { get; set; }
        public static Double Dforte { get; set; }
        public static Double DRefernce { get; set; }
        public static Double DReading { get; set; }
        public static Double DCalcForte { get; set; }
        public static Double DCalcForte1 { get; set; }
        public static Double DCalcForte2 { get; set; }

       //private const string V = "outSerialOne";

        public static DateTime _timestamp = DateTime.Now;
        public static DateTime DayEnd { get; set; }
        public static DateTime ShiftOne { get; set; }
        public static DateTime ShiftTwo { get; set; }
        public static DateTime ShiftThree { get; set; }

        //Moisture
        public static Double DMoisture { get; set; }
        public static Double RealMoisture { get; set; }

        public static string MoistMes { get; set; }

        //Weight
        public static Double DScaleWeight { get; set; }
        public static Double DTareWeight { get; set; }
        public static Double DOrigWeight { get; set; }
        public static Double DNetWeight { get; set; }
        public static string WtMes { get; set; }
        public static string WtUnit { get; set; }
        public static bool BDefaultWeight { get; set; }
        public static int IForteRange { get; set; }
        public static int OscReadTimeout { get; set; }
        public static string OscMode { get; set; }


        public static string ScaleMode { get; set; }
        public static Double DsimScaleWeight { get; set; }
        public static bool BSimWtRandom { get; set; }

        public static string SerialNumber { get; set; }
        public static string NewSerialNumber { get; set; }

        public static int IIndex { get; set; }
        public static int UID { get; set; }

        public static int QualityUID { get; set; }

        public static string CurrStock { get; set; }

        /// <summary>
        /// Lot ---------------------------------------------------------------
        /// </summary>
        /// 
        public static long BaleInLot { get; set; } 
        public static int LotNumber { get; set; }
        public static int JobNumber { get; set; }
        public static long BaleCount { get; set; }
        public static double MinNetWtBale { get; set; }
        public static double MaxNetWtBale { get; set; }
        public static int MinMCBale { get; set; }
        public static int MaxMCBale { get; set; }
        public static int NextBaleNum { get; set; }
       
        public static bool BCloseBySize { get; set; }
        public static bool BCloseByTime { get; set; }

        public static double TotalNetWeight { get; set; }
        public static double MinNetWeight { get; set; }
        public static double MaxNetWeight { get; set; }
        public static double MinNetWeightBale { get; set; }
        public static double MaxNetWeightBale { get; set; }
        public static double NetWeightSquare { get; set; }
        public static double MeanNetWeight { get; set; }

        public static double RangeNetWeight { get; set; }
        public static double StdNetWeight { get; set; }
        public static double TotalTareWeight { get; set; }
        public static double TotalBdWeight { get; set; }
        public static double TotalMC { get; set; }
        public static double MinMC { get; set; }
        public static double MaxMC { get; set; }
        public static double McSquare { get; set; }
        public static double RangeMC { get; set; }
        public static double StdMC { get; set; }
        public static double BaleAssigned { get; set; }
        public static int ClosingSize { get; set; }
        public static DateTime OpenTD { get; set; }
        public static DateTime CloseTD { get; set; }

        public static string MonthCode { get; set; }

        public static int NextLotNumber { get; set; }
        public static int LotBaleNumber { get; set; }
        public static bool BLotReset { get; set; }
        public static bool BLotClose { get; set; }
        public static string LotStatus { get; set; }

       

        //end lot----------------------------------------------------------------

        public static string StockName { get; set; }
        public static string CalibrationName { get; set; }
        public static string ShiftName { get; set; }
        public static string StockLabel1 { get; set; }
        public static string StockLabel2 { get; set; }
        public static string StockLabel3 { get; set; }
        public static string StockLabel4 { get; set; }

        //Time Events
        public static string HourNow { get; set; }
        public static string DateNow { get; set; }
        public static string StartDate { get; set; }
        public static string DayEndDate { get; set; }
        public static string DayEndHrMn { get; set; }
        public static string ShiftOneDate { get; set; }
        public static string ShiftOneHrMn { get; set; }
        public static string ShiftTwoDate { get; set; }
        public static string ShiftTwoHrMn { get; set; }
        public static string ShiftThreeDate { get; set; }
        public static string ShiftThreeHrMn { get; set; }
        public static string CsvArchivesFileLocation { get; set; }


        public const int SNRollOver = 0;
        public const int SNDayEnd = 1;
        public const int SNMonthEnd = 2;

        public static bool BSnReset;

        public static bool SCaleModeAuto;

        public static string DSWeightEntry = "200.00";

        public enum SerialErrorCodes
        {
            ScaleOnLine = 0,
            ScaleOffline = 1,
            ScaleSimulation = 2,
            OscOnline = 3,
            OscOffline = 4,
            OscSimulation = 5

        };

        public const int OnLine = 0;
        public const int OffLine = 1;
        public const int Simulation = 2;
        public static List<string> DevicesMode = new List<string>() { "OnLine", "OffLine", "Simulation" };

        public static List<string> OscSens = new List<string>() { "A (.100 mSec.)", "B (.200 mSec.)", "C (1 Sec.)" };

        public static int ScaleModeIndex { get; set; }

        public enum ScaleRead : int
        {
            ReadOK = 0,
            ReadBAD = 1,
            ScaleRetry = 2,
            UseDefault = 3
        };

        public enum OscRead : int
        {
            ReadOK = 0,
            ReadBAD = 1,
            ScaleRetry = 2,
            UseDefault = 3
        };


        public enum ProcessState : int
        {
            Idle = 0,
            Empty_wait = 1,
            Scale_wait = 2,
            Full_wait = 3,
            Complete = 4,
            OscTest_Wait = 5,
            ScaleTest_Wait = 6,
            DGHTest_Wait = 7,
            DGH_Wait = 8,
            ScaleRead = 9,
            Reference_Complete = 10,
            Read1_Complete = 11,
            ReadOscError = 12,
            Full2_Wait = 13
        };

        public enum TimeEvents : int
        {
            dayend = 0,
            monthend = 1,
            shiftone = 2,
            shifttwo = 3,
            shiftthree = 4
        };


        /// <summary>
        /// Used for FieldSelect Windows
        /// </summary>
        public enum InstanceType : int
        {
            OpenLot = 0,
            CloseLot = 1,
            Summary = 2
        }

        public enum LotEvent : int
        {
            OpenLot = 0,
            CloseLot = 1,
            Reset = 2
        };



        /// <summary>
        ///  HML Data stored here for lot processing ////////////////////////////////////////////////////////////////////////////
        /// </summary>
        //public static string _xmlHdrOpenLot = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "HdrOpenLot.xml");
        //public static string _xmlHdrCloseLot = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "HdrCloseLot.xml");

        public static List<string> _lotHdrXmlLoclst = new List<string> 
        { Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "HdrOpenLot.xml"),
          Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "HdrCloseLot.xml"),
          Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "GridviewItems.xml")};


        public static List<string> LotTypeLst = new List<string>() { "OpenLot Summary", "CloseLot Summary" };

        public static int idle = 0;
        public static int Initialize = 1;
        public static int ReadUpCount = 2;
        public static int ReadDowncount = 3;
        public static int ReadScale = 4;
        public static int ProcessData = 5;
        public static int SendtoOutput = 6;

        public static string EventsState;

        public static List<string> State = new List<string>()
        { "idle",
            "Initialize",
            "ReadUpCount",
            "ReadDowncount",
            "ReadScale",
            "ProcessData",
            "SendtoOutput"
        };

        /*
    public enum EventsState
    {
        idle,
        Initialize,
        ReadUpCount,
        ReadScale,
        ReadDowncount,
        ProcessData,
        SendtoOutput
    }
    */

        public enum PrntEvents
        {
            DayEnd = 0,
            MonthEnd = 1,
            ShiftOne = 2,
            ShiftTwo = 3,
            ShiftThree = 4,
            ShiftFour = 5,
            LotClose = 6,
            CrossDay = 7
        }

        public enum DataOutput
        {
            outSerialOne = 0,
            outSharedFile = 1
        }

        public static List<string> StringOut = new List<string>() { "SerialOutputString", "SharedFileOutputString", "BalePrintString", "ScaleReqString" };

        public static int outSerialOne = 0;
        public static int outSharedFile = 1;
        public static int outBalePrint = 2;
        public static int ScaleReqString = 3;

        public static string[] ComportList = SerialPort.GetPortNames();
        public static List<int> BaudRateList = new List<int>() { 14400, 9600, 4800, 2400, 1200, 600, 300 };
        public static List<Parity> ParityList = new List<Parity>() { Parity.None, Parity.Even, Parity.Mark, Parity.Odd, Parity.Space };
        public static List<int> DataBitList = new List<int>() { 6, 7, 8 };
        public static List<StopBits> StopBitList = new List<StopBits>() { StopBits.One, StopBits.None, StopBits.OnePointFive, StopBits.Two };

        public static List<Tuple<string, char>> Asciilist = new List<Tuple<string, char>>() {
            new Tuple<string, char>("<NULL>", '\x0'),
            new Tuple<string, char>("<SOH>", '\x1'),
            new Tuple<string, char>("<STX>", '\x2'),
            new Tuple<string, char>("<ETX>", '\x3'),
            new Tuple<string, char>("<EOT>", '\x4'),
            new Tuple<string, char>("<ENQ>", '\x5'),
            new Tuple<string, char>("<ACK>", '\x6'),
            new Tuple<string, char>("<TAB>", '\x9'),
            new Tuple<string, char>("<LF>", '\xA'),
            new Tuple<string, char>("<VT>", '\xB'),
            new Tuple<string, char>("<FF>", '\xC'),
            new Tuple<string, char>("<CR>", '\xD'),
            new Tuple<string, char>("<SO>", '\xF'),
            new Tuple<string, char>("<ETB>", '\x17'),
            new Tuple<string, char>("<ESC>", '\x1B'),
            new Tuple<string, char>("<Space>", '\x20'),
            new Tuple<string, char>(",", '\x2C'),
            new Tuple<string, char>("-", '\x2D'),
            new Tuple<string, char>(".", '\x2E'),
            new Tuple<string, char>("#", '\x23'),
            new Tuple<string, char>(":", '\x2A'),
            new Tuple<string, char>(";", '\x2B'),
            new Tuple<string, char>("[", '\x5B'),
            new Tuple<string, char>("]", '\x5D'),
            new Tuple<string, char>("_", '\x5F')};


        public static int MoistureType
        {
            get => Settings.Default.MoistureType;
            set
            {
                Settings.Default.MoistureType = value;
                Settings.Default.Save();
            }
        }

        public static int WtMetric = 0;
        public static int WtEnglish = 1;

        public static int WeightType
        {
            get => Settings.Default.WeightType;
            set
            {
                Settings.Default.WeightType = value;
                Settings.Default.Save();
            }
        }

        public static bool LotEnable
        {
            get => Settings.Default.LotEnable;
            set 
            {
                Settings.Default.LotEnable = value;
                Settings.Default.Save();
            }
        }

        #region Lot Processing


        #endregion Lot Processing

        public static string CalulateMoisture(string data, int mType)
        {
            string Newdata = string.Empty;
            float ftMoisture = 0;

            switch (mType)
            {
                case 0: // %MC = moisture from Sql database
                    ftMoisture = Convert.ToSingle(data);
                    break;

                case 1: // %MR = Moisture / ( 1- Moisture / 100)
                    ftMoisture = Convert.ToSingle(data) / (1 - Convert.ToSingle(data) / 100);
                    break;

                case 2: // %AD = (100 - moisture) / 0.9
                    ftMoisture = (float)((100 - Convert.ToSingle(data)) / 0.9);

                    break;

                case 3: // %BD = 100 - moisture
                    ftMoisture = 100 - Convert.ToSingle(data);
                    break;
            }
            return ftMoisture.ToString("0.##");
        }
    }

    public class ArchiveData : BindableBase
    {
        private int _Index;
        public int Index
        {
            get { return _Index; }
            set { SetProperty(ref _Index, value); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        private string _datatype;
        public string Datatype
        {
            get { return _datatype; }
            set { SetProperty(ref _datatype, value); }
        }

    }

}
