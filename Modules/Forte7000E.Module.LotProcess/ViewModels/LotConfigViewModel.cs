using Forte7000E.Module.LotProcess.Models;
using Forte7000E.Module.LotProcess.Properties;
using Forte7000E.Services;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forte7000E.Module.LotProcess.ViewModels
{
    public class LotConfigViewModel : BindableBase
    {
        protected readonly IEventAggregator _eventAggregator;
        private readonly LotProcessModel ProcessModel;


        private bool _LpEnable = ClassCommon.LotEnable;
        public bool LpEnable
        {
            get { return _LpEnable; }
            set
            {
                SetProperty(ref _LpEnable, value);
                ClassCommon.LotEnable = value;

                if (value) Lotopc = "1.0";
                else Lotopc = "0.2";
                // if (value) ClassCommon.LotStatus = "Open";
                _eventAggregator.GetEvent<LotProcessEnable>().Publish(value);
            }
        }

        //Lot Type Single Lot or Multiple ----------------------------------------
        public int LotType { get; set; }

        private bool _SingleLot;
        public bool SingleLot
        {
            get { return _SingleLot; }
            set
            {
                SetProperty(ref _SingleLot, value);
                if (value) ProcessModel.LotType = 0;
            }
        }
        private bool _MultipleLot;
        public bool MultipleLot
        {
            get { return _MultipleLot; }
            set
            {
                SetProperty(ref _MultipleLot, value);
                if (value) ProcessModel.LotType = 1;
            }
        }
        //Open Lot per Source Stock or Grade-------------------------------------
        public int OpenLot { get; set; }

        private bool openbySource;
        public bool OpenbySource
        {
            get { return openbySource; }
            set
            {
                SetProperty(ref openbySource, value);
                if (value) ProcessModel.OpenLot = 0;
            }
        }
        private bool openbyStock;
        public bool OpenbyStock
        {
            get { return openbyStock; }
            set
            {
                SetProperty(ref openbyStock, value);
                if (value) ProcessModel.OpenLot = 1;
            }
        }
        private bool openbyGrade;
        public bool OpenbyGrade
        {
            get { return openbyGrade; }
            set
            {
                SetProperty(ref openbyGrade, value);
                if (value) ProcessModel.OpenLot = 2;
            }
        }
        //Lot number Sequence Single, Independent Sequence, Independent overlap------------------

        public int LotSequence { get; set; }

        private bool singleSeq;
        public bool SingleSeq
        {
            get { return singleSeq; }
            set
            {
                SetProperty(ref singleSeq, value);
                if (value) ProcessModel.LotSequence = 0;
            }
        }
        private bool indSeq;
        public bool IndSeq
        {
            get { return indSeq; }
            set
            {
                SetProperty(ref indSeq, value);
                if (value) ProcessModel.LotSequence = 1;
            }
        }
        private bool indSeqNonLap;
        public bool IndSeqNonLap
        {
            get { return indSeqNonLap; }
            set
            {
                SetProperty(ref indSeqNonLap, value);
                if (value) ProcessModel.LotSequence = 2;
            }
        }
        //Lot closing Manual, Close all Auto, Close auto independent ------------
        public int LotClose { get; set; }

        private bool lotCloseManual;
        public bool LotCloseManual
        {
            get { return lotCloseManual; }
            set
            {
                SetProperty(ref lotCloseManual, value);
                if (value) ProcessModel.LotClose = 0;
            }
        }
        private bool lotCloseAllAuto;
        public bool LotCloseAllAuto
        {
            get { return lotCloseAllAuto; }
            set
            {
                SetProperty(ref lotCloseAllAuto, value);
                if (value) ProcessModel.LotClose = 1;
            }
        }
        private bool lotCloseInd;
        public bool LotCloseInd
        {
            get { return lotCloseInd; }
            set
            {
                SetProperty(ref lotCloseInd, value);
                if (value) ProcessModel.LotClose = 2;
            }
        }
        //Lot number Reset Rollover, Time ------------------------------
        public int LotReset { get; set; }

        private bool lotCloseRover;
        public bool LotCloseRover
        {
            get { return lotCloseRover; }
            set
            {
                SetProperty(ref lotCloseRover, value);
                if (value) ProcessModel.LotReset = 0;
            }
        }
        private bool lotCloseTime;
        public bool LotCloseTime
        {
            get { return lotCloseTime; }
            set
            {
                SetProperty(ref lotCloseTime, value);
                if (value) ProcessModel.LotReset = 1;
            }
        }
        //---------------------------------------------------------------

        private string lotopc;
        public string Lotopc
        {
            get => lotopc; 
            set => SetProperty(ref lotopc, value);
        }

        public LotConfigViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            ProcessModel = new LotProcessModel(_eventAggregator);
            Lotopc = "0";
            LpEnable = ClassCommon.LotEnable;

            if (Settings.Default.LotType == 0) SingleLot = true;
            if (Settings.Default.LotType == 1) MultipleLot = true;

            if (Settings.Default.OpenLot == 0) OpenbySource = true;
            if (Settings.Default.OpenLot == 1) OpenbyStock = true;
            if (Settings.Default.OpenLot == 2) openbyGrade = true;

            if (Settings.Default.LotSequence == 0) SingleSeq = true;
            if (Settings.Default.LotSequence == 1) IndSeq = true;
            if (Settings.Default.LotSequence == 2) IndSeqNonLap = true;

            if (Settings.Default.LotClose == 0) LotCloseManual = true;
            if (Settings.Default.LotClose == 1) LotCloseAllAuto = true;
            if (Settings.Default.LotClose == 2) LotCloseInd = true;

            if (Settings.Default.LotReset == 0) LotCloseRover = true;
            if (Settings.Default.LotReset == 1) LotCloseTime = true;

        }




    }
}
