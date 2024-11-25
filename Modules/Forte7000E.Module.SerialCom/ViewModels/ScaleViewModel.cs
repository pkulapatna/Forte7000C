using ClsErrorLog;
using Forte7000E.Module.SerialCom.Models;
using Forte7000E.Module.SerialCom.Properties;
using Forte7000E.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading;
using System.Windows;

namespace Forte7000E.Module.SerialCom.ViewModels
{
    public class ScaleViewModel : BindableBase
    {
        protected readonly IEventAggregator _eventAggregator;

        private Window ScaleWindow;
        private SerialDevicesModel _serialDevModel;
       
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


        private int _scaleRetryPeriod = Settings.Default.ScaleRetryPeriod;
        public int ScaleRetryPeriod
        {
            get { return _scaleRetryPeriod; }
            set 
            {
                if ((value > 0) & (value < 20))
                    SetProperty(ref _scaleRetryPeriod, value);
                else
                    SetProperty(ref _scaleRetryPeriod, 3);
                Settings.Default.ScaleRetryPeriod = value;
                Settings.Default.Save();
            }
        }

        private int _scaleRetry;
        public int ScaleRetry
        {
            get { return _scaleRetry; }
            set
            {
                if ((value > 0) & (value < 10))
                    SetProperty(ref _scaleRetry, value);
                else
                    SetProperty(ref _scaleRetry, 5);

                Settings.Default.ScaleRetry = value;
                Settings.Default.Save();
            }
        }
        
        private string _wtUnit = ClassCommon.WtUnit;
        public string WtUnit
        {
            get => _wtUnit;
            set => SetProperty(ref _wtUnit, value);
        }


        private int _ScaleCommIndex = Settings.Default.ScaleCommIndex;
        public int ScaleCommIndex
        {
            get { return _ScaleCommIndex; }
            set
            {
                SetProperty(ref _ScaleCommIndex, value);
                if (value > -1)
                {
                    Settings.Default.ScaleCommIndex = value;
                    Settings.Default.Save();
                }
            }
        }

        private int _ScaleBaudRateIndex = Settings.Default.ScaleBaudRateIndex;
        public int ScaleBaudRateIndex
        {
            get { return _ScaleBaudRateIndex; }
            set
            {
                SetProperty(ref _ScaleBaudRateIndex, value);
                if (value > -1)
                {
                    Settings.Default.ScaleBaudRateIndex = value;
                    Settings.Default.Save();
                }
            }
        }

        private int _ScaleParityIndex = Settings.Default.ScaleParityIndex;
        public int ScaleParityIndex
        {
            get { return _ScaleParityIndex; }
            set
            {
                SetProperty(ref _ScaleParityIndex, value);
                if (value > -1)
                {
                    Settings.Default.ScaleParityIndex = value;
                    Settings.Default.Save();
                }
            }
        }

        private int _ScaleDataBitIndex = Settings.Default.ScaleDataBitIndex;
        public int ScaleDataBitIndex
        {
            get { return _ScaleDataBitIndex; }
            set
            {
                SetProperty(ref _ScaleDataBitIndex, value);
                if (value > -1)
                {
                    Settings.Default.ScaleDataBitIndex = value;
                    Settings.Default.Save();
                }
            }
        }

        private int _ScaleStopBitIndex = Settings.Default.ScaleStopBitIndex;
        public int ScaleStopBitIndex
        {
            get { return _ScaleStopBitIndex; }
            set
            {
                SetProperty(ref _ScaleStopBitIndex, value);
                if (value > -1)
                {
                    Settings.Default.ScaleStopBitIndex = value;
                    Settings.Default.Save();
                }
            }
        }

        private bool _bScalePortOpen = false;
        public bool BScalePortOpen
        {
            get { return _bScalePortOpen; }
            set
            {
                SetProperty(ref _bScalePortOpen, value);
                if (value) ScalePortMsg = "Port Opened";
                else ScalePortMsg = "Port Closed";
            }
        }

        private bool _bScaleOnLine = false;
        public bool BScaleOnLine
        {
            get { return _bScaleOnLine; }
            set { SetProperty(ref _bScaleOnLine, value); }
        }

        private string _DevSCaleMode; // Settings.Default.ScaleMode;
        public string DevSCaleMode
        {
            get => _DevSCaleMode;
            set => SetProperty(ref _DevSCaleMode, value);  
        }

        private int _idxStrCharOne = Settings.Default.idxStrCharOne;
        public int IdxStrCharOne
        {
            get { return _idxStrCharOne; }
            set { SetProperty(ref _idxStrCharOne, value); }
        }
        private int _idxStrCharTwo = Settings.Default.idxStrCharTwo;
        public int IdxStrCharTwo
        {
            get { return _idxStrCharTwo; }
            set { SetProperty(ref _idxStrCharTwo, value); }
        }
        private int _idxStrCharThree = Settings.Default.idxStrCharThree;
        public int IdxStrCharThree
        {
            get { return _idxStrCharThree; }
            set { SetProperty(ref _idxStrCharThree, value); }
        }

        private int _idxEndCharOne = Settings.Default.idxEndCharOne;
        public int IdxEndCharOne
        {
            get { return _idxEndCharOne; }
            set { SetProperty(ref _idxEndCharOne, value); }
        }
        private int _idxEndCharTwo = Settings.Default.idxEndCharTwo;
        public int IdxEndCharTwo
        {
            get { return _idxEndCharTwo; }
            set { SetProperty(ref _idxEndCharTwo, value); }
        }
        private int _idxEndCharThree = Settings.Default.idxEndCharThree;
        public int IdxEndCharThree
        {
            get { return _idxEndCharThree; }
            set { SetProperty(ref _idxEndCharThree, value); }
        }

        private List<string> _EndCharLst;
        public List<string> EndCharLst
        {
            get { return _EndCharLst; }
            set { SetProperty(ref _EndCharLst, value); }
        }

        private Visibility _bScaleSim;
        public Visibility BScaleSim
        {
            get { return _bScaleSim; }
            set { SetProperty(ref _bScaleSim, value); }
        }

        private int _ScaleOptionIdx = 0;
        public int ScaleOptionIdx
        {
            get { return _ScaleOptionIdx; }
            set { SetProperty(ref _ScaleOptionIdx, value); }
        }

        private int _selScaleModeIndex = Settings.Default.ScaleModeIndex;
        public int SelScaleModeIndex
        {
            get { return _selScaleModeIndex; }
            set
            {
                SetProperty(ref _selScaleModeIndex, value);
                DevSCaleMode = ClassCommon.DevicesMode[value];
                _eventAggregator.GetEvent<SetScaleMode>().Publish(DevSCaleMode);

                Settings.Default.ScaleModeIndex = value;
                Settings.Default.Save();
                if (value == 0) BScaleOnLine = true;
                if (value == 1) BScaleOnLine = false;
                if (value == 2)
                {
                    BScaleOnLine = false;
                    BScaleSim = Visibility.Visible;
                }
                else
                    BScaleSim = Visibility.Hidden;
            }
        }

        private string _strWeightReq = Settings.Default.StrWeightReq;
        public string StrWeightReq
        {
            get { return _strWeightReq; }
            set { SetProperty(ref _strWeightReq, value); }
        }

        private string _ScaleCharReq = Settings.Default.ScaleCharReq;
        public string ScaleCharReq
        {
            get { return _ScaleCharReq; }
            set { SetProperty(ref _ScaleCharReq, value); }
        }

        private ObservableCollection<string> _ScaleDataReceive;
        public ObservableCollection<string> ScaleDataReceive
        {
            get { return _ScaleDataReceive; }
            set { SetProperty(ref _ScaleDataReceive, value); }
        }
        private string _ScalePortMsg;
        public string ScalePortMsg
        {
            get { return _ScalePortMsg; }
            set { SetProperty(ref _ScalePortMsg, value); }
        }

        private List<string> _DevModeList = ClassCommon.DevicesMode;
        public List<string> DevModeList
        {
            get { return _DevModeList; }
            set { SetProperty(ref _DevModeList, value); }
        }

        private List<string> _ScaleOptList;
        public List<string> ScaleOptList
        {
            get => _ScaleOptList;
            set => SetProperty(ref _ScaleOptList, value);
        }

        private double _SimWeight = Settings.Default.strSimWeight;
        public double SimWeight
        {
            get { return _SimWeight; }
            set
            {
                if ((value > 0) & (value < 500))
                    SetProperty(ref _SimWeight, value);
                else
                    SetProperty(ref _SimWeight, 100);

                ClassCommon.DsimScaleWeight = value;

                Settings.Default.strSimWeight = value;
                Settings.Default.Save();
            }
        }

        private DelegateCommand _ScaleTestCommand;
        public DelegateCommand ScaleTestCommand =>
        _ScaleTestCommand ?? (_ScaleTestCommand = new DelegateCommand(ScaleTestExecute));
        private void ScaleTestExecute()
        {
            using (_serialDevModel = new SerialDevicesModel(_eventAggregator))
            {
                _serialDevModel.ScaleWeightOnDemand();
            }
        }


        private DelegateCommand _SetWtStrCommand;
        public DelegateCommand SetWtStrCommand =>
        _SetWtStrCommand ?? (_SetWtStrCommand = new DelegateCommand(SetWtStrCommandExecute).ObservesCanExecute(() => BScaleOnLine)); 
        private void SetWtStrCommandExecute()
        {
            ScaleWindow = new Window
            {
                Title = "Scale Request String",
                Width = 900,
                Height = 420,
                Content = new Forte700E.Module.ItemSelect.Views.ItemSelectView(ClassCommon.ScaleReqString, _eventAggregator)
            };
            ScaleWindow.ShowDialog();
        }

        public ScaleViewModel(IEventAggregator EventAggregator)
        {
            this._eventAggregator = EventAggregator;

            _eventAggregator.GetEvent<ScaleReqString>().Subscribe(UpdateStringRequest);
            _eventAggregator.GetEvent<ScaleTestDataReturn>().Subscribe(DisplayScaleData);

            SelScaleModeIndex = Settings.Default.ScaleModeIndex;

            ScaleDataReceive = new ObservableCollection<string>();

            ScaleOptList = new List<string>
            {
                "Read/Test"
            };

            ScaleRetry = Settings.Default.ScaleRetry;
        }

        private void DisplayScaleData(string obj)
        {
            ScaleDataReceive.Add(obj.ToString());
        }
     

        private void UpdateStringRequest(string obj)
        {
            if (obj != string.Empty)
            {
                StrWeightReq = obj;
                Settings.Default.StrWeightReq = StrWeightReq;
                Settings.Default.Save();
            }
        }

        private bool TestForNullOrEmpty(string s)
        {
            bool result = s == null || s == string.Empty;
            return result;
        }


    }
}
