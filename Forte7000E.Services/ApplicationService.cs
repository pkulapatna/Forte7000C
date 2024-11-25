using Prism.Events;
using System;

namespace Forte7000E.Services
{

    public sealed class ApplicationService
    {
        private ApplicationService() { }
        private static readonly ApplicationService _instance = new ApplicationService();
        internal static ApplicationService Instance { get { return _instance; } }

        private Prism.Events.IEventAggregator _eventAggregator;
        internal Prism.Events.IEventAggregator EventAggregator
        {
            get
            {
                if (_eventAggregator == null)
                    _eventAggregator = new Prism.Events.EventAggregator();

                return _eventAggregator;
            }
        }
    }

    /// <summary>
    /// send bool event from class ApplicationService
    /// </summary>
    public class UpdateSqldataEvent : PubSubEvent<string>
    {
    }
  
    public class UpdateAllEvent : PubSubEvent<int>
    {
    }
   
    public class UpdateStringOutput : PubSubEvent<int>
    {
    }
    public class UpdateLotData : PubSubEvent<int>
    {
    }
    public class LotProcessEnable : PubSubEvent<bool>
    {
    }
    public class LotRealTimeEvents : PubSubEvent<int>
    {
    }

    public class ScaleReqString : PubSubEvent<string>
    {
    }
    public class ScaleErrorEvent : PubSubEvent<int>
    {
    }
    public class ScaleRetryEvent : PubSubEvent<int>
    {
    }
    public class UpdateScaledataEvent : PubSubEvent<string>
    {
    }
    public class ScaleTestDataReturn : PubSubEvent<string>
    {  
    }
    public class SetScaleMode : PubSubEvent<string>
    {
    }
    /// <summary>
    /// Osc Messages ------------------------------------------
    /// </summary>
    public class OscErrorEvent : PubSubEvent<int>
    {
    }
    public class OscRetryEvent : PubSubEvent<int>
    {
    }


    public class UpdateOscdataEvent : PubSubEvent<string>
    {
    }
    public class SendOscdataEvent : PubSubEvent<double>
    {
    }
    public class OscTestDataReturn : PubSubEvent<string>
    {
    }
    public class SetOscMode : PubSubEvent<string>
    {
    }
    //------------------------------------------------------


    public class QuartzScheduler : PubSubEvent<DateTime>
    {
    }
    public class UpdateMainTimerEvents : PubSubEvent<DateTime>
    {
    }
    public class PrintScheduleEvent : PubSubEvent<int>
    {
    }

    public class UserLoginEvent : PubSubEvent<bool>
    {
    }

    public class SaveFieldsEvent : PubSubEvent<int>
    {
    }
    public class CancelFieldsEvent : PubSubEvent<int>
    {
    }
    public class SaveItemSelectEvent : PubSubEvent<int>
    {
    }

    public class QuartzDayEbdEvents : PubSubEvent<string>
    {

    }

    public class QuartzEvents : PubSubEvent<string>
    {

    }
}
