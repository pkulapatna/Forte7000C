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
using System.Windows;

namespace Forte7000E.Module.SerialCom.ViewModels
{
    public class SerialOneViewModel : BindableBase
    {
        protected readonly IEventAggregator _eventAggregator;

        private Window SerialWindow;

        public Xmlhandler MyXml { get; set; }

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

        private string _outputstatus;
        public string Outputstatus
        {
            get => _outputstatus;
            set => SetProperty(ref _outputstatus, value);
        }

        private bool _serialOneEnable = Settings.Default.SerialOneEnable;
        public bool SerialOneEnable
        {
            get { return _serialOneEnable; }
            set 
            {
                SetProperty(ref _serialOneEnable, value);
                Settings.Default.SerialOneEnable = value;
                Settings.Default.Save();
                if (value) SerialOneOutString = GetXmlfile(0);
            }
        }

        private string _SerialOneOutString = " ";
        public string SerialOneOutString
        {
            get { return _SerialOneOutString; }
            set { SetProperty(ref _SerialOneOutString, value); }
        }

        private int _SerialOneCommIndex = Settings.Default.SerialOneCommIndex;
        public int SerialOneCommIndex
        {
            get { return _SerialOneCommIndex; }
            set
            {
                SetProperty(ref _SerialOneCommIndex, value);
                if (value > -1)
                {
                    Settings.Default.SerialOneCommIndex = value;
                    Settings.Default.Save();
                }
            }
        }

        private int _SerialOneBaudRateIndex = Settings.Default.SerialOneBaudRateIndex;
        public int SerialOneBaudRateIndex
        {
            get { return _SerialOneBaudRateIndex; }
            set
            {
                SetProperty(ref _SerialOneBaudRateIndex, value);
                if (value > -1)
                {
                    Settings.Default.SerialOneBaudRateIndex = value;
                    Settings.Default.Save();
                }
            }
        }

        private int _SerialOneParityIndex = Settings.Default.SerialOneParityIndex;
        public int SerialOneParityIndex
        {
            get { return _SerialOneParityIndex; }
            set
            {
                SetProperty(ref _SerialOneParityIndex, value);
                if (value > -1)
                {
                    Settings.Default.SerialOneParityIndex = value;
                    Settings.Default.Save();
                }
            }
        }

        private int _SerialOneDataBitIndex = Settings.Default.SerialOneDataBitIndex;
        public int SerialOneDataBitIndex
        {
            get { return _SerialOneDataBitIndex; }
            set
            {
                SetProperty(ref _SerialOneDataBitIndex, value);
                if (value > -1)
                {
                    Settings.Default.SerialOneDataBitIndex = value;
                    Settings.Default.Save();
                }
            }
        }

        private int _SerialOneStopBitIndex = Settings.Default.SerialOneStopBitIndex;
        public int SerialOneStopBitIndex
        {
            get { return _SerialOneStopBitIndex; }
            set
            {
                SetProperty(ref _SerialOneStopBitIndex, value);
                if (value > -1)
                {
                    Settings.Default.SerialOneStopBitIndex = value;
                    Settings.Default.Save();
                }
            }
        }

        public SerialOneViewModel(IEventAggregator EventAggregator)
        {
            _eventAggregator = EventAggregator;
            MyXml = Xmlhandler.Instance;

            _eventAggregator.GetEvent<UpdateStringOutput>().Subscribe(UpdateOutput);
            if (SerialOneEnable) SerialOneOutString = GetXmlfile(0);
        }

        private void UpdateOutput(int obj)
        {
            SerialOneOutString = GetXmlfile(obj);
        }

      
        private string GetXmlfile(int obj)
        {
            string xmlfile = string.Empty;
          
            ObservableCollection<DataOutput> _myxmlfile = MyXml.ReadXmlStringOut(obj);

            for (int i = 0; i < _myxmlfile.Count; i++)
            {
                xmlfile += _myxmlfile[i].Name;
            }
            return xmlfile;
        }

        private DelegateCommand _SerialOneConfigCommand;
        public DelegateCommand SerialOneConfigCommand =>
        _SerialOneConfigCommand ?? (_SerialOneConfigCommand =
            new DelegateCommand(SerialOneConfigCommandExecute).ObservesCanExecute(() => SerialOneEnable));
        private void SerialOneConfigCommandExecute()
        {
            SerialWindow = new Window
            {
                Title = "Serial port one",
                Width = 900,
                Height = 420,
                Content = new Forte700E.Module.ItemSelect.Views.ItemSelectView(ClassCommon.outSerialOne, _eventAggregator)
            };
            SerialWindow.ShowDialog();
        }

        private DelegateCommand _SerialOneTestCommand;
        public DelegateCommand SerialOneTestCommand =>
        _SerialOneTestCommand ?? (_SerialOneTestCommand =
            new DelegateCommand(SerialOneTestExecute).ObservesCanExecute(() => SerialOneEnable));
        private void SerialOneTestExecute()
        {


            //Outputstatus = "Serial Output  [" + ProcessModel.GetDataFromXmlfile(ClassCommon.outSharedFile) + "]"; SerialOneOutString
        }
    }
}
