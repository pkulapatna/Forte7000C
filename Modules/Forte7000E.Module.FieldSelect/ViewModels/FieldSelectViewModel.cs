using Forte7000E.Module.FieldSelect.Models;
using Forte7000E.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using Forte7000E.Module.FieldSelect.Views;

namespace Forte7000E.Module.FieldSelect.ViewModels
{
    public class FieldSelectViewModel : BindableBase, ICloseWindows
    {
        protected readonly Prism.Events.IEventAggregator _eventAggregator;
        private readonly FieldSelectModel FieldsModel;

        public DelegateCommand ClosedPageICommand { get; set; } //Close page
        public Action CloseAction { get; set; }
        public int InstanceID { get; set; }

        private ObservableCollection<string> _XmlItemsList;
        public ObservableCollection<string> XmlItemsList
        {
            get { return _XmlItemsList; }
            set { SetProperty(ref _XmlItemsList, value); }
        }

        private List<Tuple<string, string>> _SqlFleldsList;
        public List<Tuple<string, string>> SqlFleldsList
        {
            get { return _SqlFleldsList; }
            set { SetProperty(ref _SqlFleldsList, value); }
        }
        private string _SelectedS1Name;
        public string SelectedS1Name
        {
            get { return _SelectedS1Name; }
            set { SetProperty(ref _SelectedS1Name, value); }
        }
        private int _FieldSelectedIndex;
        public int FieldSelectedIndex
        {
            get { return _FieldSelectedIndex; }
            set { SetProperty(ref _FieldSelectedIndex, value); }
        }

        private List<string> _AsciiList;
        public List<string> AsciiList
        {
            get { return _AsciiList; }
            set { SetProperty(ref _AsciiList, value); }
        }

        private List<Tuple<string, char>> _ASCIItupleList;
        public List<Tuple<string, char>> ASCIItupleList
        {
            get { return _ASCIItupleList; }
            set { SetProperty(ref _ASCIItupleList, value); }
        }

        private int _SelectedAsciiIndex;
        public int SelectedAsciiIndex
        {
            get { return _SelectedAsciiIndex; }
            set { SetProperty(ref _SelectedAsciiIndex, value); }
        }

        private string _SelectedAscii;
        public string SelectedAscii
        {
            get { return _SelectedAscii; }
            set { SetProperty(ref _SelectedAscii, value); }
        }

        private bool _bS1Enable; // = Settings.Default.bS1Enable;
        public bool BS1Enable
        {
            get { return _bS1Enable; }
            set
            {
                SetProperty(ref _bS1Enable, value);
               // Settings.Default.bS1Enable = value;
              //  Settings.Default.Save();
                // if (value) UpdateSerialOneOut();
            }
        }

        //For Serial output data and format
        private ObservableCollection<DataOutput> _SerialOutOne;
        public ObservableCollection<DataOutput> SerialOutOne
        {
            get { return _SerialOutOne; }
            set { SetProperty(ref _SerialOutOne, value); }
        }

        private int _SelectDelIndex;
        public int SelectDelIndex
        {
            get { return _SelectDelIndex; }
            set { SetProperty(ref _SelectDelIndex, value); }
        }

        private object _SelectDelItem;
        public object SelectDelItem
        {
            get { return _SelectDelItem; }
            set { SetProperty(ref _SelectDelItem, value); }
        }

        private string _SerialOneOutString = " ";
        public string SerialOneOutString
        {
            get { return _SerialOneOutString; }
            set { SetProperty(ref _SerialOneOutString, value); }
        }

        //SelectDelItem

        private string _SelectedDelVal;
        public string SelectedDelVal
        {
            get { return _SelectedDelVal; }
            set { SetProperty(ref _SelectedDelVal, value); }
        }

        private DataTable _lvDatatable;
        public DataTable LvDatatable
        {
            get { return _lvDatatable; }
            set { SetProperty(ref _lvDatatable, value); }
        }

        private string _ASCii;
        public string ASCii
        {
            get { return _ASCii; }
            set { SetProperty(ref _ASCii, value); }
        }
       
        private ObservableCollection<CheckedListItem> _hdrListboxList;
        public ObservableCollection<CheckedListItem> AvailableHdrList
        {
            get { return _hdrListboxList; }
            set { SetProperty(ref _hdrListboxList, value); }
        }

        private bool _bOpenSetup = false;
        public bool OpenSetup
        {
            get { return _bOpenSetup; }
            set { SetProperty(ref _bOpenSetup, value); }
        }
  
        private ObservableCollection<string> _selectHdrItems;
        public ObservableCollection<string> SelectHdrItems
        {
            get { return _selectHdrItems; }
            set { SetProperty(ref _selectHdrItems, value); }
        }

        private DelegateCommand _ModifyCommand;
        public DelegateCommand ModifyCommand =>
        _ModifyCommand ?? (_ModifyCommand = new DelegateCommand(ModifyCommandExecute));
        private void ModifyCommandExecute()
        {
            OpenSetup = true;
        }

        private DelegateCommand _SaveUpdateCommand;
        public DelegateCommand SaveUpdateCommand =>
        _SaveUpdateCommand ?? (_SaveUpdateCommand =
            new DelegateCommand(SaveUpdateCommandExecute).ObservesCanExecute(() => OpenSetup));
        private void SaveUpdateCommandExecute()
        {
            FieldsModel.SaveModified_setting();
            FieldsModel.SaveXmlcolumnList(SelectHdrItems);
            OpenSetup = false;
            _eventAggregator.GetEvent<SaveFieldsEvent>().Publish(InstanceID);
            
           // Console.WriteLine("InstanceID = " + InstanceID);
        }

        private DelegateCommand _CancelCommand;
        public DelegateCommand CancelCommand =>
        _CancelCommand ?? (_CancelCommand =
            new DelegateCommand(CancelCommandExecute).ObservesCanExecute(() => OpenSetup));
        private void CancelCommandExecute()
        {
            OpenSetup = false;

            _eventAggregator.GetEvent<CancelFieldsEvent>().Publish(InstanceID);
        }

        private DelegateCommand _OnCheckCommand;
        public DelegateCommand OnCheckCommand =>
            _OnCheckCommand ?? (_OnCheckCommand =
            new DelegateCommand(OnCheckCommandExecute).ObservesCanExecute(() => OpenSetup));
        private void OnCheckCommandExecute()
        {
            ObservableCollection<string> NewList = new ObservableCollection<string>();
            ObservableCollection<string> orgList = _selectHdrItems;

            for (int i = 0; i < AvailableHdrList.Count; i++)
            {
                if (AvailableHdrList[i].IsChecked == true) NewList.Add(AvailableHdrList[i].Name);
            }

            if (orgList.Count > NewList.Count) //Remove item
            {
                IEnumerable<string> ItemRemove = orgList.Except(NewList);
                SelectHdrItems = FieldsModel.RemoveHdrItem(orgList, ItemRemove.ElementAt(0).ToString());
            }
            else //add item
            {
                IEnumerable<string> ItemAdd = NewList.Except(orgList);
                SelectHdrItems = FieldsModel.AddHdrItem(orgList, ItemAdd.ElementAt(0).ToString());
            }
        }
        

        public FieldSelectViewModel(IEventAggregator eventAggregator, int Instance)
        {
            _eventAggregator = eventAggregator;
           
            ClosedPageICommand = new DelegateCommand(ClosedPageExecute);

            InstanceID = Instance;
            FieldsModel = new FieldSelectModel(_eventAggregator, Instance);
            LoadPageExecute();
        }

        private void ClosedPageExecute()
        {
            OpenSetup = false;
        }

        private void LoadPageExecute()
        {
            SelectHdrItems = new ObservableCollection<string>();

            SelectHdrItems = FieldsModel.GetSelectHrdCheckList(InstanceID);
            AvailableHdrList = FieldsModel.AvailableItemList;
        }

       

        //SelectedAsciiIndex
        /// <summary>
        /// Look up for data type? SqlFleldsList
        /// </summary>
        private void InsertItemCommandExecute()
        {
            switch (SelectedS1TabIndex)
            {
                case 0:
                    for (int i = 0; i < SqlFleldsList.Count; i++)
                    {
                        if (SqlFleldsList[i].Item1 == SelectedS1Name)
                            SerialOutOne.Add(new DataOutput(FieldSelectedIndex, SelectedS1Name, SqlFleldsList[i].Item2, "00.00"));
                    }
                    SerialOneOutString += SelectedS1Name; // + ",";
                    break;

                case 1:
                    SerialOutOne.Add(new DataOutput(SelectedAsciiIndex, SelectedAscii, "Ascii", "00.00"));
                    SerialOneOutString += SelectedAscii; // + ",";
                    break;

                case 2:
                    SerialOutOne.Add(new DataOutput(SelectedAsciiIndex, ASCii, "Text", "00.00"));
                    SerialOneOutString += SelectedAscii;
                    break;

                default:

                    break;
            }

        }

        private int _SelectedS1TabIndex;
        public int SelectedS1TabIndex
        {
            get { return _SelectedS1TabIndex; }
            set { SetProperty(ref _SelectedS1TabIndex, value); }
        }

        public Action Close { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private void UpdateSerialOneOut()
        {
            SerialOneOutString = string.Empty;
            SerialOutOne = FieldsModel.ReadXmlSerialOneOut(InstanceID);

            for (int i = 0; i < SerialOutOne.Count; i++)
            {
                SerialOneOutString += SerialOutOne[i].Name;
            }

         //   _eventAggregator.GetEvent<UpdateStringOutput>().Publish(InstanceID);
        }
    }

    interface ICloseWindows
    {
        Action Close { get; set; }
    }
}
