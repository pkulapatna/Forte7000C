using Forte7000E.Services;
using Forte700E.Module.ItemSelect.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forte700E.Module.ItemSelect.ViewModels
{
    public class ItemSelectViewModel : BindableBase
    {
        protected readonly Prism.Events.IEventAggregator _eventAggregator;
        private readonly ItemSelectModel _ItemSelectModel;
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

        public Action CloseAction { get; set; }

        public DelegateCommand InsertItemCommand { get; set; }
        public DelegateCommand RemoveitemCommand { get; set; }
        public DelegateCommand SaveItemListCommand { get; set; }

        public ItemSelectViewModel(int id, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _ItemSelectModel = new ItemSelectModel(_eventAggregator);

            InstanceID = id;
            _ItemSelectModel.SetInstanceId = InstanceID;

            ASCIItupleList = new List<Tuple<string, char>>();
            ASCIItupleList = _ItemSelectModel.GetASCIItupleList();

            SqlFleldsList = new List<Tuple<string, string>>();
            SqlFleldsList = _ItemSelectModel.GetSqlFieldsList();

            XmlItemsList = new ObservableCollection<string>();
            XmlItemsList = _ItemSelectModel.GetXmlItemsList();

            AsciiList = new List<string>();
            for (int i = 0; i < ASCIItupleList.Count; i++)
            {
                AsciiList.Add(ASCIItupleList[i].Item1);
            }

            switch (InstanceID)
            {
                case 0:
                    SelectedS1TabIndex = 0;
                    break;

                case 1:
                    SelectedS1TabIndex = 0;
                    break;

                case 2:
                    SelectedS1TabIndex = 0;
                    break;

                case 3: //ScaleReqString
                    SelectedS1TabIndex = 1;
                    break;

                default:
                    break;
            }
            UpdateSerialOneOut(InstanceID);


            InsertItemCommand = new DelegateCommand(InsertItemCommandExecute);
            SaveItemListCommand = new DelegateCommand(SaveItemListCommandExecute);
            RemoveitemCommand = new DelegateCommand(RemoveitemCommandExecute);
        }

        private void RemoveitemCommandExecute()
        {
            if (SelectDelIndex != -1)
                SerialOutOne.RemoveAt(SelectDelIndex);
        }

        private void SaveItemListCommandExecute()
        {
            _ItemSelectModel.SaveItemList(SerialOutOne, InstanceID);
            UpdateSerialOneOut(InstanceID);
            _eventAggregator.GetEvent<SaveItemSelectEvent>().Publish(InstanceID);
        }

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

        private void UpdateSerialOneOut(int instanceID)
        {
            SerialOneOutString = string.Empty;
            SerialOutOne = _ItemSelectModel.ReadXmlSerialOneOut(InstanceID);

            for (int i = 0; i < SerialOutOne.Count; i++)
            {
                SerialOneOutString += SerialOutOne[i].Name;
            }

            if (instanceID == 0)
                _eventAggregator.GetEvent<UpdateStringOutput>().Publish(instanceID);

            if (instanceID == 3)
                _eventAggregator.GetEvent<ScaleReqString>().Publish(SerialOneOutString);

            
        }
    }
}
