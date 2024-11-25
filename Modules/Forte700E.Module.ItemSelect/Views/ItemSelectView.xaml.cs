using Forte7000E.Services;
using Forte700E.Module.ItemSelect.ViewModels;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Forte700E.Module.ItemSelect.Views
{
    /// <summary>
    /// Interaction logic for ItemSelect.xaml
    /// </summary>
    public partial class ItemSelectView : UserControl
    {
        private readonly ItemSelectViewModel ItemsViewModel;
        protected readonly Prism.Events.IEventAggregator _eventAggregator;

        public ItemSelectView(int InstID, IEventAggregator ea)
        {
            InitializeComponent();
            _eventAggregator = ea;
            
            ItemsViewModel = new ItemSelectViewModel(InstID,_eventAggregator);
            this.DataContext = ItemsViewModel;

       
        }

        private void AsciiText_dclick(object sender, MouseButtonEventArgs e)
        {
            if (AsciiText.IsReadOnly == false)
            {
                AsciiText.Background = Brushes.AntiqueWhite;
                AsciiText.IsReadOnly = true;
            }
            else
            {
                AsciiText.Background = Brushes.White;
                AsciiText.IsReadOnly = false;
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((ItemsViewModel.SelectDelIndex > -1) & (ItemsViewModel.SelectDelIndex + 1 < SelectedHdrList.Items.Count))
                {
                    ObservableCollection<DataOutput> newlist = (ObservableCollection<DataOutput>)ItemsViewModel.SerialOutOne;
                    int NewIndex = ItemsViewModel.SelectDelIndex + 1;
                    object selected = ItemsViewModel.SelectDelItem;

                    if (NewIndex < SelectItemList.Items.Count)
                    {
                        // Removing removable element ItemsControl.ItemsSource
                        newlist.RemoveAt(ItemsViewModel.SelectDelIndex);
                        // Insert it in new position
                        newlist.Insert(NewIndex, (DataOutput)selected);
                        // Restore selection
                        ItemsViewModel.SerialOutOne = newlist;
                        SelectedHdrList.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in MoveDown_Click " + ex.Message);
            }
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ItemsViewModel.SelectDelIndex > -1)
                {
                    ObservableCollection<DataOutput> newlist = (ObservableCollection<DataOutput>)ItemsViewModel.SerialOutOne;
                    int NewIndex = ItemsViewModel.SelectDelIndex - 1;

                    if ((NewIndex > -1) || (NewIndex >= SelectedHdrList.Items.Count))
                    {
                        object selected = ItemsViewModel.SelectDelItem;

                        // Removing removable element ItemsControl.ItemsSource
                        newlist.RemoveAt(ItemsViewModel.SelectDelIndex);
                        // Insert it in new position
                        newlist.Insert(NewIndex, (DataOutput)selected);
                        // Restore selection
                        ItemsViewModel.SerialOutOne = newlist;
                        SelectedHdrList.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in MoveUp_Click " + ex.Message);
            }
        }
    }
}
