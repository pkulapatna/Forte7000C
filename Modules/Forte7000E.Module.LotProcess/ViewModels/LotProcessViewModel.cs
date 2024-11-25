using System;
using System.Data;
using System.Windows;
using Forte7000E.Module.FieldSelect.Views;
using Forte7000E.Module.LotProcess.Models;
using Forte7000E.Module.LotProcess.Properties;
using Forte7000E.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Forte7000E.Module.LotProcess.ViewModels
{
    public class LotProcessViewModel : BindableBase
    {
        protected readonly IEventAggregator _eventAggregator;
        private readonly LotProcessModel ProcessModel;
        private Window OpenLotWindow;
        private Window CloseLotWindow;

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public bool LpEnable { get; set; }

        /*
        private bool _LpEnable = false;
        public bool LpEnable
        {
            get { return _LpEnable; }
            set
            {
                SetProperty(ref _LpEnable, value);
                ClassCommon.LotEnable = value;
                if (value)ClassCommon.LotStatus = "Open";

                _eventAggregator.GetEvent<LotProcessEnable>().Publish(value);
            }
        }
        */

        private bool _LCRollOver;   //0
        public bool LCRollOver
        {
            get => _LCRollOver;
            set
            {
                SetProperty(ref _LCRollOver, value);
                if (value)
                {
                    Settings.Default.LpResetMode = 0;
                    Settings.Default.Save();
                }
            }
        }
        private bool _LCDayEnd;     //1
        public bool LCDayEnd
        {
            get { return _LCDayEnd; }
            set
            {
                SetProperty(ref _LCDayEnd, value);
                if (value)
                {
                    Settings.Default.LpResetMode = 1;
                    Settings.Default.Save();
                }
            }
        }
        private bool _LCMonthEnd;   //2
        public bool LCMonthEnd
        {
            get { return _LCMonthEnd; }
            set
            {
                SetProperty(ref _LCMonthEnd, value);
                if (value)
                {
                    Settings.Default.LpResetMode = 2;
                    Settings.Default.Save();
                }
            }
        }
        private bool _LCLotSize;    //3
        public bool LCLotSize
        {
            get { return _LCLotSize; }
            set
            {
                SetProperty(ref _LCLotSize, value);
                if (value)
                {
                    Settings.Default.LpResetMode = 3;
                    Settings.Default.Save();
                }
            }
        }


        private bool _LCMaunal;    //4
        public bool LCMaunal
        {
            get { return _LCMaunal; }
            set
            {
                SetProperty(ref _LCMaunal, value);
                if (value)
                {
                    Settings.Default.LpResetMode = 4;
                    Settings.Default.Save();
                }
            }
        }


        private bool _SingleLot;
        public bool SingleLot
        {
            get { return _SingleLot; }
            set
            {
                SetProperty(ref _SingleLot, value);
                if (value)
                {
                    Settings.Default.LotType = 0;
                    Settings.Default.Save();
                }
            }
        }
        private bool _MultipleLot;
        public bool MultipleLot
        {
            get { return _MultipleLot; }
            set
            {
                SetProperty(ref _MultipleLot, value);
                if (value)
                {
                    Settings.Default.LotType = 1;
                    Settings.Default.Save();
                }
            }
        }

       

        private long _LotMax = Settings.Default.iLotMax;
        public long LotMax
        {
            get { return _LotMax; }
            set
            {
                SetProperty(ref _LotMax, value);
                if ((value > 0) & (value < 99999))
                {
                    Settings.Default.iLotMax = value;
                    Settings.Default.Save();
                }
            }
        }
        private long _customLotSize = Settings.Default.iCustomLotSize;
        public long CustomLotSize
        {
            get { return _customLotSize; }
            set
            {
                SetProperty(ref _customLotSize, value);
                if ((value > 0) & (value < 99999))
                {
                    Settings.Default.iCustomLotSize = value;
                    Settings.Default.Save();
                }
            }
        }

        private bool _lotPrintChecked = Settings.Default.LotPrintChecked;
        public bool LotPrintChecked
        {
            get => _lotPrintChecked;
            set
            {
                SetProperty(ref _lotPrintChecked, value);
                Settings.Default.LotPrintChecked = value;
                Settings.Default.Save();
            }
        }


        private DataTable _openLotTableLPVM;
        public DataTable OpenLotTableLPVM
        {
            get => _openLotTableLPVM;
            set => SetProperty(ref _openLotTableLPVM, value);
        }
        private DataTable _closeLotTable;
        public DataTable CloseLotTable
        {
            get => _closeLotTable;
            set => SetProperty(ref _closeLotTable, value);
        }

        private int _selectIdxLotOpen;
        public int SelectIdxLotOpen
        {
            get => _selectIdxLotOpen;
            set => SetProperty(ref _selectIdxLotOpen, value);
        }
        private int _selectIdxLotClose;
        public int SelectIdxLotClose
        {
            get => _selectIdxLotClose;
            set => SetProperty(ref _selectIdxLotClose, value);
        }

        private DelegateCommand _printLotRepCommand;
        public DelegateCommand PrintLotRepCommand =>
       _printLotRepCommand ?? (_printLotRepCommand =
           new DelegateCommand(PrintLotRepExecute).ObservesCanExecute(() => LpEnable));
        private void PrintLotRepExecute()
        {
           //throw new NotImplementedException();
        }

        private DelegateCommand _previewLotRepCommand;
        public DelegateCommand PreviewLotRepCommand =>
       _previewLotRepCommand ?? (_previewLotRepCommand =
           new DelegateCommand(PreviewLotRepExecute).ObservesCanExecute(() => LpEnable));
        private void PreviewLotRepExecute()
        {
            if (!ProcessModel.ShowReportDialog((int)ClassCommon.PrntEvents.LotClose))
                MessageBox.Show("No Lot Data found to preview!");
        }

        private DelegateCommand _OpenLotCommand;
        public DelegateCommand OpenLotCommand =>
        _OpenLotCommand ?? (_OpenLotCommand =
            new DelegateCommand(OpenLotCommandExecute).ObservesCanExecute(() => LpEnable));
        private void OpenLotCommandExecute()
        {
            OpenLotWindow = new Window()
            {
                Title = "Open Lot Field Selections",
                Width = 970,
                Height = 440,
                Content = new FieldSelect.Views.FieldSelect(_eventAggregator,(int)ClassCommon.InstanceType.OpenLot)
            };
            OpenLotWindow.ResizeMode = ResizeMode.NoResize;
            OpenLotWindow.ShowDialog();
        }

        private DelegateCommand _CloseLotCommand;
        public DelegateCommand CloseLotCommand =>
        _CloseLotCommand ?? (_CloseLotCommand =
            new DelegateCommand(CloseLotCommandExecute).ObservesCanExecute(() => LpEnable));
        private void CloseLotCommandExecute()
        {
            CloseLotWindow = new Window
            {
                Title = "Close Lot Field Selections",
                Width = 970,
                Height = 440,
                Content = new FieldSelect.Views.FieldSelect(_eventAggregator,(int)ClassCommon.InstanceType.CloseLot)
            };
            CloseLotWindow.ResizeMode = ResizeMode.NoResize;
            CloseLotWindow.ShowDialog();
        }

        /// <summary>
        /// Close and Reset Lot
        /// </summary>
        private DelegateCommand _resetCommand;
        public DelegateCommand ResetCommand =>
            _resetCommand ?? (_resetCommand =
                new DelegateCommand(ResetCommandExecute).ObservesCanExecute(() => LpEnable));
        private void ResetCommandExecute()
        {
            ProcessModel.CloseResetLot();
           
            for (int i = OpenLotTableLPVM.Rows.Count - 1; i >= 0; i--)
            {
                DataRow dr = OpenLotTableLPVM.Rows[i];
                dr.Delete();
            }
            OpenLotTableLPVM.AcceptChanges();

        }

        private DelegateCommand _closeopenLotCommand;
        public DelegateCommand CloseOpenLotCommand =>
            _closeopenLotCommand ?? (_closeopenLotCommand =
                new DelegateCommand(CloseOpenLotCommandExecute).ObservesCanExecute(() => LpEnable));
        private void CloseOpenLotCommandExecute()
        {
            ProcessModel.CloseOpenLot();
            _eventAggregator.GetEvent<LotRealTimeEvents>().Publish((int)ClassCommon.LotEvent.CloseLot);
        }

        public LotProcessViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            LpEnable = false;

             ProcessModel = new LotProcessModel(_eventAggregator);
            LotSettings();
            OpenLotTableLPVM =  ProcessModel.GetOpenLotsTable((int)ClassCommon.InstanceType.OpenLot);

            ClassCommon.OpenTD = OpenLotTableLPVM.Rows[0].Field<DateTime>("OpenTD");
            ClassCommon.CloseTD = OpenLotTableLPVM.Rows[0].Field<DateTime>("OpenTD"); //used this for now!, when start
            ClassCommon.LotNumber = (int)OpenLotTableLPVM.Rows[0]["LotNum"];
            ClassCommon.StockName = (string)OpenLotTableLPVM.Rows[0]["StockName"];
            ClassCommon.BaleCount = (short)OpenLotTableLPVM.Rows[0]["BaleCount"];
            ClassCommon.TotalNetWeight = (double)OpenLotTableLPVM.Rows[0]["TotNW"];
            ClassCommon.MaxNetWeight = (float)OpenLotTableLPVM.Rows[0]["MaxNW"];
            ClassCommon.MinNetWeight = (float)OpenLotTableLPVM.Rows[0]["MinNW"];
            ClassCommon.MinMC = (float)OpenLotTableLPVM.Rows[0]["MinMC"];
            ClassCommon.MaxMC = (float)OpenLotTableLPVM.Rows[0]["MaxMC"];
            

            //Startup value
          //  _eventAggregator.GetEvent<LotProcessEnable>().Publish(LpEnable);

            _eventAggregator.GetEvent<SaveFieldsEvent>().Subscribe(SaveModifyFields);
            _eventAggregator.GetEvent<CancelFieldsEvent>().Subscribe(CancelModifyFields);
        }

        private void CancelModifyFields(int obj)
        {
            switch (obj)
            {
                case (int)ClassCommon.InstanceType.OpenLot:
                    if (OpenLotWindow != null)
                    {
                        OpenLotWindow.Close();
                        OpenLotWindow = null;
                    }
                    break;
                case (int)ClassCommon.InstanceType.CloseLot:
                    if (CloseLotWindow != null)
                    {
                        CloseLotWindow.Close();
                        CloseLotWindow = null;
                    }
                    break;
            }
        }

        private void SaveModifyFields(int obj)
        {
            switch (obj)
            {
                case (int)ClassCommon.InstanceType.OpenLot:
                    if(OpenLotWindow != null)
                    {
                        OpenLotWindow.Close();
                        OpenLotWindow = null;
                    }
                    break;
                case (int)ClassCommon.InstanceType.CloseLot:
                    if( CloseLotWindow != null)
                    {
                        CloseLotWindow.Close();
                        CloseLotWindow = null;
                    }
                    break;
            }
        }

        private void LotSettings()
        {
            if (LpEnable)
            {
               
                switch (Settings.Default.LpResetMode)
                {
                    case 0:
                        LCRollOver = true;
                        break;
                    case 1:
                        LCDayEnd = true;
                        break;
                    case 2:
                        LCMonthEnd = true;
                        break;
                    case 3:
                        LCLotSize = true;
                        break;
                    case 4:
                        LCMaunal = true;
                        break;
                    default:
                        LCRollOver = true;
                        break;
                }
                switch (Settings.Default.LotType)
                {
                    case 0:
                        SingleLot = true;
                        break;

                    case 1:
                        MultipleLot = true;
                        break;

                    default:
                        SingleLot = true;
                        break;
                }
            }
        }
    }
}
