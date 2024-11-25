using CrystalDecisions.Shared;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Forte7000C.Reports
{
    public partial class CrystalReport : Form
    {
        public DataTable RepTable { get; set; }
        public string StrRepQuery { get; set; }
        private string ReportTitle { get; set; }
        public string ArchTable { get; set; }
        public CrystalReport()
        {
            InitializeComponent();
        }

        private void CrystalReport_Load(object sender, EventArgs e)
        {
            CrtDayReport1.SetDataSource(RepTable);
            CrtDayReport1.SummaryInfo.ReportTitle = ReportTitle;
            CrtDayReport1.Refresh();
        }
       
        internal void UpdateRepTable(DataTable RepDatatable)
        {
            RepTable = RepDatatable;
            CrtDayReport1.SetDataSource(RepDatatable);
            CrtDayReport1.Refresh();
        }

        internal void PrintReport()
        {
            Thread printThread = new Thread(() =>
            {
                CrtDayReport1.SetDataSource(RepTable);
                //CrtDayReport1.PrintToPrinter(1, false, 0, 0);
            });
            printThread.SetApartmentState(ApartmentState.STA);
            printThread.Start();
        }

        /// <summary>
        /// Not to lockup UI while printing
        /// </summary>
        /// <returns></returns>
        internal async Task PrintReportAsync()
        {
            await Task.Run(() =>
            {
                CrtDayReport1.SetDataSource(RepTable);
                //CrtDayReport1.PrintToPrinter(1, false, 0, 0);
            });
        }

        private string CreateSelectQueryAndParameters()
        {
            ParameterFields paramFields;
            ParameterField paramField;
            ParameterDiscreteValue paramDiscreteValue;

            paramFields = new ParameterFields();

            string query = string.Empty;
            int columnNo = 0;

            if(CBSerialNumber.Checked)
            {
                columnNo++;
                query = query.Insert(query.Length, "SerialNumber as SerialNumber,");

                paramField = new ParameterField
                {
                    Name = "SerialNumber"
                };
                paramDiscreteValue = new ParameterDiscreteValue
                {
                    Value = "SerialNumber"
                };
                paramField.CurrentValues.Add(paramDiscreteValue);
                //Add the paramField to paramFields
                paramFields.Add(paramField);
            }

            if (CBTimeStart.Checked)
            {
                columnNo++;
                query = query.Insert(query.Length, "TimeStart as TimeStart,");

                paramField = new ParameterField
                {
                    Name = "TimeStart"
                };
                paramDiscreteValue = new ParameterDiscreteValue
                {
                    Value = "TimeStart"
                };
                paramField.CurrentValues.Add(paramDiscreteValue);
                //Add the paramField to paramFields
                paramFields.Add(paramField);
            }

            if (CBTimeComplete.Checked)
            {
                columnNo++;
                query = query.Insert(query.Length, "TimeComplete as TimeComplete,");

                paramField = new ParameterField
                {
                    Name = "TimeComplete"
                };
                paramDiscreteValue = new ParameterDiscreteValue
                {
                    Value = "TimeComplete"
                };
                paramField.CurrentValues.Add(paramDiscreteValue);
                //Add the paramField to paramFields
                paramFields.Add(paramField);
            }

            if (CBShiftName.Checked)
            {
                columnNo++;
                query = query.Insert(query.Length, "ShiftName as ShiftName,");

                paramField = new ParameterField
                {
                    Name = "ShiftName"
                };
                paramDiscreteValue = new ParameterDiscreteValue
                {
                    Value = "ShiftName"
                };
                paramField.CurrentValues.Add(paramDiscreteValue);
                //Add the paramField to paramFields
                paramFields.Add(paramField);
            }

            if (CBStockName.Checked)
            {
                columnNo++;
                query = query.Insert(query.Length, "StockName as StockName," );

                paramField = new ParameterField
                {
                    Name = "StockName"
                };
                paramDiscreteValue = new ParameterDiscreteValue
                {
                    Value = "StockName"
                };
                paramField.CurrentValues.Add(paramDiscreteValue);
                //Add the paramField to paramFields
                paramFields.Add(paramField);
            }

            if (CBCalibrationName.Checked)
            {
                columnNo++;
                query = query.Insert(query.Length, "CalibrationName as CalibrationName,");

                paramField = new ParameterField
                {
                    Name = "CalibrationName"
                };
                paramDiscreteValue = new ParameterDiscreteValue
                {
                    Value = "CalibrationName"
                };
                paramField.CurrentValues.Add(paramDiscreteValue);
                //Add the paramField to paramFields
                paramFields.Add(paramField);
            }

            if (CBNetWeight.Checked)
            {
                columnNo++;
                query = query.Insert(query.Length, "NetWeight as NetWeight,");

                paramField = new ParameterField
                {
                    Name = "NetWeight"
                };
                paramDiscreteValue = new ParameterDiscreteValue
                {
                    Value = "NetWeight"
                };
                paramField.CurrentValues.Add(paramDiscreteValue);
                //Add the paramField to paramFields
                paramFields.Add(paramField);
            }

            if (CBTareWeight.Checked)
            {
                columnNo++;
                query = query.Insert(query.Length, "TareWeight as TareWeight,");

                paramField = new ParameterField
                {
                    Name = "TareWeight"
                };
                paramDiscreteValue = new ParameterDiscreteValue
                {
                    Value = "TareWeight"
                };
                paramField.CurrentValues.Add(paramDiscreteValue);
                //Add the paramField to paramFields
                paramFields.Add(paramField);
            }

            if (CBWeight.Checked)
            {
                columnNo++;
                query = query.Insert(query.Length, "Weight as Weight,");

                paramField = new ParameterField
                {
                    Name = "Weight"
                };
                paramDiscreteValue = new ParameterDiscreteValue
                {
                    Value = "Weight"
                };
                paramField.CurrentValues.Add(paramDiscreteValue);
                //Add the paramField to paramFields
                paramFields.Add(paramField);
            }

            if (CBWtMes.Checked)
            {
                columnNo++;
                query = query.Insert(query.Length, "WtMes as WtMes,");

                paramField = new ParameterField
                {
                    Name = "WtMes"
                };
                paramDiscreteValue = new ParameterDiscreteValue
                {
                    Value = "WtMes"
                };
                paramField.CurrentValues.Add(paramDiscreteValue);
                //Add the paramField to paramFields
                paramFields.Add(paramField);
            }

            if (CBMoisture.Checked)
            {
                columnNo++;
                query = query.Insert(query.Length, "Moisture as Moisture,");

                paramField = new ParameterField
                {
                    Name = "Moisture"
                };
                paramDiscreteValue = new ParameterDiscreteValue
                {
                    Value = "Moisture"
                };
                paramField.CurrentValues.Add(paramDiscreteValue);
                //Add the paramField to paramFields
                paramFields.Add(paramField);
            }

            if (CBMoistMes.Checked)
            {
                columnNo++;
                query = query.Insert(query.Length, "MoistMes as MoistMes,");

                paramField = new ParameterField
                {
                    Name = "MoistMes"
                };
                paramDiscreteValue = new ParameterDiscreteValue
                {
                    Value = "MoistMes"
                };
                paramField.CurrentValues.Add(paramDiscreteValue);
                //Add the paramField to paramFields
                paramFields.Add(paramField);
            }

            //crystalReportViewer2.ParameterFieldInfo = paramFields;

            return query;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            CrtDayReport1 = new CrtDayReport();
            CrtDayReport1.SetDataSource(RepTable);
            CrtDayReport1.Refresh();


            
        }

        internal string GetItemsString()
        {
            CBSerialNumber.Checked = true;
            CBTimeComplete.Checked = true;
            CBShiftName.Checked = true;
            CBStockName.Checked = true;
            CBCalibrationName.Checked = true;
            CBWeight.Checked = true;
            CBMoisture.Checked = true;

            return CreateSelectQueryAndParameters();
        }

        internal void SetReportTitle(string strTitle)
        {
            ReportTitle = strTitle;
            CrtDayReport1.SetDataSource(RepTable);
            CrtDayReport1.SummaryInfo.ReportTitle = strTitle;
            CrtDayReport1.Refresh();
        }

        internal void ShowRepWindow()
        {
            CrtDayReport1.SetDataSource(RepTable);
           
        }

        private void CrystalReportViewer2_Load(object sender, EventArgs e)
        {

        }

        private void SplitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void CrtDayReport1_InitReport(object sender, EventArgs e)
        {

        }
    }
}
