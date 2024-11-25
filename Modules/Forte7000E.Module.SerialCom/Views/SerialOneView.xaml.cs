using Forte7000E.Module.SerialCom.ViewModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Forte7000E.Module.SerialCom.Views
{
    /// <summary>
    /// Interaction logic for SerialOneView.xaml
    /// </summary>
    public partial class SerialOneView : UserControl
    {
        protected readonly IEventAggregator _eventAggregator;

        public SerialOneView(IEventAggregator EventAggregator)
        {
            InitializeComponent();
            this._eventAggregator = EventAggregator;
            this.DataContext = new SerialOneViewModel(_eventAggregator);
        }
    }
}
