using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forte7000E.Services
{
    public class EventsTimers
    {
        protected readonly Prism.Events.IEventAggregator _eventAggregator;
        //Main
        private System.Windows.Threading.DispatcherTimer MainEventsTimer;
        //Print
        private System.Windows.Threading.DispatcherTimer PrintEventTimer;

        public EventsTimers(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// System.Windows.Threading.DispatcherTimer
        /// </summary> ////////////////////////////////////////////////////////////////////////////////////////////
        public void InitializeMainEventsTimer(string EventTag)
        {
            if (MainEventsTimer != null) MainEventsTimer = null;
            MainEventsTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60)
            };
            MainEventsTimer.Tick += new EventHandler(MainEventsTimer_Tick);
            MainEventsTimer.Tag = EventTag;
            MainEventsTimer.Start();
        }

        /// <summary>
        /// System.Windows.Threading.DispatcherTimer.Tick handler
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainEventsTimer_Tick(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { SetEventActions(DateTime.Now); }));
        }
        private void SetEventActions(DateTime timenow)
        {
            _eventAggregator.GetEvent<UpdateMainTimerEvents>().Publish(timenow);
        }
        
        public void StopMainEventsTimer()
        {
            if(MainEventsTimer != null)
            {
                MainEventsTimer.Stop();
                MainEventsTimer = null;
            }
        }

        
        /// <summary>
        /// Print Events
        /// </summary>
        /// <param name="prnId"></param> /////////////////////////////////////////////////////////////////////////
        int EvnPrint = 0;
        public void InitializePrintEventTimer(int prnId)
        {
            if (PrintEventTimer != null) PrintEventTimer = null;
            PrintEventTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60)
            };
            PrintEventTimer.Tick += new EventHandler(PrintEventTimer_Tick);
            PrintEventTimer.Tag = prnId.ToString();
            PrintEventTimer.Start();
            EvnPrint = prnId;
        }
        private void PrintEventTimer_Tick(object sender, EventArgs e)
        {
            PrintEventTimer.Stop();
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { PrintEvents(EvnPrint); }));
        }
        private void PrintEvents(int evnPrint)
        {
            _eventAggregator.GetEvent<PrintScheduleEvent>().Publish(evnPrint);  
        }

        public void StopAllEventsTimers()
        {
            if (MainEventsTimer != null) MainEventsTimer = null;
            if (PrintEventTimer != null) PrintEventTimer = null;
           
        }
    }
}
