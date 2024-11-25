using Forte7000E.Services.Properties;
using Prism.Events;
using QurtzLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Forte7000E.Services
{
    public class QuartzTimer
    {

        protected readonly IEventAggregator _eventAggregator;
        private static readonly object padlock = new object();
        private static QuartzTimer instance = null;
        private readonly QuartzSched MySched;

        public int PeriodScan
        {
            get => Settings.Default.periodScan;
            set
            {
                Settings.Default.periodScan = value;
                Settings.Default.Save();
            }
        }
        public static QuartzTimer Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new QuartzTimer(ApplicationService.Instance.EventAggregator);
                    }
                    return instance;
                }
            }
        }

        public QuartzTimer(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
            MySched = QuartzSched.Instance;
        }


        public void StartPeriodQuart()
        {
            MySched.StartPeriodQuartz(PeriodScan);
            MySched.RaisePeriodEvent += MySched_RaisePeriodEvent;
        }

        private void MySched_RaisePeriodEvent(object sender, string e)
        {
            _eventAggregator?.GetEvent<QuartzEvents>().Publish(e);
        }

        public void StartDayEndQuart(string endhr, string endMin)
        {
            MySched.StartDayEndQuartz(endhr, endMin);
            MySched.RaiseDayEndEvent += MySched_RaiseDayEndEvent;
        }

        private void MySched_RaiseDayEndEvent(object sender, string e)
        {
            _eventAggregator?.GetEvent<QuartzDayEbdEvents>().Publish(e);
        }



        public void StopQuartz()
        {
            if (MySched != null)
            {
                MySched.StopQuartz();
                MySched.RaisePeriodEvent -= MySched_RaisePeriodEvent;
                MySched.RaiseDayEndEvent -= MySched_RaiseDayEndEvent;
            }
        }

       
    }
}
