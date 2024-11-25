using Forte7000E.Module.SerialCom.Models;
using Forte7000E.Module.SerialCom.Properties;
using Forte7000E.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;

namespace Forte7000E.Module.SerialCom.ViewModels
{
    public class OscillatorViewModel : BindableBase
    {
        protected readonly IEventAggregator _eventAggregator;

        private SerialDevicesModel _serialDevModel;

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

        private int _OscCommIndex = Settings.Default.OscCommIndex;
        public int OscCommIndex
        {
            get { return _OscCommIndex; }
            set
            {
                SetProperty(ref _OscCommIndex, value);
                if (value > -1)
                {
                    Settings.Default.OscCommIndex = value;
                    Settings.Default.Save();
                }
            }
        }
        private int _OscBaudRateIndex = Settings.Default.OscBaudRateIndex;
        public int OscBaudRateIndex
        {
            get { return _OscBaudRateIndex; }
            set
            {
                SetProperty(ref _OscBaudRateIndex, value);
                if (value > -1)
                {
                    Settings.Default.OscBaudRateIndex = value;
                    Settings.Default.Save();
                }
            }
        }
        private int _OscParityIndex = Settings.Default.OscParityIndex;
        public int OscParityIndex
        {
            get { return _OscParityIndex; }
            set
            {
                SetProperty(ref _OscParityIndex, value);
                if (value > -1)
                {
                    Settings.Default.OscParityIndex = value;
                    Settings.Default.Save();
                }
            }
        }
        private int _OscDataBitIndex = Settings.Default.OscDataBitIndex;
        public int OscDataBitIndex
        {
            get { return _OscDataBitIndex; }
            set
            {
                SetProperty(ref _OscDataBitIndex, value);
                if (value > -1)
                {
                    Settings.Default.OscDataBitIndex = value;
                    Settings.Default.Save();
                }
            }
        }
        private int _OscStopBitIndex = Settings.Default.OscStopBitIndex;
        public int OscStopBitIndex
        {
            get { return _OscStopBitIndex; }
            set
            {
                SetProperty(ref _OscStopBitIndex, value);
                if (value > -1)
                {
                    Settings.Default.OscStopBitIndex = value;
                    Settings.Default.Save();
                }
            }
        }
        private bool _bOscPortOpen = false;
        public bool BOscPortOpen
        {
            get { return _bOscPortOpen; }
            set
            {
                SetProperty(ref _bOscPortOpen, value);
                if (value) OscPortMsg = "Port Opened";
                else OscPortMsg = "Port Closed";
            }
        }
        private string _OscDataReceive;
        public string OscDataReceive
        {
            get { return _OscDataReceive; }
            set { SetProperty(ref _OscDataReceive, value); }
        }
        private string _OscPortMsg;
        public string OscPortMsg
        {
            get { return _OscPortMsg; }
            set { SetProperty(ref _OscPortMsg, value); }
        }


        private List<string> _OscSens = ClassCommon.OscSens;
        public List<string> OscSens
        {
            get { return _OscSens; }
            set { SetProperty(ref _OscSens, value); }
        }

        private int _SelOscSensIndex =0;
        public int SelOscSensIndex
        {
            get { return _SelOscSensIndex; }
            set { SetProperty(ref _SelOscSensIndex, value); }
        }

        private List<string> _DevModeList = ClassCommon.DevicesMode;
        public List<string> DevModeList
        {
            get { return _DevModeList; }
            set { SetProperty(ref _DevModeList, value); }
        }
        private int _SelOscModeIndex; 
        public int SelOscModeIndex
        {
            get { return _SelOscModeIndex; }
            set
            {
                SetProperty(ref _SelOscModeIndex, value);

                DevOscMode = ClassCommon.DevicesMode[value];
                Settings.Default.OscModeIndex = value;
                Settings.Default.Save();

                _eventAggregator.GetEvent<SetOscMode>().Publish(DevOscMode);

                switch (value)
                {
                    case 0: //Online
                        OscOnLineVis = Visibility.Visible;
                        OscSimVis = Visibility.Hidden;
                        break;
                    case 1: //OffLine
                        OscSimVis = Visibility.Hidden;
                        OscOnLineVis = Visibility.Hidden;
                        break;
                    case 2: //Sim
                        OscSimVis = Visibility.Visible;
                        OscOnLineVis = Visibility.Hidden;
                        break;
                }
            }
        }

        private Visibility _bOscSim;
        public Visibility OscSimVis
        {
            get { return _bOscSim; }
            set { SetProperty(ref _bOscSim, value); }
        }

        private Visibility _bOscOnLine;
        public Visibility OscOnLineVis
        {
            get { return _bOscOnLine; }
            set { SetProperty(ref _bOscOnLine, value); }
        }


        private string _DevOscMode = Settings.Default.OscMode;
        public string DevOscMode
        {
            get => _DevOscMode;
            set
            {
                if (value != null)
                {
                    Settings.Default.OscMode = value;
                    Settings.Default.Save();
                    SetProperty(ref _DevOscMode, value);
                }
            }
        }
        private List<string> _OscOptList;
        public List<string> OscOptList
        {
            get { return _OscOptList; }
            set { SetProperty(ref _OscOptList, value); }
        }
        private int _OscOptionIdx = 0;
        public int OscOptionIdx
        {
            get { return _OscOptionIdx; }
            set { SetProperty(ref _OscOptionIdx, value); }
        }


        private int _ForteRange = Settings.Default.iForteRange;
        public int ForteRange
        {
            get { return _ForteRange; }
            set
            {
                SetProperty(ref _ForteRange, value);
                if ((value > 0) & (value < 6000))
                    SetProperty(ref _ForteRange, value);
                else
                    SetProperty(ref _ForteRange, 1000);

                Settings.Default.iForteRange = value;
                Settings.Default.Save();
            }
        }


        public OscillatorViewModel(IEventAggregator EventAggregator)
        {
            this._eventAggregator = EventAggregator;

            //Check Osc online?
            SelOscModeIndex = Settings.Default.OscModeIndex;

            _eventAggregator.GetEvent<OscTestDataReturn>().Subscribe(OscDataReturn);

            OscOptList = new List<string>
            {
                "Read/Test",  // '0
                "Version",    // '1
                "ReTransmit", // '2
                "Init"        // '3
            };
        }

        private void OscDataReturn(string obj)
        {
            OscDataReceive = obj;
        }

        private DelegateCommand _OscTestCommand;
        public DelegateCommand OscTestCommand =>
        _OscTestCommand ?? (_OscTestCommand = new DelegateCommand(OscTestExecute));
        private void OscTestExecute()
        {
            SendOscRequest(SentOscRequestType(OscOptionIdx));
        }

        private void SendOscRequest(string strTosend)
        {
            using (_serialDevModel = new SerialDevicesModel(_eventAggregator))
            {
                _serialDevModel.SendOscRequestString(strTosend);        
            }
        }

        internal string SentOscRequestType(int intType)
        {
            string strTosend;
            switch (intType)
            {
                case 0:     //"Read/Test"
                    strTosend = "T";
                    break;
                case 1:     //Version
                    strTosend = "V";
                    break;
                case 2:     //ReTransmit
                    strTosend = "R";
                    break;
                case 3:     // Init
                    strTosend = "Init";
                    break;
                default:
                    strTosend = "T";
                    break;
            }
            return strTosend;
        }
    }
}
