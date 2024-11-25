using ClsErrorLog;
using Forte7000E.Module.SerialCom.Properties;
using Forte7000E.Services;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Forte7000E.Module.SerialCom.Models
{
    public class SerialDevicesModel : BindableBase, IDisposable
    {
        protected readonly Prism.Events.IEventAggregator _eventAggregator;

        public SerialPort ScaleDevices;
        public SerialPort OscDevices;
        public SerialPort SerialOneDevice;

      //  private readonly Queue<byte> recievedData = new Queue<byte>();

        private readonly char chrCR = '\xD';     //<CR>
        private string strWeightReq = string.Empty;
        private string strOscReq;


        bool BScaleDataValid;
        bool BOscDataValid;

        public bool BDTR { get; set; }

        public IReadOnlyList<string> ComPortLst
        {
            get { return ClassCommon.ComportList; }
        }
        public IReadOnlyList<int> BaudRateLst
        {
            get { return ClassCommon.BaudRateList; }
        }
        public IReadOnlyList<Parity> ParityLst
        {
            get { return ClassCommon.ParityList; }
        }
        public IReadOnlyList<StopBits> StopBitLst
        {
            get { return ClassCommon.StopBitList; }
        }
        public IReadOnlyList<int> DataBitsLst
        {
            get { return ClassCommon.DataBitList; }
        }

        public SerialDevicesModel(IEventAggregator EventAggregator)
        {
            _eventAggregator = EventAggregator;
        }

        public bool SetUpSerialDevices()
        {
            bool bDone = false;
            strWeightReq = Settings.Default.StrWeightReq;
            strOscReq = "T";

            try
            {
                bDone = InitScaleDevice();
                bDone = InitOscDevice();
            }
            catch (Exception ex )
            {
                MessageBox.Show("ERROR in SetUpSerialDevices " + ex.Message );
            }
            return bDone;
        }


        /// <summary>
        /// ///////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        private bool InitScaleDevice()
        {
            bool bOK = false;
            try
            {

                if (ScaleDevices == null) ScaleDevices = new SerialPort();
                if (ComPortLst.Count == 0) ScaleDevices.PortName = "0";
                    else
                    {
                        ScaleDevices.PortName = ComPortLst[Settings.Default.ScaleCommIndex];
                        ScaleDevices.BaudRate = BaudRateLst[Settings.Default.ScaleBaudRateIndex];
                        ScaleDevices.Parity = ParityLst[Settings.Default.ScaleParityIndex];
                        ScaleDevices.StopBits = StopBitLst[Settings.Default.ScaleStopBitIndex];
                        ScaleDevices.DataBits = DataBitsLst[Settings.Default.ScaleDataBitIndex];
                        ScaleDevices.DataReceived += new SerialDataReceivedEventHandler(ScaleDataReceived);
                        ScaleDevices.Handshake = Handshake.None;
                        ScaleDevices.WriteTimeout = 500;
                        ScaleDevices.ReadTimeout = 200;
                        ScaleDevices.ErrorReceived += new SerialErrorReceivedEventHandler(Scaleport_Error);
                    }
                if (BDTR) //using DTR -> DSR pin20 to pin6
                {
                    ScaleDevices.RtsEnable = true;
                    ScaleDevices.DtrEnable = true;
                    ScaleDevices.PinChanged += new SerialPinChangedEventHandler(Scaleport_PinChanged);
                }
                BScaleDataValid = false;
                bOK = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in InitScaleDevice" + ex.Message);   
            }
            return bOK;
        }

        private void Scaleport_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            
        }

        private void Scaleport_Error(object sender, SerialErrorReceivedEventArgs e)
        {
            MessageBox.Show("Scaleport_Error " + e.ToString());
        }

        private void ScaleDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!ScaleDevices.IsOpen) return;
            try
            {
                Thread.Sleep(250); //mSec grap the whole string from sacale

                if (ScaleDevices.IsOpen)
                {
                    string buffer = ScaleDevices.ReadExisting();
                    if ((buffer != null) & (buffer.Length > 0))
                    {
                        //Serialport is a different Thread to display on windows form!
                        //Use the Invoke Method and pass the delegate and data
                        //Application.Current.Dispatcher.Invoke(new Action(() => { ParseScalePacketData(buffer); }));

                        Application.Current.Dispatcher.Invoke(new ProcessScaleDataDelegate(ParseScalePacketData), buffer);
                    }   
                }
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show("ERROR in ScaleDataReceived" + ex.ToString());
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.CRITICAL, MsgSources.ANALOG, "Scale Port ERROR " + ex.Message);
            }
            finally
            {
                if (ScaleDevices.IsOpen) ScaleDevices.Close();
            }
        }

        delegate void ProcessScaleDataDelegate(string data);
        private void ParseScalePacketData(string data)
        {
            BScaleDataValid = false;
            try
            {
                if ((data != null) && (data.Length > 0))
                {
                    if (dispatcherTimerScale != null)
                        dispatcherTimerScale.Stop();
                    iScaleRetry = 0;
                    BScaleDataValid = true;

                    var doubleArray = System.Text.RegularExpressions.Regex.Split(data, @"[^0-9\.]+")
                                  .Where(c => !String.IsNullOrEmpty(c) && c != ".").ToArray();
                    string StrWeight = doubleArray[0].ToString();

                    _eventAggregator.GetEvent<ScaleTestDataReturn>().Publish(StrWeight);
                    _eventAggregator.GetEvent<UpdateScaledataEvent>().Publish(StrWeight);
                }
            }
            catch (Exception ex)
            {
                BScaleDataValid = false;
                MessageBox.Show("ERROR in ParseScalePacketData" + ex.ToString());
            }
        }


        internal void ScaleWeightOnDemand()
        {
            if (ScaleDevices == null) InitScaleDevice();
            if (!ScaleDevices.IsOpen) ScaleDevices.Open();
            ScaleDevices.Write(Settings.Default.StrWeightReq + chrCR);
        }

        public void GetScaleWeight()
        {
            ClassCommon.BDefaultWeight = false;

            switch (Settings.Default.ScaleModeIndex)
            {
                case 0: //online
                    StartTimerScale();
                    break;

                case 1://offline
                   // string strMsg = ClassCommon.DevicesMode[ClassCommon.OffLine];
                   // System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { ReportScaleMessage(strMsg); }));
                    break;

                case 2: //Simulation
                    string simWeight = GetSimScaleWeight(Settings.Default.strSimWeight, Settings.Default.bSimWtRandom);
                    _eventAggregator.GetEvent<UpdateScaledataEvent>().Publish(simWeight);
                    _eventAggregator.GetEvent<ScaleTestDataReturn>().Publish(simWeight);
                    ClassCommon.BDefaultWeight = false;
                    break;
            }
        }

        public string GetSimScaleWeight(double simDefaultwt, bool bWtRandom)
        {
            string textdata = simDefaultwt.ToString();

            if (bWtRandom)
            {
                if (simDefaultwt > 0)
                {
                    double rWeight = GetRandWeight(Convert.ToInt16(simDefaultwt * 10 - 100), (Convert.ToInt16(simDefaultwt * 10 + 100))) / 10;
                    textdata = rWeight.ToString();
                }
            }
            return textdata;
        }

        private double GetRandWeight(int iLoWeight, int iHiWeight)
        {
            Random rnd = new Random();
            return (rnd.Next(iLoWeight, iHiWeight));
        }
        
        /// <summary>
        /// /////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        private bool InitOscDevice()
        {
            bool bOK = false;
            try
            {
                 if (OscDevices == null) OscDevices = new SerialPort();

                
                if (ComPortLst.Count == 0) OscDevices.PortName = "0";
                else
                    OscDevices.PortName = ComPortLst[Settings.Default.OscCommIndex];
                    OscDevices.BaudRate = BaudRateLst[Settings.Default.OscBaudRateIndex];
                    OscDevices.Parity = ParityLst[Settings.Default.OscParityIndex];
                    OscDevices.StopBits = StopBitLst[Settings.Default.OscStopBitIndex];
                    OscDevices.DataBits = DataBitsLst[Settings.Default.OscDataBitIndex];
                    OscDevices.DataReceived += new SerialDataReceivedEventHandler(OscDataReceived);
                    OscDevices.Handshake = Handshake.None;
                    OscDevices.WriteTimeout = 500;
                    OscDevices.ReadTimeout = 200;
                    OscDevices.ErrorReceived += new SerialErrorReceivedEventHandler(Oscport_Error);

                if (BDTR) //using DTR -> DSR pin20 to pin6
                {
                    OscDevices.RtsEnable = true;
                    OscDevices.DtrEnable = true;
                    OscDevices.PinChanged += new SerialPinChangedEventHandler(Oscport_PinChanged);
                }
                bOK = true;
                BOscDataValid = false;
            }
            catch (Exception ex) 
            {
                MessageBox.Show("ERROR in InitOscDevice " + ex.Message);
            }
            return bOK;
        }

        public void SendOscRequestString(string strOscReq)
        {
            if (Settings.Default.OscMode == ClassCommon.DevicesMode[ClassCommon.OnLine])
            {
                InitOscDevice();
                if (!OscDevices.IsOpen) OscDevices.Open();
                OscDevices.Write(strOscReq + chrCR);
            }
            else if (Settings.Default.OscMode == ClassCommon.DevicesMode[ClassCommon.Simulation])
            {
                int iCountDown = 3071112; //2EDC88

                if (ClassCommon.EventsState == ClassCommon.State[ClassCommon.ReadUpCount])
                {
                    iCountDown = 3071112 + Settings.Default.iForteRange;
                }
                else if (ClassCommon.EventsState == ClassCommon.State[ClassCommon.ReadDowncount])
                {
                    iCountDown = 3071112;
                }

                int dRanUp = GetRandOsc(iCountDown, iCountDown + 100);
                string strToSend = dRanUp.ToString("X") + "," + iCountDown.ToString("X");

                string strcheck = GetChecksumByte('\x02' + "T" + strToSend);
                string strSImDat = "\x02" + "T" + strToSend + strcheck + "\xD";

                _eventAggregator.GetEvent<OscTestDataReturn>().Publish(strSImDat);
                _eventAggregator.GetEvent<UpdateOscdataEvent>().Publish(strSImDat);
            }
        }


        private int GetRandOsc(int iLoWeight, int iHiWeight)
        {
            Random rnd = new Random();
            return (rnd.Next(iLoWeight, iHiWeight));
        }

        private string GetChecksumByte(string strToCalc)
        {
            byte chcksum = 0;
            byte asciival;

            //Chenge each char to ASCii Dec.
            foreach (byte b in System.Text.Encoding.UTF8.GetBytes(strToCalc.ToCharArray()))
            {
                asciival = b;
                chcksum ^= asciival;
            }
            chcksum &= 0xff; //chcksum &= 0xe;
            return chcksum.ToString("X2");
        }

        private void Oscport_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
           // throw new NotImplementedException();
          
        }

        private void Oscport_Error(object sender, SerialErrorReceivedEventArgs e)
        {
            MessageBox.Show("Oscport_Error " + e.ToString());
        }

        private void OscDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
           // string data = string.Empty;

            if (!OscDevices.IsOpen) return;

            try
            {
                Thread.Sleep(250); //mSec.

                if (OscDevices.IsOpen)
                {
                   var data = OscDevices.ReadExisting();

                    if ((data != null) & (data.Length > 0))
                    {
                        //Serialport is a different Thread to display on windows form!
                        //Use the Invoke Method and pass the delegate and data
                        Application.Current.Dispatcher.Invoke(new Action(() => { ParseOscPacketData(data); }));

                        Application.Current.Dispatcher.Invoke(new ProcessOscDataDelegate(ParseOscPacketData), data);
                    }
                }
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show("ERROR in OscDataFromModel" + ex.ToString());
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.CRITICAL, MsgSources.ANALOG, ex.Message);
            }
            finally
            {
                if (OscDevices.IsOpen) OscDevices.Close();
            }
        }

        delegate void ProcessOscDataDelegate(string data);
        private void ParseOscPacketData(string data)
        {
            BOscDataValid = true;
            _eventAggregator.GetEvent<OscTestDataReturn>().Publish(data);
            _eventAggregator.GetEvent<UpdateOscdataEvent>().Publish(data);
        }

        /// <summary>
        /// OutOnePort only call when serial Data out is enabled 
        /// </summary>
        public bool InitOpenOutOnePort()
        {
            if (SerialOneDevice == null) SerialOneDevice = new SerialPort();

            if (!SerialOneDevice.IsOpen)
            {
                SerialOneDevice.PortName = ComPortLst[Settings.Default.SerialOneCommIndex];
                SerialOneDevice.BaudRate = BaudRateLst[Settings.Default.SerialOneBaudRateIndex];
                SerialOneDevice.Parity = ParityLst[Settings.Default.SerialOneParityIndex];
                SerialOneDevice.StopBits = StopBitLst[Settings.Default.SerialOneStopBitIndex];
                SerialOneDevice.DataBits = DataBitsLst[Settings.Default.SerialOneDataBitIndex];
                SerialOneDevice.DataReceived += new SerialDataReceivedEventHandler(SerialOneDataReceived);
                SerialOneDevice.Handshake = Handshake.None;
                SerialOneDevice.WriteTimeout = 500;
                SerialOneDevice.ReadTimeout = 100;
                SerialOneDevice.ErrorReceived += new SerialErrorReceivedEventHandler(SerialOne_Error);

                if (BDTR) //using DTR -> DSR pin20 to pin6
                {
                    SerialOneDevice.RtsEnable = true;
                    SerialOneDevice.DtrEnable = true;
                    SerialOneDevice.PinChanged += new SerialPinChangedEventHandler(SerialOne_PinChanged);
                }
            }
            try
            {
                if (!SerialOneDevice.IsOpen)
                    SerialOneDevice.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in  InitOpenOutOnePort " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.ANALOG, "ERROR in  InitOpenOutOnePort " + ex.Message);

                return false;
            }
            return true;
        }


        private void SerialOneDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //string data = string.Empty;

            try
            {
                Thread.Sleep(250); //mSec.

                if (SerialOneDevice.IsOpen)
                {
                    var data = SerialOneDevice.ReadExisting();

                    if ((data != null) & (data.Length > 0))
                    {
                        //Serialport is a different Thread to display on windows form!
                        //Use the Invoke Method and pass the delegate and data
                        Application.Current.Dispatcher.Invoke(new Action(() => { ProcessSerialOneData(data); }));
                    }       
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in  SerialOneDataReceived " + ex.Message);
            }
            finally
            {
                if (SerialOneDevice.IsOpen) SerialOneDevice.Close();
            }
        }

        /// <summary>
        /// Need Work!
        /// </summary>
        /// <param name="data"></param>
        private void ProcessSerialOneData(string data)
        {
            //throw new NotImplementedException();
        }

        private void SerialOne_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
          //  throw new NotImplementedException();
        }

        private void SerialOne_Error(object sender, SerialErrorReceivedEventArgs e)
        {
            MessageBox.Show("SerialOne_Error " + e.ToString());
        }

        public void SendSerialOneOut(string strSerialOut)
        {
            if (InitOpenOutOnePort())
            {
                if (strSerialOut != null)
                {
                    SerialOneDevice.Write(strSerialOut);
                }
                SerialOneDevice.Close();
            }
        }

        public bool CheckSerialOneEnable()
        {
            return Settings.Default.SerialOneEnable;
        }

        public void Dispose()
        {
          //  if (OscDevices != null) OscDevices = null;
          //  if (ScaleDevices != null) ScaleDevices = null;
         //   if (SerialOneDevice != null) SerialOneDevice = null;
        }

        #region DispatcherScaleTimer /////////////////////////////////////////////////////////////////////////////////////

        private System.Windows.Threading.DispatcherTimer dispatcherTimerScale;
        int iScaleRetry = 0;

        /// <summary>
        /// System.Windows.Threading.DispatcherTimer
        /// When request Weight
        /// 1. Start timer
        /// 2. First tick Request Weight => ScaleDataReceived
        /// Good
        /// 3. If ScaleDataReceived then BScaleDataValid => true;
        /// 4. Stop timer.
        /// Bad
        /// 5. If no ScaleDataReceived => continue Timer until retry expire
        /// 6. Then use default weight from Material type.
        /// </summary>
        ///
        private void InitializeScaleTimer(string EventTag)
        {
            if (dispatcherTimerScale != null) dispatcherTimerScale = null;
            dispatcherTimerScale = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(Settings.Default.ScaleRetryPeriod) // in sec.
            };
            dispatcherTimerScale.Tick += new EventHandler(DispatcherScaleTimer_Tick);
            dispatcherTimerScale.Tag = EventTag;
            iScaleRetry = 0;
            BScaleDataValid = false;
        }

        private void DispatcherScaleTimer_Tick(object sender, EventArgs e)
        {
            if (!ScaleDevices.IsOpen) ScaleDevices.Open();
            ScaleDevices.Write(Settings.Default.StrWeightReq + chrCR);

            iScaleRetry += 1;

            //If no weight read then continue to retry
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { EvaluateScaleTimer(); }));

        }
        /// <summary>
        /// To continue or stop the Scale Timer
        /// </summary>
        private void EvaluateScaleTimer()
        {
            if (iScaleRetry == Settings.Default.ScaleRetry + 2 )
            {
                Console.WriteLine("-------------------" + " Last ScaleRetry = " + (iScaleRetry-1).ToString());
                StopTimerScale();
                iScaleRetry = 0;
                _eventAggregator.GetEvent<ScaleErrorEvent>().Publish((int)ClassCommon.ScaleRead.ReadBAD);
                if (ScaleDevices.IsOpen) ScaleDevices.Close();
            }
            else if (iScaleRetry == 1) //Pre Retry
            {
               Console.WriteLine("---------Read Scale First time ----------");
               ClassCommon.BDefaultWeight = false;
            }
            else //Scale Retry
            {
                _eventAggregator.GetEvent<ScaleRetryEvent>().Publish(iScaleRetry - 1);
                Console.WriteLine("-------------------" + " ScaleRetry = " + (iScaleRetry - 1).ToString());
            }

            if (BScaleDataValid)
            {
                StopTimerScale();
                if (ScaleDevices.IsOpen) ScaleDevices.Close();
                ClassCommon.BDefaultWeight = false;
            }
        }

        public void StartTimerScale()
        {
            InitializeScaleTimer("SCALE");
            dispatcherTimerScale.Start();
            BScaleDataValid = false;
            ClassCommon.BDefaultWeight = false;
        }

        private void StopTimerScale()
        {
            if (dispatcherTimerScale != null)
            {
                dispatcherTimerScale.Stop();
                Console.WriteLine("---Stop Timer Scale " + DateTime.Now);
            }
        }

        #endregion DispatcherTimerScale ///////////////////////////////////////////////////////////////////////////////


        #region DispatcherOscTimer /////////////////////////////////////////////////////////////////////////////////////

        private System.Windows.Threading.DispatcherTimer dispatcherTimerOsc;
        int iOscRetry = 0;

        /// <summary>
        /// System.Windows.Threading.DispatcherTimer
        /// OSC need to be read twice for Upcount and downcount not the same time to get Forte number.
        /// 1. Upcount reads when the testcell is empty 
        /// 2. Downcount reads when the material is in the testcell at the same time also reads the scale
        /// </summary>
        /// 
        private void InitializeOscTimer(string EventTag)
        {
            if (dispatcherTimerOsc != null) dispatcherTimerOsc = null;
            dispatcherTimerOsc = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(Settings.Default.OscRetryPeriod) // in sec
            };
            dispatcherTimerOsc.Tick += new EventHandler(DispatcherOscTimer_Tick);
            dispatcherTimerOsc.Tag = EventTag;
        }

        private void DispatcherOscTimer_Tick(object sender, EventArgs e)
        {
           
            if (!OscDevices.IsOpen) OscDevices.Open();
            OscDevices.Write(strOscReq + chrCR);
            iOscRetry += 1;
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { EvaluateOscTimer(); }));
        }

        private void EvaluateOscTimer()
        {
            if (iOscRetry == Settings.Default.OscRetry + 2)
            {
                Console.WriteLine("-------------------" + " Last OscRetry = " + (iOscRetry - 1).ToString());
                StopOscTimer();
                iOscRetry = 0;
                _eventAggregator.GetEvent<OscErrorEvent>().Publish((int)ClassCommon.OscRead.ReadBAD);
            }
            else if (iOscRetry == 1) //Pre Retry
            {
                Console.WriteLine("---------Read Oscirst time ----------");
            }
            else //OscRetry
            {
                _eventAggregator.GetEvent<OscRetryEvent>().Publish(iOscRetry - 1);
                Console.WriteLine("-------------------" + " OSCRetry = " + (iOscRetry - 1).ToString());
            }

            if(BOscDataValid)
            {
                StopOscTimer();
            }
        }

       
        public void StartOscTimer()
        {
            InitializeOscTimer("OSC");
            dispatcherTimerOsc.Start();
        }
        public void StopOscTimer()
        {
            if (dispatcherTimerOsc != null)
            {
                dispatcherTimerOsc.Stop();
            }
        }

        #endregion DispatcherTimerOsc //////////////////////////////////////////////////////////////////////////////
    }
}
