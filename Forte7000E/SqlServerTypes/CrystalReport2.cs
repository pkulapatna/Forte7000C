using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Forte7000C.Reports
{
    public partial class CrystalReport2 : Form
    {
        public DataTable RepTable { get; set; }
     
        public CrystalReport2()
        {
            InitializeComponent();
        }

        internal void UpdateRepTable(DataTable RepDatatable)
        {
            RepTable = RepDatatable;
        }

        internal void SetReportTitle(string strTitle)
        {
            CrtDayReport21.SetDataSource(RepTable);
            CrtDayReport21.SummaryInfo.ReportTitle = strTitle;
            CrtDayReport21.Refresh();
        }

        internal void PrintReport()
        {
            CrtDayReport21.SetDataSource(RepTable);
           // CrtDayReport21.PrintToPrinter(1, false, 0, 0);
        }


    }
}
