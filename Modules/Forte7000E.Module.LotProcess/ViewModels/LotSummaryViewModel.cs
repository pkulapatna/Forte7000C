using Forte7000E.Module.LotProcess.Models;
using Forte7000E.Module.LotProcess.Properties;
using Forte7000E.Module.LotProcess.Views;
using Forte7000E.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Forte7000E.Module.LotProcess.ViewModels
{
    public class LotSummaryViewModel : BindableBase
    {
        protected readonly IEventAggregator _eventAggregator;
        private readonly LotProcessModel _lotProcessModel;

        private Window OpenLotWindow;
        private Window CloseLotWindow;

        private bool _userLogon;
        public bool UserLogon
        {
            get => _userLogon;
            set => SetProperty(ref _userLogon, value);
        }


        private DataTable _openLotTable;
        public DataTable OpenLotTable
        {
            get => _openLotTable;
            set => SetProperty(ref _openLotTable, value);
        }
        private DataTable _closeLotTable;
        public DataTable CloseLotTable
        {
            get => _closeLotTable;
            set => SetProperty(ref _closeLotTable, value);
        }

        private int _selectIdxLotClose;
        public int SelectIdxLotClose
        {
            get => _selectIdxLotClose;
            set => SetProperty(ref _selectIdxLotClose, value);
        }
        private int _selectIdxLotOpen;
        public int SelectIdxLotOpen
        {
            get => _selectIdxLotOpen;
            set => SetProperty(ref _selectIdxLotOpen, value);
        }


        private DelegateCommand _closeLotFieldsCommand;
        public DelegateCommand CloseLotFieldsCommand =>
            _closeLotFieldsCommand ?? (_closeLotFieldsCommand =
                new DelegateCommand(CloseLotFieldsCommandExecute));
        private void CloseLotFieldsCommandExecute()
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

        private DelegateCommand _openLotFieldsCommand;
        public DelegateCommand OpenLotFieldsCommand =>
            _openLotFieldsCommand ?? (_openLotFieldsCommand =
                new DelegateCommand(OpenLotFieldsCommandExecute));
        private void OpenLotFieldsCommandExecute()
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

        private DelegateCommand _closeopenLotCommand;
        public DelegateCommand CloseOpenLotCommand =>
            _closeopenLotCommand ?? (_closeopenLotCommand =
                new DelegateCommand(CloseOpenLotCommandExecute));
        private void CloseOpenLotCommandExecute()
        {
            _lotProcessModel.CloseOpenLot();
            _eventAggregator.GetEvent<LotRealTimeEvents>().Publish((int)ClassCommon.LotEvent.CloseLot);
            //LotEventsHandle((int)ClassCommon.LotEvent.CloseLot);
        }

        private DelegateCommand _resetCommand;
        public DelegateCommand ResetCommand =>
            _resetCommand ?? (_resetCommand =
                new DelegateCommand(ResetCommandExecute));
        private void ResetCommandExecute()
        {
            _lotProcessModel.CloseResetLot();
        }

        public LotSummaryViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            UserLogon = false;

            _lotProcessModel = new LotProcessModel(_eventAggregator);

            OpenLotTable = _lotProcessModel.GetOpenLotsTable((int)ClassCommon.InstanceType.OpenLot); 
            CloseLotTable = _lotProcessModel.CloseLotTable;

            _eventAggregator.GetEvent<UpdateLotData>().Subscribe(NewLotDataReady);
            _eventAggregator.GetEvent<LotRealTimeEvents>().Subscribe(LotEventsHandle);

            _eventAggregator.GetEvent<UserLoginEvent>().Subscribe(UserLogin);
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

        private void UserLogin(bool obj)
        {
            UserLogon = obj;
        }

        private void LotEventsHandle(int obj)
        {
            if( obj ==  (int)ClassCommon.LotEvent.CloseLot)
            {
                _lotProcessModel.CloseOpenLot();
                for (int i = OpenLotTable.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow dr = OpenLotTable.Rows[i];
                    dr.Delete();
                }
                OpenLotTable.AcceptChanges();
            }
        }

        private void NewLotDataReady(int obj)
        {
            if (obj == (int)ClassCommon.InstanceType.OpenLot)
                OpenLotTable = _lotProcessModel.GetSqlLotTable((int)ClassCommon.InstanceType.OpenLot);
            
            if(obj == (int)ClassCommon.InstanceType.CloseLot)
                CloseLotTable = _lotProcessModel.GetCustomLotArchiveTable((int)ClassCommon.InstanceType.CloseLot);

        }
    }
}
