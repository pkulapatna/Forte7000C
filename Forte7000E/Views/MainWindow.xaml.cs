using ClsErrorLog;
using Forte7000C.Properties;
using Forte7000C.Viewmodels;
using Prism.Events;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Forte7000E.Services;
using Forte7000E.Module.LotProcess.Views;

namespace Forte7000C.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected readonly IEventAggregator _eventAggregator = new EventAggregator();

        public static MainWindow AppWindows;
        private readonly System.Threading.Mutex objMutex = null;
        private readonly MainWindowsViewModel _mainWindowsViewModel;

        private double IScreenWidth { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            AppWindows = this;

            objMutex = new System.Threading.Mutex(false, "Forte7000C.exe");
            if (!objMutex.WaitOne(0, false))
            {
                objMutex.Close();
                objMutex = null;
                MessageBox.Show("Another instance is already running!");
                AppWindows.Close();
                Thread.CurrentThread.Abort();
            }

            ClassCommon.MyInfoLog.SetTitle("Forte7000C");
            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.APPSTART, "Startup Forte7000C ..........................");

            _mainWindowsViewModel = new MainWindowsViewModel(_eventAggregator);
            DataContext = _mainWindowsViewModel;

     //       cbRndom.IsChecked = Settings.Default.bSimWtRandom;

            BaleTab.IsSelected = true;

            WeightEntry.Text = ClassCommon.DSWeightEntry;
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            DataGridControl.Columns[0].Visibility = Visibility.Collapsed;

            DataGridControl.SelectedIndex = 0;
            DataGridControl.Focus();

            if (e.PropertyName.StartsWith("Moisture"))
            {
                switch (ClassCommon.MoistureType)
                {
                    case 0: // %MC
                        e.Column.Header = "MC %";
                        break;

                    case 1: // %MR
                        e.Column.Header = "MR %";
                        break;

                    case 2: // %AD
                        e.Column.Header = "AD %";
                        break;

                    case 3: // %BD
                        e.Column.Header = "BD %";
                        break;
                }
            }

            if (e.PropertyName.StartsWith("Weight"))
            {
                if(ClassCommon.WeightType == ClassCommon.WtMetric) e.Column.Header = "Weight kg.";
                if (ClassCommon.WeightType == ClassCommon.WtEnglish) e.Column.Header = "Weight lb.";
            }

            if (e.PropertyType == typeof(System.DateTime))
            e.Column.ClipboardContentBinding.StringFormat = "MM:dd:yyyy HH:mm";

            if ((e.PropertyType == typeof(System.Single)) || (e.PropertyType == typeof(System.Double)))
            {
                e.Column.ClipboardContentBinding.StringFormat = "{0:0.##}";
                e.Column.Width = e.Column.Header.ToString().Length * 25;
            }
        }

      

        private void MainWin_SideChanged(object sender, SizeChangedEventArgs e)
        {
            double dBoxFontSize = e.NewSize.Width * 0.08;
            double dColHdrHeight = e.NewSize.Width * 0.03;
            double iGVFontsize = e.NewSize.Width * 0.015;

          //  double ilotFontsize = e.NewSize.Width * 0.012;
          //  double dLotColHdrHeight = e.NewSize.Width * 0.025;

            DataGridControl.FontSize = iGVFontsize;
            DataGridControl.ColumnHeaderHeight = dColHdrHeight;
            Moisture.FontSize = dBoxFontSize;
            Weight.FontSize = dBoxFontSize;
        }

       
        private void OnClosed(object sender, EventArgs e)
        {
           Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.SHUTDOWN, "Closing Forte7000C ..........................");

            e.Cancel = true;
        }

        private void NumericOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }

        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9.]+");
            return reg.IsMatch(str);
        }

      

        private void BtnInit_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                e.Handled = true;

                if ((_mainWindowsViewModel.HotKeyChecked) && (tabHome.IsSelected))
                {
                    if (_btnInit.IsEnabled)
                    {
                        ButtonAutomationPeer peer = new ButtonAutomationPeer(_btnInit);
                        IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                        invokeProv.Invoke();
                    }

                    if (btnRead.IsEnabled)
                    {
                        ButtonAutomationPeer peer = new ButtonAutomationPeer(btnRead);
                        IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                        invokeProv.Invoke();
                    }
                }
            }
        }

        private void SampleBox_dclick(object sender, MouseButtonEventArgs e)
        {
            if (Serial.IsReadOnly == false)
            {
                Serial.Background = Brushes.AntiqueWhite;
                Serial.IsReadOnly = true;
            }
            else
            {
                Serial.Background = Brushes.White;
                Serial.IsReadOnly = false;
            }
        }

        private void StockName_DClick(object sender, MouseButtonEventArgs e)
        {
            if (StockName.IsReadOnly == false)
            {
                StockName.Background = Brushes.AntiqueWhite;
                StockName.IsReadOnly = true;
            }
            else
            {
                StockName.Background = Brushes.White;
                StockName.IsReadOnly = false;
            }
        }

        private void MtLimitLow_dlcick(object sender, MouseButtonEventArgs e)
        {
            if (MtLimitLow.IsReadOnly == false)
            {
                MtLimitLow.Background = Brushes.AntiqueWhite;
                MtLimitLow.IsReadOnly = true;
            }
            else
            {
                MtLimitLow.Background = Brushes.White;
                MtLimitLow.IsReadOnly = false;
            }
        }

       
        private void MtLimitHi_dclick(object sender, MouseButtonEventArgs e)
        {
            if (MtLimitHi.IsReadOnly == false)
            {
                MtLimitHi.Background = Brushes.AntiqueWhite;
                MtLimitHi.IsReadOnly = true;
            }
            else
            {
                MtLimitHi.Background = Brushes.White;
                MtLimitHi.IsReadOnly = false;
            }
        }

        private void WtLimitLow_dclick(object sender, MouseButtonEventArgs e)
        {
            if (WtLimitLow.IsReadOnly == false)
            {
                WtLimitLow.Background = Brushes.AntiqueWhite;
                WtLimitLow.IsReadOnly = true;
            }
            else
            {
                WtLimitLow.Background = Brushes.White;
                WtLimitLow.IsReadOnly = false;
            }
        }

        private void WtLimitHi_dclick(object sender, MouseButtonEventArgs e)
        {
            if (WtLimitHi.IsReadOnly == false)
            {
                WtLimitHi.Background = Brushes.AntiqueWhite;
                WtLimitHi.IsReadOnly = true;
            }
            else
            {
                WtLimitHi.Background = Brushes.White;
                WtLimitHi.IsReadOnly = false;
            }
        }

        private void DefWeight_dclick(object sender, MouseButtonEventArgs e)
        {
            if (DefWeight.IsReadOnly == false)
            {
                DefWeight.Background = Brushes.AntiqueWhite;
                DefWeight.IsReadOnly = true;
            }
            else
            {
                DefWeight.Background = Brushes.White;
                DefWeight.IsReadOnly = false;
            }
        }

        private void DefNetWt_dclick(object sender, MouseButtonEventArgs e)
        {
            if (DefNetWt.IsReadOnly == false)
            {
                DefNetWt.Background = Brushes.AntiqueWhite;
                DefNetWt.IsReadOnly = true;
            }
            else
            {
                DefNetWt.Background = Brushes.White;
                DefNetWt.IsReadOnly = false;
            }
        }

        private void DefForte_dclick(object sender, MouseButtonEventArgs e)
        {
            if (DefForte.IsReadOnly == false)
            {
                DefForte.Background = Brushes.AntiqueWhite;
                DefForte.IsReadOnly = true;
            }
            else
            {
                DefForte.Background = Brushes.White;
                DefForte.IsReadOnly = false;
            }
        }

        private void DefMoisture_dclick(object sender, MouseButtonEventArgs e)
        {
            if (DefMoisture.IsReadOnly == false)
            {
                DefMoisture.Background = Brushes.AntiqueWhite;
                DefMoisture.IsReadOnly = true;
            }
            else
            {
                DefMoisture.Background = Brushes.White;
                DefMoisture.IsReadOnly = false;
            }
        }

        private void FieldTwo_dclick(object sender, MouseButtonEventArgs e)
        {
            if (FieldTwo.IsReadOnly == false)
            {
                FieldTwo.Background = Brushes.AntiqueWhite;
                FieldTwo.IsReadOnly = true;
            }
            else
            {
                FieldTwo.Background = Brushes.White;
                FieldTwo.IsReadOnly = false;
            }
        }

        private void FieldThree_dclcik(object sender, MouseButtonEventArgs e)
        {
            if (FieldThree.IsReadOnly == false)
            {
                FieldThree.Background = Brushes.AntiqueWhite;
                FieldThree.IsReadOnly = true;
            }
            else
            {
                FieldThree.Background = Brushes.White;
                FieldThree.IsReadOnly = false;
            }
        }

        private void FieldFour_dclick(object sender, MouseButtonEventArgs e)
        {
            if (FieldFour.IsReadOnly == false)
            {
                FieldFour.Background = Brushes.AntiqueWhite;
                FieldFour.IsReadOnly = true;
            }
            else
            {
                FieldFour.Background = Brushes.White;
                FieldFour.IsReadOnly = false;
            }
        }

        private void FieldOne_dclick(object sender, MouseButtonEventArgs e)
        {
            if (FieldOne.IsReadOnly == false)
            {
                FieldOne.Background = Brushes.AntiqueWhite;
                FieldOne.IsReadOnly = true;
            }
            else
            {
                FieldOne.Background = Brushes.White;
                FieldOne.IsReadOnly = false;
            }
        }

        private void Serialmax_dlcick(object sender, MouseButtonEventArgs e)
        {
            if (Serialmax.IsReadOnly == false)
            {
                Serialmax.Background = Brushes.AntiqueWhite;
                Serialmax.IsReadOnly = true;
            }
            else
            {
                Serialmax.Background = Brushes.White;
                Serialmax.IsReadOnly = false;
            }
        }


        private void PackShiftOne_DClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void PickEndDay_DClick(object sender, MouseButtonEventArgs e)
        {
            if (PickEndDay.IsReadOnly == false)
            {
                PickEndDay.Background = Brushes.AntiqueWhite;
                PickEndDay.IsReadOnly = true;
            }
            else
            {
                PickEndDay.Background = Brushes.White;
                PickEndDay.IsReadOnly = false;
            }
        }

        private void PickShiftOne_DClick(object sender, MouseButtonEventArgs e)
        {
            if (PickShiftOne.IsReadOnly == false)
            {
                PickShiftOne.Background = Brushes.AntiqueWhite;
                PickShiftOne.IsReadOnly = true;
            }
            else
            {
                PickShiftOne.Background = Brushes.White;
                PickShiftOne.IsReadOnly = false;
            }
        }

        private void PickShiftTwo_DClick(object sender, MouseButtonEventArgs e)
        {
            if (PickShiftTwo.IsReadOnly == false)
            {
                PickShiftTwo.Background = Brushes.AntiqueWhite;
                PickShiftTwo.IsReadOnly = true;
            }
            else
            {
                PickShiftTwo.Background = Brushes.White;
                PickShiftTwo.IsReadOnly = false;
            }
        }

        private void PickShiftThree_DClick(object sender, MouseButtonEventArgs e)
        {
            if (PickShiftThree.IsReadOnly == false)
            {
                PickShiftThree.Background = Brushes.AntiqueWhite;
                PickShiftThree.IsReadOnly = true;
            }
            else
            {
                PickShiftThree.Background = Brushes.White;
                PickShiftThree.IsReadOnly = false;
            }
        }

        private void ShiftOneName_DClick(object sender, MouseButtonEventArgs e)
        {
            if (ShiftOneName.IsReadOnly == false)
            {
                ShiftOneName.Background = Brushes.AntiqueWhite;
                ShiftOneName.IsReadOnly = true;
            }
            else
            {
                ShiftOneName.Background = Brushes.White;
                ShiftOneName.IsReadOnly = false;
            }
        }

        private void ShiftTwoName_DClick(object sender, MouseButtonEventArgs e)
        {
            if (ShiftTwoName.IsReadOnly == false)
            {
                ShiftTwoName.Background = Brushes.AntiqueWhite;
                ShiftTwoName.IsReadOnly = true;
            }
            else
            {
                ShiftTwoName.Background = Brushes.White;
                ShiftTwoName.IsReadOnly = false;
            }
        }

        private void ShiftThreeName_DClick(object sender, MouseButtonEventArgs e)
        {
            if (ShiftThreeName.IsReadOnly == false)
            {
                ShiftThreeName.Background = Brushes.AntiqueWhite;
                ShiftThreeName.IsReadOnly = true;
            }
            else
            {
                ShiftThreeName.Background = Brushes.White;
                ShiftThreeName.IsReadOnly = false;
            }
        }

        private void TareWeight_DClick(object sender, MouseButtonEventArgs e)
        {
            if (TareWeight.IsReadOnly == false)
            {
                TareWeight.Background = Brushes.AntiqueWhite;
                TareWeight.IsReadOnly = true;
            }
            else
            {
                TareWeight.Background = Brushes.White;
                TareWeight.IsReadOnly = false;
            }

        }

        private void SharedFileName_dclick(object sender, MouseButtonEventArgs e)
        {
            if (SharedFileName.IsReadOnly == false)
            {
                SharedFileName.Background = Brushes.AntiqueWhite;
                SharedFileName.IsReadOnly = true;
            }
            else
            {
                SharedFileName.Background = Brushes.White;
                SharedFileName.IsReadOnly = false;
            }
        }

        private void BaleSumDepth_dclick(object sender, MouseButtonEventArgs e)
        {
            if (BaleSumDepth.IsReadOnly == false)
            {
                BaleSumDepth.Background = Brushes.AntiqueWhite;
                BaleSumDepth.IsReadOnly = true;
            }
            else
            {
                BaleSumDepth.Background = Brushes.White;
                BaleSumDepth.IsReadOnly = false;
            }
        }


        private void NextLotNum_dclick(object sender, MouseButtonEventArgs e)
        {
            if (NextLotNum.IsReadOnly == false)
            {
                NextLotNum.Background = Brushes.AntiqueWhite;
                NextLotNum.IsReadOnly = true;
            }
            else
            {
                NextLotNum.Background = Brushes.White;
                NextLotNum.IsReadOnly = false;
            }
        }

        private void Grid_mouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void WeightEntry_dclick(object sender, MouseButtonEventArgs e)
        {
            if (WeightEntry.IsReadOnly == false)
            {
                WeightEntry.Background = Brushes.AntiqueWhite;
                WeightEntry.IsReadOnly = true;
                ClassCommon.DSWeightEntry = WeightEntry.Text;
            }
            else
            {
                WeightEntry.Background = Brushes.White;
                WeightEntry.IsReadOnly = false;
                
            }

        }
    }
}
