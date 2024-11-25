using ClsErrorLog;
using Forte7000E.Services;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forte7000E.Module.FieldSelect.Models
{
    public class FieldSelectModel
    {
        protected readonly IEventAggregator _eventAggregator;

        public List<string> XmlColumnList = new List<string>();
        private readonly List<string> XmlLotHeaderList;
        public int InstanceID;

        public ObservableCollection<CheckedListItem> AvailableItemList { get; set; }
        public ObservableCollection<CheckedListItem> AvailableSummaryItemList { get; internal set; }

        public DataTable HdrTable;
      
        public string XMLRealTimeGdvFile
        {
            get { return Path.Combine(Xmlhandler.SettingsGdvFile); }
        }

        public Sqlhandler SqlHandler { get; set; }
        public Xmlhandler Xmlhandler { get; set; }
       

        public FieldSelectModel(IEventAggregator EventAggregator, int idtype)
        {
            _eventAggregator = EventAggregator;
            InstanceID = idtype;

            Xmlhandler = Xmlhandler.Instance; // new Xmlhandler();
            SqlHandler = Sqlhandler.Instance; // new Sqlhandler();

            XmlLotHeaderList = new List<string>();
            XmlLotHeaderList = ClassCommon._lotHdrXmlLoclst;
            AvailableItemList = new ObservableCollection<CheckedListItem>();
            AvailableSummaryItemList = new ObservableCollection<CheckedListItem>();

        }
        internal void SaveItemList(ObservableCollection<DataOutput> serialDataOut, int TargetId)
        {
            Xmlhandler.UpdateSerialOutOneList(serialDataOut, TargetId);
        }

        internal ObservableCollection<DataOutput> ReadXmlSerialOneOut(int InstanceID)
        {
            return Xmlhandler.ReadXmlStringOut(InstanceID);
        }

        internal List<Tuple<string, char>> GetASCIItupleList()
        {
            return null;// _serialDevices.GetASCIItupList();
        }

        internal ObservableCollection<string> GetXmlItemsList()
        {
            ObservableCollection<string> XmlCheckedList = new ObservableCollection<string>();
            XmlColumnList.Clear();
            XmlColumnList = GetXmlcolumnList(XMLRealTimeGdvFile);

            foreach (var item in XmlColumnList)
            {
                XmlCheckedList.Add(item);
            }
            return XmlCheckedList;
        }
        private List<string> GetXmlcolumnList(string xMLDropsGdvFile)
        {
            return Xmlhandler.ReadXmlGridView(xMLDropsGdvFile);
        }

        public ObservableCollection<string> GetSelectHrdCheckList(int InstanceID)
        {
            ObservableCollection<string> XmlCheckedList = new ObservableCollection<string>();
            AvailableItemList.Clear();

            HdrTable = new DataTable();


            XmlColumnList.Clear();

            if (InstanceID == (int)ClassCommon.InstanceType.Summary)
            {
                HdrTable = SqlHandler.GetSqlTableHdr();
                XmlColumnList = GetXmlcolumnList(XMLRealTimeGdvFile);
            }
            else
            {
                HdrTable = SqlHandler.GetSqlLotScema();
                XmlColumnList = GetXmlcolumnList(XmlLotHeaderList[InstanceID]);
            }
                

            foreach (DataRow item in HdrTable.Rows)
            {
                if (AllowField(item[1].ToString()))
                {
                    if (XmlColumnList.Contains(item[1].ToString()))
                        AvailableItemList.Add(new CheckedListItem(Convert.ToInt32(item[0]), item[1].ToString(), true, item[2].ToString()));
                    else
                        AvailableItemList.Add(new CheckedListItem(Convert.ToInt32(item[0]), item[1].ToString(), false, item[2].ToString()));
                }
            }
            foreach (var item in XmlColumnList)
            {
                XmlCheckedList.Add(item);
            }
            return XmlCheckedList;
        }

        public ObservableCollection<string> GetXmlSelectedHdrCheckedList()
        {
            ObservableCollection<string> XmlCheckedList = new ObservableCollection<string>();
            HdrTable = new DataTable();

            try
            {
                HdrTable = SqlHandler.GetSqlTableHdr();
                XmlColumnList.Clear();

                XmlColumnList = GetXmlcolumnList(XMLRealTimeGdvFile);
                AvailableItemList.Clear();

                if ((HdrTable.Rows.Count > 0) && (XmlColumnList.Count > 0))
                {
                    foreach (DataRow item in HdrTable.Rows)
                    {
                        // Console.WriteLine("  ------   " + item[1].ToString());
                        if (AllowField(item[1].ToString()))
                        {
                            if (XmlColumnList.Contains(item[1].ToString()))
                                AvailableItemList.Add(new CheckedListItem(Convert.ToInt32(item[0]), item[1].ToString(), true, item[2].ToString()));
                            else
                                AvailableItemList.Add(new CheckedListItem(Convert.ToInt32(item[0]), item[1].ToString(), false, item[2].ToString()));
                        }
                    }
                    foreach (var item in XmlColumnList)
                    {
                        XmlCheckedList.Add(item);
                    }
                }
                return XmlCheckedList;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("ERROR in ClsBaleRealTimeModel GetXmlItemsList" + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.APPBALEREALTIME, ex.Message);
            }
            return XmlCheckedList;
        }



        public ObservableCollection<string> AddHdrItem(ObservableCollection<string> orgList, string NewItem)
        {
            ObservableCollection<string> tempList = orgList;
            tempList.Add(NewItem);
            return tempList;
        }

        public ObservableCollection<string> RemoveHdrItem(ObservableCollection<string> orgList, string Removeitem)
        {
            ObservableCollection<string>  tempList = orgList;
            tempList.Remove(Removeitem);
            return tempList;
        }

        private bool AllowField(string strItem)
        {
            foreach (var item in SqlHandler.RemoveFieldsList)
            {
                if (item == strItem) return false;
            }
            return true;
        }

        public void SaveXmlcolumnList(ObservableCollection<string> selectedHdrList)
        {
            Xmlhandler.UpdateXMlcolumnList(selectedHdrList, XmlLotHeaderList[InstanceID]);
        }

        internal void SaveModified_setting()
        {
            List<CheckedListItem> CustomHdrList = new List<CheckedListItem>();

            foreach (var item in AvailableItemList)
            {
                if (item.IsChecked)
                    CustomHdrList.Add(new CheckedListItem(item.Id, item.Name, item.IsChecked, item.FieldType));
            }
            //   _xmlhandler.WriteXmlGridView(CustomHdrList, _xmlhandler.XMLGdvFilePath);

            CustomHdrList.Clear();
        }

    }
}
