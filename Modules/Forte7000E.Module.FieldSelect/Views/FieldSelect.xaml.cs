using Forte7000E.Module.FieldSelect.ViewModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Forte7000E.Module.FieldSelect.Views
{
    /// <summary>
    /// Interaction logic for FieldSelect.xaml
    /// </summary>
    public partial class FieldSelect : UserControl
    {
        public static FieldSelect fieldSelectx;
        protected readonly Prism.Events.IEventAggregator _eventAggregator;
        private readonly FieldSelectViewModel _fieldselectviewmodel;

        public FieldSelect(IEventAggregator eventAggregator, int ilottype)
        {
            InitializeComponent();
            _eventAggregator = eventAggregator;
            fieldSelectx = this;

            _fieldselectviewmodel = new FieldSelectViewModel(_eventAggregator, ilottype);
            this.DataContext = _fieldselectviewmodel;
        }

        private void LeftClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedHdrList.SelectedIndex > 0)
                {
                    ObservableCollection<string> newlist = (ObservableCollection<string>)SelectedHdrList.ItemsSource;
                    int NewIndex = SelectedHdrList.SelectedIndex - 1;

                    if ((NewIndex > -1) || (NewIndex >= SelectedHdrList.Items.Count))
                    {
                        object selected = SelectedHdrList.SelectedItem;

                        // Removing removable element ItemsControl.ItemsSource
                        newlist.Remove(selected.ToString());
                        // Insert it in new position
                        newlist.Insert(NewIndex, selected.ToString());
                        // Restore selection
                        _fieldselectviewmodel.SelectHdrItems = newlist;

                        //SelectHdrItems.SelectedItem = selected;
                        SelectedHdrList.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in LeftClick " + ex.Message);
                //   MainWindow.AppWindows.LogObject.LogMessage(MsgTypes.WARNING, MsgSources.APPDROPPROFILE, "LeftClick " + ex.Message);
            }

        }

        private void RightClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((SelectedHdrList.SelectedIndex > -1) & (SelectedHdrList.SelectedIndex + 1 < SelectedHdrList.Items.Count))
                {
                    ObservableCollection<string> newlist = (ObservableCollection<string>)SelectedHdrList.ItemsSource;
                    int NewIndex = SelectedHdrList.SelectedIndex + 1;
                    object selected = SelectedHdrList.SelectedItem;

                    // Removing removable element ItemsControl.ItemsSource
                    newlist.Remove(selected.ToString());
                    // Insert it in new position
                    newlist.Insert(NewIndex, selected.ToString());

                    _fieldselectviewmodel.SelectHdrItems = newlist;
                    SelectedHdrList.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in RightClick " + ex.Message);
                //MainWindow.AppWindows.LogObject.LogMessage(MsgTypes.WARNING, MsgSources.APPBALEREALTIME, "RightClick" + ex.Message);
            }
        }
    }
}
