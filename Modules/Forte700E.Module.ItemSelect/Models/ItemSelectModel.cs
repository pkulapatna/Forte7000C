using Forte7000E.Services;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forte700E.Module.ItemSelect.Models
{
    public class ItemSelectModel
    {
        protected readonly IEventAggregator _eventAggregator;

        public Sqlhandler SqlHandler { get; set; }
        public Xmlhandler Xmlhandler { get; set; }

        public List<string> XmlColumnList = new List<string>();
        public int SetInstanceId { get; set; }

        public ObservableCollection<CheckedListItem> AvailableItemList { get; set; }

        public string XMLRealTimeGdvFile
        {
            get { return Path.Combine(Xmlhandler.SettingsGdvFile); }
        }

        public ItemSelectModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
            Xmlhandler = Xmlhandler.Instance;
            SqlHandler = Sqlhandler.Instance;
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

        internal List<Tuple<string, string>> GetSqlFieldsList()
        {
            return SqlHandler.GetTableSchema();
        }
        private List<string> GetXmlcolumnList(string xMLDropsGdvFile)
        {
            return Xmlhandler.ReadXmlGridView(xMLDropsGdvFile);
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
            return ClassCommon.Asciilist;
        }


    }
}
