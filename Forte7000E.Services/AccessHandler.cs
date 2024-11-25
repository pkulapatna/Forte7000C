using ClsErrorLog;
using System;
using System.Data;
using System.Data.OleDb;
using System.Windows;

namespace Forte7000E.Services
{
    public class AccessHandler
    {
        private readonly string dbProvider = "PROVIDER=Microsoft.Jet.OLEDB.4.0;";
        private readonly string DB_4760 = "Data Source=c:\\Forte4760\\4760.mdb;Persist Security Info=True";
        //private readonly string DB_SUPJ4 = @"Data Source=C:\\ForteSystem\Reports\Rep_SupJ4.mdb;Persist Security Info=True;";
        private readonly string DB_LVFORMAT = @"Data Source=C:\\ForteSystem\Reports\LVFormats.mdb;Persist Security Info=True;";

        private readonly string DB_GRADE = @"Data Source=C:\\ForteSystem\Grades\PulpGrade.mdb;Persist Security Info=True;";
        //private string DB_GRADESUB = @"Data Source=C:\\ForteSystem\Grades\Grade_Sup.mdb;Persist Security Info=True;";
        //private string DB_GRADRES = @"Data Source=C:\\ForteSystem\Grades\CalGradeRes.mdb;Persist Security Info=True;";

        private readonly string DB_CALTABLE = @"Data Source=C:\\ForteSystem\Calibrations\Calibrate.mdb;Persist Security Info=True;";

        public DataTable ConfigTable { get; set; }
        public DataTable WetMTable { get; set; }
        public DataTable IdentTable { get; set; }
        public DataTable DtTmTable { get; set; }
        public DataTable ModBusConfigTable { get; set; }
        public DataTable LVHdrFmtBale { get; set; }

        public const string TB_GRADE = "GradeTable";
        public const string TB_STOCK = "StockTable";

        public string strRootPath = "C:\\ForteSystem";
        public string strRealTimePath = "..\\RealTime";
        public string strGradePath = "..\\Grades";
        public string strCalPath = "..\\calibrations";

      
        public AccessHandler()
        {
      
        }

        public DataTable GetAccessStockTable()
        {
            DataTable MyDatTable = new DataTable();
            string connectionString = dbProvider + DB_GRADE;
            string strQuery = "SELECT* FROM StockTable ORDER BY Name ASC";

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(strQuery, connection))
                    {
                        adapter.Fill(MyDatTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in GetAccessStockTable " + ex);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBACCESS, ex.Message);
            }
            return MyDatTable;
        }


        public string GetCalDatById(string CalId)
        {
            string calName = string.Empty;
            string connectionString = dbProvider + DB_CALTABLE;
            string strQuery = "SELECT * FROM CalTable WHERE Id = " + CalId;

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    if (connection != null) connection.Open();
                    using (var command = new OleDbCommand(strQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                                while (reader.Read())
                                {
                                    if (reader != null)
                                        calName = reader[0].ToString();
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in GetCalDatById " + ex);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBACCESS, ex.Message);
            }
            return calName;
        }


        public DataTable GetAccessCalParamsByProdIdTable(string CalId)
        {
            DataTable TempTable = new DataTable();
            string connectionString = dbProvider + DB_CALTABLE;
            string strQuery = "SELECT Name,A,B,C FROM CalTable WHERE ID = " + CalId;

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(strQuery, connection))
                    {
                        adapter.Fill(TempTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("EROR in GetAccessCalParamsByProdIdTable " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBACCESS, ex.Message);
            }
            return TempTable;
        }

        public DataTable GetAccDataTable()
        {
            DataTable MyTable = new DataTable();
            string connectionString = dbProvider + DB_4760;
            string strQuery = "SELECT * FROM ProductData ORDER BY TimeStamp DESC;";
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(strQuery, connection))
                    {
                        adapter.Fill(MyTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("EROR in GetAccDataTable " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBACCESS, ex.Message);
            }
            return MyTable;
        }

        public DataTable GetLVHdrFmtBaleTable()
        {
            DataTable LVHdrFmtBaleTable = new DataTable();

            string strQuery = "SELECT FieldExpr,Text,Format FROM [LVHdrFmtBale]";
            string connectionString = dbProvider + DB_LVFORMAT;

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(strQuery, connection))
                    {
                        adapter.Fill(LVHdrFmtBaleTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("EROR in GetLVHdrFmtBaleTable " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBACCESS, ex.Message);
            }
            return LVHdrFmtBaleTable;
        }

        public DataTable GetProductList()
        {
            DataTable MyDatTable = new DataTable();
            string connectionString = dbProvider + DB_4760;
            string strQuery = "SELECT * FROM Types";

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(strQuery, connection))
                    {
                        adapter.Fill(MyDatTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("EROR in GetProductList " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBACCESS, ex.Message);
            }
            return MyDatTable;
        }

        public DataTable GetAccessCalTable()
        {
            DataTable MyDataTable = new DataTable();
            string connectionString = dbProvider + DB_CALTABLE;
            // string strQuery = "SELECT ID,Name,A,B,C FROM CalTable ORDER by ID ASC;";
            string strQuery = "SELECT * FROM CalTable ORDER by ID ASC;";

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(strQuery, connection))
                    {
                        adapter.Fill(MyDataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR IN GetAccessCalTable " + ex);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBACCESS, ex.Message);
            }
            return MyDataTable;
        }



    }
}
