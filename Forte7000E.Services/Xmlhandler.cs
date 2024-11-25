using ClsErrorLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Forte7000E.Services
{
    /// <summary>
    /// using Singleton Pattern for 1 instance
    /// </summary>
    public class Xmlhandler 
    {
        public string GetSettingsDirectory()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }

        public string SettingsGdvFile
        {
            get { return Path.Combine(GetSettingsDirectory(), "GridviewItems.xml"); }
        }

        public string XMLOutSerialOne
        {
            get { return Path.Combine(GetSettingsDirectory(), "OutSerialOne.xml"); }
        }

        public string XMLSharedListFile
        {
            get { return Path.Combine(GetSettingsDirectory(), "OutSharedFile.xml"); }
        }

        public string XMLPrintBaleFile
        {
            get { return Path.Combine(GetSettingsDirectory(), "PrintBaleFile.xml"); }
        }

        public string XMLScaleRequestString
        {
            get { return Path.Combine(GetSettingsDirectory(), "ScaleRequestString.xml"); }
        }

       
        public string XMLoutputfile { get; set; }

        public string XMLGdvFilePath
        {
            get { return Path.Combine(GetSettingsDirectory(), "GridviewItems.xml"); }
        }

        private static Xmlhandler instance = null;
        private static readonly object padlock = new object();
        public static Xmlhandler Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Xmlhandler();
                    }
                    return instance;
                }
            }
        }

        public Xmlhandler()
        {

        }


        public ObservableCollection<DataOutput> ReadXmlStringOut(int InstanceID)
        {
            ObservableCollection<DataOutput> SerialOneOutList = new ObservableCollection<DataOutput>();
            SerialOneOutList.Clear();
            XmlDocument xmldoc = new XmlDocument();
            int i = 0;

            XMLoutputfile = GetXmlFile(InstanceID);

            try
            {
                if (File.Exists(XMLoutputfile))
                {
                    xmldoc.Load(XMLoutputfile);
                    XmlNodeList xmlnode;

                    using (FileStream fsx = new FileStream(XMLoutputfile, FileMode.Open, FileAccess.Read))
                    {
                        xmldoc.Load(fsx);
                        xmlnode = xmldoc.SelectNodes("SerialOneOutGridView/Field");
                        for (i = 0; i <= xmlnode.Count - 1; i++)
                        {
                            SerialOneOutList.Add(new DataOutput(Convert.ToInt32(xmlnode[i].ChildNodes.Item(0).InnerText.Trim()),
                                xmlnode[i].ChildNodes.Item(1).InnerText.Trim(),
                                xmlnode[i].ChildNodes.Item(2).InnerText.Trim(),
                                xmlnode[i].ChildNodes.Item(3).InnerText.Trim()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in ReadSerilaOneList - " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBERROR, ex.Message);
            }
            return SerialOneOutList;
        }

        private string GetXmlFile(int instanceID)
        {
            string strXml = string.Empty;

            switch (instanceID)
            {
                case 0:
                    strXml = XMLOutSerialOne;
                    break;

                case 1:
                    strXml = XMLSharedListFile;
                    break;

                case 2:
                    strXml = XMLPrintBaleFile;
                    break;

                case 3:
                    strXml = XMLScaleRequestString;
                    break;
            }
            return strXml;
        }

        public List<string> ReadXmlGridView(string FileLocation)
        {
            List<string> XmlGridView = new List<string>();
            XmlGridView.Clear();
            XmlDocument doc = new XmlDocument();

            try
            {
                if (File.Exists(FileLocation))
                {
                    doc.Load(FileLocation);
                    XmlNodeList xnl = doc.SelectNodes("CustomGridView/Field/Name");

                    if ((xnl != null) && (xnl.Count > 0))
                    {
                        foreach (XmlNode xn in xnl)
                        {
                            if (File.Exists(FileLocation))
                                XmlGridView.Add(xn.InnerText);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in ReadXmlGridView - " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBERROR, ex.Message);
            }
            return XmlGridView;
        }

        public void UpdateXMlcolumnList(ObservableCollection<string> selectedHdrList, string settingsGdvFile)
        {

            try
            {
                if (File.Exists(settingsGdvFile)) File.Delete(settingsGdvFile);

                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true
                };
                using (XmlWriter writer = XmlWriter.Create(settingsGdvFile, settings))
                {
                    //Begin write
                    writer.WriteStartDocument();
                    //Node
                    writer.WriteStartElement("CustomGridView");

                    foreach (var item in selectedHdrList)
                    {
                        writer.WriteStartElement("Field");
                        writer.WriteElementString("Name", item);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Close();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error in Update XMlcolumnList " + ex);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBERROR, ex.Message);
            }
        }

        public void UpdateSerialOutOneList(ObservableCollection<DataOutput> serialOutOne, int targetId)
        {
            XMLoutputfile = GetXmlFile(targetId);

            try
            {
                if ((serialOutOne.Count == 0) & (File.Exists(XMLoutputfile)))
                {
                    File.Delete(XMLoutputfile);
                    ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.XMLFILE, "Deleted XMLoutputfile @ " + DateTime.Now);
                }
                else if (serialOutOne.Count > 0)
                {
                    if (File.Exists(XMLoutputfile)) File.Delete(XMLoutputfile);

                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = true
                    };
                    using (XmlWriter writer = XmlWriter.Create(XMLoutputfile, settings))
                    {
                        //Begin write
                        writer.WriteStartDocument();
                        //Node
                        writer.WriteStartElement("SerialOneOutGridView");

                        foreach (var item in serialOutOne)
                        {
                            writer.WriteStartElement("Field");
                            writer.WriteElementString("Id", item.Id.ToString());
                            writer.WriteElementString("Name", item.Name);
                            writer.WriteElementString("FieldType", item.FieldType);
                            writer.WriteElementString("FieldFormat", item.FieldFormat);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                        writer.Close();
                    }
                    ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.XMLFILE, "UpdateSerialOutOneList @ " + DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in UpdateSerialOutOneList " + ex);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBERROR, ex.Message);
            }
        }

        public List<int> ReadXmlHdrList(string FileLocation)
        {
            List<int> ihdrlist = new List<int>();
            ihdrlist.Clear();
            XmlDocument doc = new XmlDocument();

            try
            {
                if (File.Exists(FileLocation))
                {
                    doc.Load(FileLocation);
                    XmlNodeList xnl = doc.SelectNodes("CustomHdr/Field/Value");
                    if ((xnl != null) && (xnl.Count > 0))
                    {
                        foreach (XmlNode xn in xnl)
                        {
                            ihdrlist.Add(Int32.Parse(xn.InnerText));
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error in ReadXmlHdrList - " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBERROR, ex.Message);
            }
            return ihdrlist;
        }

        public void WriteXmlGridView(List<CheckedListItem> StringsListBox, string FileLocation)
        {
            try
            {
                if (System.IO.File.Exists(FileLocation))
                    System.IO.File.Delete(FileLocation);

                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true
                };

                using (XmlWriter writer = XmlWriter.Create(FileLocation, settings))
                {
                    //Begin write
                    writer.WriteStartDocument();
                    //Node
                    writer.WriteStartElement("CustomGridView");

                    foreach (var item in StringsListBox)
                    {
                        writer.WriteStartElement("Field");
                        writer.WriteElementString("Id", item.Id.ToString());
                        writer.WriteElementString("Name", item.Name);
                        writer.WriteElementString("FieldType", item.FieldType);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndDocument();
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in WriteXmlGridView - " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBERROR, ex.Message);
            }
        }


    }
}
