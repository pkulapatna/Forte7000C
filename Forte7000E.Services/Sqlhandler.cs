using ClsErrorLog;
using Forte7000E.Services.Properties;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Forte7000E.Services;

namespace Forte7000E.Services
{
    /// <summary>
    /// using Singleton Pattern for 1 instance
    /// </summary>
    public sealed class Sqlhandler 
    {
        private const string DB_GRADETABLE = @"Data Source=C:\\ForteSystem\Grades\PulpGrade.mdb;Persist Security Info=True;";
        private const string DB_CALTABLE = @"Data Source=C:\\ForteSystem\Calibrations\Calibrate.mdb;Persist Security Info=True;";
        private const string dbProvider = "PROVIDER=Microsoft.Jet.OLEDB.4.0;";

        public Xmlhandler Xmlhandler { get; set; }

        public string SqlAuConString { get; set; }
        public string SqlAuProdConStr { get; set; }
        public string StrUserName { get; set; }
        public string StrPassWrd { get; set; }
        public string StrHostID { get; set; }
        public string StrInstance { get; set; }
        public string WorkStationID { get; set; }
        public string TargetWorkStationID { get; set; }

        public string m_CurrentAchivesTable = "BaleArchive" + DateTime.Now.ToString("MMMyy");
        public string m_PreviousArchivesTable = "BaleArchive" + DateTime.Now.AddMonths(-1).ToString("MMMyy");

        public string m_CurrentLotTable = "LotArchive" + DateTime.Now.ToString("MMMyy");
        public string m_PreviousLotTable = "LotArchive" + DateTime.Now.AddMonths(-1).ToString("MMMyy");

        private string m_CreateTableQuery = string.Empty;
      //  private readonly string m_InsertTableQuery = string.Empty;

        private string m_CreateLotTableQuery = string.Empty;
        private string MasterConStr { get; set; }
        
        private string WinAuProdConStr { get; set; }
        private string WinAuRealtimeConStr { get; set; }
        private string WinAuTransConStr { get; set; }

    //    private string m_TableScript { get; set; }

        public int iNewIndexNumber = 0;
        public List<string> RemoveFieldsList = null;

        public List<ArchiveData> m_ArchiveFieldsList;
        public List<ArchiveData> m_LotArchiveFieldsList;

        public List<ArchiveData> m_LotTransitFieldsList;
        public List<ArchiveData> m_LotOpenFieldsList;
        public List<ArchiveData> m_LotClosedFieldsList;

        //Sql Database
        public const string MASTER_DB = "Master";
        public const string REALTIME_DB = "Fortedata";
        public const string PULPGRADE_DB = "PulpGrade";
        public const string PROCESSTRAN_DB = "ForteTrans";

        //SQL script to create table
        public const string SQLGRADETABLE = "GradeTable";
        public const string SQLSTOCKTABLE = "stockTable";
        public const string SQLCALTABLE = "CalTable";

        public const string SQLLOTTRANSTABLE = "LotTransaction";
        public const string SQLLOTARCHSTABLE = "LotArchive";
        public const string SQLOPENLOTSTABLE = "LotOpen";
        public const string SQLCLOSELOTSTABLE = "LotClose";

        
        public string ScriptsFilePath
        {
            get { return Path.Combine(GetSettingsDirectory(), "CreateArchiveTable.sql"); }
        }

        public string GetSettingsDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ForteData");
        }

        private static Sqlhandler instance = null;        
        private static readonly object padlock = new object();
        public static Sqlhandler Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Sqlhandler();
                    }
                    return instance;
                }
            }
        }

        public Sqlhandler()
        {
            InitialSetupSqlDataBase();
            SetRemoveFields();
            Xmlhandler = Xmlhandler.Instance;
        }

        public void InitialSetupSqlDataBase()
        {
            SetUpSqlParams();
            SetConnectionString();
            CheckCreatePulpGradeDbTab();
         
            //Bale Archives
            m_CreateTableQuery = ArchiveTablescript(m_CurrentAchivesTable);
            CheckCreateSqlTable(m_CurrentAchivesTable, m_CreateTableQuery);
            m_ArchiveFieldsList = SetArchiveDataStructure(m_CreateTableQuery, m_CurrentAchivesTable);

            //Lot Archives
            m_CreateLotTableQuery = LotTableScript(m_CurrentLotTable);
            CheckCreateSqlTable(m_CurrentLotTable, m_CreateLotTableQuery);
            m_LotArchiveFieldsList = SetArchiveDataStructure(m_CreateLotTableQuery, m_CurrentLotTable);

            
            if (CheckSqlDatabase(PROCESSTRAN_DB)) //for lot processing
            {
                //Lot Transition Table
                string m_CreateLotTransitTableQuery = LotTableScript(SQLLOTTRANSTABLE);
                CheckCreateLotSqlTable(SQLLOTTRANSTABLE, m_CreateLotTransitTableQuery);
                m_LotTransitFieldsList = SetArchiveDataStructure(m_CreateLotTransitTableQuery, SQLLOTTRANSTABLE);

                //Open Lot table - only one datafield
                string m_CreateOpenLotTableQuery = LotTableScript(SQLOPENLOTSTABLE);
                CheckCreateLotSqlTable(SQLOPENLOTSTABLE, m_CreateOpenLotTableQuery);
                m_LotOpenFieldsList = SetArchiveDataStructure(m_CreateOpenLotTableQuery, SQLOPENLOTSTABLE);

                //Closed lot table
                string m_CreateClosedLotTableQuery = LotTableScript(SQLCLOSELOTSTABLE);
                CheckCreateLotSqlTable(SQLCLOSELOTSTABLE, m_CreateClosedLotTableQuery);
                m_LotClosedFieldsList = SetArchiveDataStructure(m_CreateClosedLotTableQuery, SQLCLOSELOTSTABLE);
            }
            ClassCommon.MyInfoLog.LogMessage(MsgTypes.INFO, MsgSources.DBSQL, "InitialSetupSqlDataBase");
        }

       

        public List<string> GetLotTableList()
        {
            List<string> tablelist = new List<string>();
            string strquery = "SELECT [name],create_date FROM sys.tables WHERE [name] LIKE '%LotArchive%' ORDER BY create_date DESC";
            try
            {
                using (var sqlConnection = new SqlConnection(SqlAuConString))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    using (var command = new SqlCommand(strquery, sqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                                while (reader.Read())
                                {
                                    if (reader.GetString(0) != null)
                                        tablelist.Add(reader.GetString(0));
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetLotTableList " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBSQL, "GetUniqueStrList " + ex.Message);
            }
            return tablelist;
        }

        public List<string> GetUniqueStrItemlist(string strItem, string strTable)
        {
            string constr = SqlAuConString;
            List<string> itemList = new List<string>();
            string strQuery = "SELECT DISTINCT " + strItem + " From " + strTable + " ORDER BY " + strItem + ";";

           // if (strItem == "BalerID") constr = WLConStr;

            try
            {
                using (var sqlConnection = new SqlConnection(constr))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    using (var command = new SqlCommand(strQuery, sqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                                while (reader.Read())
                                {
                                    if (reader != null)
                                        itemList.Add(reader[0].ToString());
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetUniqueStrItemlist " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBSQL, "GetUniqueStrList " + ex.Message);
            }
            return itemList;
        }

        private void SetUpSqlParams()
        {
            WorkStationID = Environment.MachineName;
            TargetWorkStationID = Settings.Default.Host;
             StrInstance = "SQLEXPRESS";
             StrUserName = "forte";
             StrPassWrd = "etrof";   
        }

        private List<ArchiveData> SetArchiveDataStructure(string QueryStr, string SqlTableName)
        {
            List<ArchiveData> ArchData = new List<ArchiveData>();
            List<Tuple<int, string, string>> ArchiveItemList = new List<Tuple<int, string, string>>();

            string[] separatingStrings = { "NULL", "," };
            string[] words = QueryStr.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
            //System.Console.WriteLine($"{words.Length} substrings in text:");

            string[] separating = { "[", "]", " ", "CREATE", "TABLE", "(", ".", "dbo", SqlTableName };

            for (int i = 0; i < words.Length - 1; i++)
            {
                string[] line1 = words[i].Split(separating, System.StringSplitOptions.RemoveEmptyEntries);

                if (line1.Length > 1)
                {
                    ArchData.Add(new ArchiveData() { Index = i, Name = line1[0], Datatype = line1[1], Value = "0" });
                    ArchiveItemList.Add(new Tuple<int, string, string>(i, line1[0], line1[1]));
                    //Console.WriteLine(i + " m_Name " + line1[0] + " m_Datatype " + line1[1]);
                }
            }
            return ArchData;
        }

        private void SetConnectionString()
        {
            MasterConStr = SetWinAuConnString(MASTER_DB);
            WinAuProdConStr = SetWinAuConnString(PULPGRADE_DB);
            WinAuRealtimeConStr = SetWinAuConnString(REALTIME_DB);
            WinAuTransConStr = SetWinAuConnString(PROCESSTRAN_DB);

            SqlAuConString = SetSqlAuConnString(REALTIME_DB);
            SqlAuProdConStr = SetSqlAuConnString(PULPGRADE_DB);
        }

        private void CheckCreatePulpGradeDbTab()
        {
            bool Calibration;

            if (CheckSqlDatabase(PULPGRADE_DB))
            {
                CheckSqlTable(SQLGRADETABLE, WinAuProdConStr);
                DataTable SqlGradeTable = GetSqlDatatable(SQLGRADETABLE);
                if (SqlGradeTable.Rows.Count == 0)
                    CopyGradeTabletoSql(SqlGradeTable);

                CheckSqlTable(SQLSTOCKTABLE, WinAuProdConStr);
                DataTable SqlStockTable = GetSqlDatatable(SQLSTOCKTABLE);
                if (SqlStockTable.Rows.Count == 0)
                    CopyStockTabletoSql(SqlStockTable);

                Calibration = CheckSqlTable(SQLCALTABLE, WinAuProdConStr);

                if (Calibration)
                {
                    DataTable SqlCalibrationDataTable = GetSqlDatatable(SQLCALTABLE);
                    //For the first time -> copy from access for now
                    if (SqlCalibrationDataTable.Rows.Count == 0)
                        CopyAccessTabletoSql(SqlCalibrationDataTable);
                }
            }
        }


        private void CopyGradeTabletoSql(DataTable sqlGradeTable)
        {
            string strQuery = "SELECT * FROM GradeTable ORDER BY ID";
            string GadeConStr = dbProvider + DB_GRADETABLE;

            try
            {
                if (sqlGradeTable.Rows.Count < 1)
                {
                    DataTable dt = new DataTable();

                    using (var sourceConnection = new OleDbConnection(GadeConStr))
                    {
                        OleDbCommand commandSourceData = new OleDbCommand(strQuery, sourceConnection);
                        sourceConnection.Open();

                        OleDbDataAdapter da = new OleDbDataAdapter(commandSourceData);
                        da.Fill(dt);
                    }
                    using (var destinationConnection = new SqlConnection(WinAuProdConStr))
                    {
                        destinationConnection.Open();
                        using (var bulkCopy = new SqlBulkCopy(destinationConnection))
                        {
                            bulkCopy.DestinationTableName = "dbo.GradeTable";
                            //Type cast all fields then write to SQL
                            dt.Columns.Cast<DataColumn>().ToList().ForEach(x =>
                                bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(x.ColumnName, x.ColumnName)));
                            bulkCopy.WriteToServer(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in CopyGradeTabletoSql " + ex.Message);
            }
        }


        private void CopyStockTabletoSql(DataTable sqlStockTable)
        {
            string strQuery = "SELECT * FROM StockTable ORDER BY ID";
            string GadeConStr = dbProvider + DB_GRADETABLE;

            try
            {
                if (sqlStockTable.Rows.Count < 1)
                {
                    DataTable dt = new DataTable();

                    using (var sourceConnection = new OleDbConnection(GadeConStr))
                    {
                        OleDbCommand commandSourceData = new OleDbCommand(strQuery, sourceConnection);
                        sourceConnection.Open();

                        OleDbDataAdapter da = new OleDbDataAdapter(commandSourceData);
                        da.Fill(dt);
                    }
                    using (var destinationConnection = new SqlConnection(WinAuProdConStr))
                    {
                        destinationConnection.Open();
                        using (var bulkCopy = new SqlBulkCopy(destinationConnection))
                        {
                            bulkCopy.DestinationTableName = "dbo.stockTable";
                            //Type cast all fields then write to SQL
                            dt.Columns.Cast<DataColumn>().ToList().ForEach(x =>
                                bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(x.ColumnName, x.ColumnName)));
                            bulkCopy.WriteToServer(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in CopyGradeTabletoSql " + ex.Message);
            }
        }

        private void CopyAccessTabletoSql(DataTable DT)
        {
            if (DT.Rows.Count < 1)
            {
                DataTable dt = new DataTable();

                const string SourceConString = dbProvider + DB_CALTABLE;
                string strQuery = "SELECT * FROM CalTable ORDER BY ID";
                try
                {
                    using (var sourceConnection = new OleDbConnection(SourceConString))
                    {
                        OleDbCommand commandSourceData = new OleDbCommand(strQuery, sourceConnection);
                        sourceConnection.Open();

                        OleDbDataAdapter da = new OleDbDataAdapter(commandSourceData);
                        da.Fill(dt);
                    }
                    using (var destinationConnection = new SqlConnection(WinAuProdConStr))
                    {
                        destinationConnection.Open();
                        using (var bulkCopy = new SqlBulkCopy(destinationConnection))
                        {
                            bulkCopy.DestinationTableName = "dbo.CalTable";
                            //Type cast all fields then write to SQL
                            dt.Columns.Cast<DataColumn>().ToList().ForEach(x =>
                                bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(x.ColumnName, x.ColumnName)));
                            bulkCopy.WriteToServer(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROR in copyAccessTabletoSql " + ex.Message);
                }
            }
        }

       

        public bool CheckSqlTable(string StrTablename, string constr)
        {
            bool bfound = false;
            string xstrQuery = "SELECT [name],create_date FROM sys.tables WHERE [name] LIKE '%" + StrTablename + "%' ORDER BY create_date DESC";

            try
            {
                using (var sqlConnection = new SqlConnection(constr))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    using (var command = new SqlCommand(xstrQuery, sqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                                bfound = true;
                        }
                    }
                }

                if (!bfound)
                    bfound = ExecuteCommand(GetCreateScript(StrTablename), constr);

            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in CheckSqlTable" + ex);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBSQL, "CheckSqlTable " + ex.Message);
            }
            return bfound;
        }

        private bool ExecuteCommand(string strquery, string m_conn)
        {
            bool bDone = false;
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(m_conn))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    using (SqlCommand command = new SqlCommand(strquery, sqlConnection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                bDone = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SQL ERROR in executeCommand " + ex.Message);

                Console.WriteLine("---------------ERROR " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBSQL, "ExecuteCommand " + ex.Message);
            }
            return bDone;
        }

        public bool CheckSqlDatabase(string Strdb)
        {
            bool bFoundTable = false;
            string strQuery = "SELECT * FROM sys.databases d WHERE d.database_id>4";
            string strCreateDb = "CREATE DATABASE " + Strdb;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    else return false;
                    using (SqlCommand command = new SqlCommand(strQuery, sqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (reader != null)
                                    {
                                        // Console.WriteLine("reader = " + reader[0].ToString());
                                        if (reader[0].ToString() == Strdb)
                                            bFoundTable = true;
                                    }
                                }
                            }
                        }
                    }
                    if (!bFoundTable)
                        bFoundTable = ExecuteCommand(strCreateDb, WinAuRealtimeConStr);
                    sqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in SQL CheckSqlDatabase " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBSQL, "FindSqlTable " + ex.Message);
            }
            return bFoundTable;
        }

        public bool CheckCreateSqlTable(string TableName, string CreateQuery)
        {
            bool TableExcist = false;
            string xstrQuery = "SELECT [name],create_date FROM sys.tables WHERE [name] = '" + TableName + "' ORDER BY create_date DESC";

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    using (SqlCommand command = new SqlCommand(xstrQuery, sqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                                TableExcist = true;
                        }
                    }
                }

                if (!TableExcist)
                {
                    ExecuteCommand(CreateQuery, WinAuRealtimeConStr);
                    Console.WriteLine("SQL Table " + TableName + " was Created");
                }
                else
                {
                    Console.WriteLine("SQL Table " + TableName + " Table Already excisted");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error in CheckCreateSqlTable ", ex.Message);
                return false;
            }
            return true;
        }

        public bool CheckCreateLotSqlTable(string TableName, string CreateQuery)
        {
            bool TableExcist = false;
            string xstrQuery = "SELECT [name],create_date FROM sys.tables WHERE [name] = '" + TableName + "' ORDER BY create_date DESC";

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuTransConStr))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    using (SqlCommand command = new SqlCommand(xstrQuery, sqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                                TableExcist = true;
                        }
                    }
                }

                if (!TableExcist)
                {
                    ExecuteCommand(CreateQuery, WinAuTransConStr);
                    Console.WriteLine("SQL Table " + TableName + " was Created");
                }
                else
                {
                    Console.WriteLine("SQL Table " + TableName + " Table Already excisted");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error in CheckCreateSqlTable ", ex.Message);
                return false;
            }
            return true;
        }


        private string SetWinAuConnString(string SqlDatabase)
        {
            string strDataSource = WorkStationID + @"\" + StrInstance;
            return "workstation id=" + Environment.MachineName +
                    ";packet size=4096;integrated security=SSPI;data source='" + strDataSource +
                    "';persist security info=False;initial catalog= " + SqlDatabase;
        }

        private string SetSqlAuConnString(string SqlDatabase)
        {
            string strDataSource = WorkStationID + @"\" + StrInstance;
            return "Data Source ='" + strDataSource + "'; Database = "
                + SqlDatabase + "; User id= '" + StrUserName + "'; Password = '"
                + StrPassWrd + "'; connection timeout=30;Persist Security Info=True;";
        }

     

      

        public void DeleteLotTransRows()
        {
            string delstr = "DELETE FROM LotTransaction";
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuTransConStr))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    using (SqlCommand command = new SqlCommand(delstr, sqlConnection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in DeleteLotTransRows " + ex.Message);   
            }
        }

        public DataTable GetSqlDatatable(string strTable)
        {
            DataTable mytable = new DataTable();
            string strquery = "SELECT * FROM " + strTable + " ORDER BY ID ASC";

            try
            {
                using (var sqlConnection = new SqlConnection(WinAuProdConStr))
                {
                    using (var adapter = new SqlDataAdapter(strquery, sqlConnection))
                    {
                        adapter.Fill(mytable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in GetSqlDatatable " + ex.Message);
            }
            return mytable;
        }
        public DataTable GetSqlScema()
        {
            DataTable dx = new DataTable();
            string strQuery = "SELECT ORDINAL_POSITION,COLUMN_NAME,DATA_TYPE FROM ForteData.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + m_CurrentAchivesTable + "'";

            try
            {
                using (var sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    using (var adapter = new SqlDataAdapter(strQuery, sqlConnection))
                    {
                        adapter.Fill(dx);
                    }
                }

                foreach (var item in this.RemoveFieldsList)
                {
                    RemoveHrdItem(dx, item);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error in GetSqlScema -> " + ex.Message);
                //   MainWindow.AppWindows.LogObject.LogMessage(MsgTypes.WARNING, MsgSources.DBSQL, "GetSqlScema " + ex.Message);
            }
            return dx;
        }

        public DataTable GetLotTransTable()
        {
            DataTable Mytable = new DataTable();
            //m_WinAuTransConStr
            string strQuery = "SELECT * from LotTransaction ORDER BY UID DESC;";

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuTransConStr))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(strQuery, sqlConnection))
                    {
                        adapter.Fill(Mytable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in GetLotTransTable " + ex.Message);
            }
            return Mytable;
        }

        public DataTable GetOpenLotTable(string strquery)
        {
            DataTable mytable = new DataTable();
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuTransConStr))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(strquery, sqlConnection))
                    {
                        adapter.Fill(mytable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetOpenLotTable " + ex.Message);
            }

            return mytable;
        }

        public async Task<DataTable> GetForteDataTableAsync(string strQuery)
        {
            DataTable ArchiveTable = new DataTable(); 
            try
            {
                await Task.Run(() =>
                {
                    using (SqlConnection sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(strQuery, sqlConnection))
                        {
                           adapter.Fill(ArchiveTable);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in GetForteDataTableAsync " + ex.Message);
            }
            return ArchiveTable;
        }

        public DataTable GetForteDataTable(string strQuery)
        {
            DataTable mytable = new DataTable();
            
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(strQuery, sqlConnection))
                    {
                        adapter.Fill(mytable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetForteDataTable -> " + ex.Message);
            }
            return mytable;
        }

        public DataTable GetLotDataTable(int LotType)
        {
            DataTable Lottable = new DataTable();
            string fileLoc = ClassCommon._lotHdrXmlLoclst[LotType].ToString();
            
            List<string> lotitems = Xmlhandler.ReadXmlGridView(fileLoc);

            if (lotitems.Count > 0)
            {
                string strquery = GetLotQueryStrFromList(lotitems, GetCurrentLotArchivesTable());
                Lottable = GetForteDataTable(strquery);
            }
            return Lottable;
        }

        private string GetLotQueryStrFromList(List<string> lotitems, string LotTable)
        {
            string strItems = string.Empty;
            char charsToTrim = ',';

            foreach (var Item in lotitems)
            {
                strItems += Item + ",";
            }
            return "SELECT " + strItems.TrimEnd(charsToTrim) + " From " + LotTable + " WHERE  LotStatus = 'Closed' ORDER BY UID DESC;";
        }

        public DataTable GetForteDataTable2(string strQuery)
        {
            DataTable mytable = new DataTable();
            mytable.Clear();
            DataColumnCollection columns = mytable.Columns;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(strQuery, sqlConnection))
                    {
                        adapter.Fill(mytable);
                    }
                }

                if (mytable.Rows.Count > 0 & columns.Contains("index"))
                {
                    iNewIndexNumber = mytable.Rows[0].Field<int>("index");
                    object inewIndex = mytable.Rows[0]["index"];
                    //mytable.Columns.Remove("index");
                }
                DataRow[] rows = mytable.Select();

                // Console.WriteLine("Settings.Default.MoistureUnit = " + Settings.Default.MoistureUnit);

                for (int i = 0; i < rows.Length; i++)
                {
                    switch (ClassCommon.MoistureType)
                    {
                        case 0: // %MC == moisture from Sql database
                            if (columns.Contains("Moisture") & (rows[i]["Moisture"] != null))
                                rows[i]["Moisture"] = rows[i]["Moisture"];
                            break;

                        case 1: // %MR  = Moisture / ( 1- Moisture / 100)
                            if (columns.Contains("Moisture") & (rows[i]["Moisture"] != null))
                                rows[i]["Moisture"] = rows[i].Field<Single>("Moisture") / (1 - rows[i].Field<Single>("Moisture") / 100);
                            break;

                        case 2: // %AD = (100 - moisture) / 0.9
                            if (columns.Contains("Moisture") & (rows[i]["Moisture"] != null))
                                rows[i]["Moisture"] = (100 - rows[i].Field<Single>("Moisture")) / 0.9;
                            break;

                        case 3: // %BD  = 100 - moisture
                            if (columns.Contains("Moisture") & (rows[i]["Moisture"] != null))
                                rows[i]["Moisture"] = 100 - rows[i].Field<Single>("Moisture");
                            break;
                    }
                    if (ClassCommon.WeightType == 1) //English Unit lb
                    {
                        if ((columns.Contains("Weight")) & (rows[i]["Weight"] != null))
                            rows[i]["Weight"] = (rows[i].Field<Single>("Weight") * 2.20462); //Lb.

                        //  if ((columns.Contains("Weight")) & (rows[i]["Weight"] != null))
                        //      rows[i]["Weight"] = (Convert.ToDouble(rows[i]["Weight"]) * 2.20462).ToString(); //Lb.
                    }
                }
                SqlDataAdapter da = new SqlDataAdapter(strQuery, WinAuRealtimeConStr);
                DataSet ds = new DataSet();
                da.Fill(ds, GetCurrentArchivesTable());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetForteDataTable -> " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBSQL, "GetForteDatatable " + ex.Message);
            }
            return mytable;
        }

        public string GetCurrentArchivesTable()
        {
            GetTop2SqlArchivetable();
            return m_CurrentAchivesTable;
        }
        public string GetPreviousBaleTable()
        {
            GetTop2SqlArchivetable();
            return m_PreviousArchivesTable;
        }



        private void RemoveHrdItem(DataTable Ttable, string strItem)
        {
            foreach (DataRow item in Ttable.Rows)
            {
                if (item[1].ToString() == strItem)
                {
                    item.Delete();
                }
            }
            Ttable.AcceptChanges();
        }

        public DataTable GetSqlLotScema()
        {
            DataTable dx = new DataTable();
            string strQuery = "SELECT ORDINAL_POSITION,COLUMN_NAME,DATA_TYPE FROM ForteData.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + m_CurrentLotTable + "'";

            try
            {
                using (var sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    using (var adapter = new SqlDataAdapter(strQuery, sqlConnection))
                    {
                        adapter.Fill(dx);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetSqlLotScema -> " + ex.Message);
            }
            return dx;
        }
        private void GetTop2SqlArchivetable()
        {
            List<string> tablelist = new List<string>();
            string strquery = "SELECT top 2 [name],create_date FROM sys.tables WHERE [name] LIKE '%BaleArchive%' ORDER BY create_date DESC";

            try
            {
                
                using (SqlConnection sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    using (var command = new SqlCommand(strquery, sqlConnection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                                while (reader.Read())
                                {
                                    if (reader.GetString(0) != null)
                                        tablelist.Add(reader.GetString(0));
                                }
                        }
                    }
                    if (sqlConnection != null) sqlConnection.Close();
                }
                m_CurrentAchivesTable = tablelist[0].ToString();
                m_PreviousArchivesTable = tablelist.Count > 1 ? tablelist[1].ToString() : tablelist[0].ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in GetTop2SqlArchivetable " + ex.Message);
            }
        }

        public DataTable GetMaterialData(string material)
        {
            DataTable MyTable = new DataTable();  // = GetSqlDatatable(SQLSTOCKTABLE);
            string strQuery = "SELECT * from " + SQLSTOCKTABLE + " WHERE Name = '" + material + "'";

            try
            {
                using (var sqlConnection = new SqlConnection(WinAuProdConStr))
                {
                    using (var adapter = new SqlDataAdapter(strQuery, sqlConnection))
                    {
                        adapter.Fill(MyTable);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("ERROR GetMaterialData " + ex.Message);

            }
            return MyTable;
        }

        /// <summary>
        /// Asy write to Archive table
        /// </summary>
        /// <param name="_condition"></param>
        /// <returns></returns>
        public bool UpdateBaleArchiveTableAsy(bool _condition)
        {
            bool bdone = false;
            bdone = ThreadPool.QueueUserWorkItem(
              o =>
              {
                  if (_condition)
                  {
                      try
                      {
                          string strInsert = BuildStringInsertSql(m_ArchiveFieldsList);
                          ExecuteCommand(strInsert, WinAuRealtimeConStr);
                          //Thread.Sleep(500);
                      }
                      catch (Exception ex)
                      {
                          MessageBox.Show("Error in UpdateBaleArchiveTableAsy -> " + ex.Message);
                      }
                  }
              });
            return bdone;
        }
        
        public bool UpdateLotTableAsy(bool bcondition, string sqlTable)
        {
            bool bdone = false;
            string ConString = string.Empty;

            if (sqlTable == "LotTransaction") ConString = WinAuTransConStr;
            else ConString = WinAuRealtimeConStr;

            try
            {

                string strInsert = BuildStringInsertLotSql(m_LotOpenFieldsList, sqlTable);
                ExecuteCommand(strInsert, ConString);
                bdone = true;

                /*
             bdone = ThreadPool.QueueUserWorkItem(
             o =>
             {
                 if (bcondition)
                 {
                     try
                     {
                          string strInsert = BuildStringInsertLotSql(m_LotOpenFieldsList, sqlTable);
                          ExecuteCommand(strInsert, ConString);
                         // Thread.Sleep(500);
                     }
                     catch (Exception ex)
                     {
                         MessageBox.Show("Error in UpdateSqlArchiveTable -> " + ex.Message);
                     }
                 }
             });

                */
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in UpdateLotTableAsy -> " + ex.Message);
            }
            return bdone;
        }

        public bool UpdateLotTransitTableAsy(string strQuery, string sQLTable)
        {
            bool bdone = false;
            try
            {
                ExecuteCommand(strQuery, WinAuTransConStr);
                bdone = true;

                /*
                bdone = ThreadPool.QueueUserWorkItem(
            o =>
            {
                try
                {
                    ExecuteCommand(strQuery, WinAuTransConStr);
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in UpdateSqlArchiveTable -> " + ex.Message);
                }
            });

                */
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in UpdateLotTransitTableAsy -> " + ex.Message);
            }
            return bdone;
        }


        public string BuildStringInsertLotSql(List<ArchiveData> LotFieldsList, string SqlTable)
        {
            string strQuery = string.Empty;
            string strvalues = "(";
            char[] charsToTrim = { ',' };
            //string values = string.Empty;

            try
            {
                int i = 0;

                for (int x = 0; x < LotFieldsList.Count; x++)
                {
                    switch (LotFieldsList[x].Datatype)
                    {
                        case "int":

                            if (LotFieldsList[x].Name == "Index")
                                LotFieldsList[x].Value = ClassCommon.IIndex.ToString();
                            else if (LotFieldsList[x].Name == "LotNum")
                                LotFieldsList[x].Value = ClassCommon.LotNumber.ToString();
                            else if (LotFieldsList[x].Name == "JobNum")
                                LotFieldsList[x].Value = ClassCommon.JobNumber.ToString();
                            else if (LotFieldsList[x].Name == "BaleCount")
                                LotFieldsList[x].Value = ClassCommon.BaleCount.ToString();
                            else if (LotFieldsList[x].Name == "MinNWBale")
                                LotFieldsList[x].Value = ClassCommon.MinNetWtBale.ToString();
                            else if (LotFieldsList[x].Name == "MaxNWBale")
                                LotFieldsList[x].Value = ClassCommon.MaxNetWtBale.ToString();
                            else if (LotFieldsList[x].Name == "MinMCBale")
                                LotFieldsList[x].Value = ClassCommon.MinMCBale.ToString();
                            else if (LotFieldsList[x].Name == "MaxMCBale")
                                LotFieldsList[x].Value = ClassCommon.MaxMCBale.ToString();
                            else if (LotFieldsList[x].Name == "NextBaleNumber")
                                LotFieldsList[x].Value = ClassCommon.NextBaleNum.ToString();
                            else if (LotFieldsList[x].Name == "UID")
                                LotFieldsList[x].Value = ClassCommon.UID.ToString();
                            else if (LotFieldsList[x].Name == "QualityUID")
                                LotFieldsList[x].Value = ClassCommon.QualityUID.ToString();
                            else if (LotFieldsList[x].Name == "BaleCount")
                                LotFieldsList[x].Value = ClassCommon.LotBaleNumber.ToString();
                            else
                                LotFieldsList[x].Value = string.Empty;
                                break;

                        case "nvarchar":

                            if (ClassCommon.LotStatus != null && LotFieldsList[x].Name == "LotStatus")
                                LotFieldsList[x].Value = "'" + ClassCommon.LotStatus.ToString() + "'";
                            else if (LotFieldsList[x].Name == "StockName")
                                LotFieldsList[x].Value = "'" + ClassCommon.StockName.ToString() + "'";
                            else if (LotFieldsList[x].Name == "MonthCode")
                                LotFieldsList[x].Value = "'" + ClassCommon.MonthCode.ToString() + "'";
                            else
                                LotFieldsList[x].Value = "'" + string.Empty +  "'";
                            break;

                        case "bit":

                            if (LotFieldsList[x].Name == "CloseBySize")
                                LotFieldsList[x].Value = "'" + ClassCommon.BCloseBySize.ToString() + "'";
                            else if (LotFieldsList[x].Name == "CloseByTime")
                                LotFieldsList[x].Value = "'" + ClassCommon.BCloseByTime.ToString() + "'";
                            else
                                LotFieldsList[x].Value = "'0'";
                                break;

                        case "real":
                        case "float":

                            if (LotFieldsList[x].Name == "TotNW")
                                LotFieldsList[x].Value = ClassCommon.TotalNetWeight.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "MinNW")
                                LotFieldsList[x].Value = ClassCommon.MinNetWeight.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "MaxNW")
                                LotFieldsList[x].Value = ClassCommon.MaxNetWeight.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "NW2N")
                                LotFieldsList[x].Value = ClassCommon.NetWeightSquare.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "MeanNW")
                                LotFieldsList[x].Value = ClassCommon.MeanNetWeight.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "RangeNW")
                                LotFieldsList[x].Value = ClassCommon.RangeNetWeight.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "StdDevNW")
                                LotFieldsList[x].Value = ClassCommon.StdNetWeight.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "TotTW")
                                LotFieldsList[x].Value = ClassCommon.TotalTareWeight.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "TotBW")
                                LotFieldsList[x].Value = ClassCommon.TotalBdWeight.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "TotMC")
                                LotFieldsList[x].Value = ClassCommon.TotalMC.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "MinMC")
                                LotFieldsList[x].Value = ClassCommon.MinMC.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "MaxMC")
                                LotFieldsList[x].Value = ClassCommon.MaxMC.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "MC2M")
                                LotFieldsList[x].Value = ClassCommon.McSquare.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "RangeMC")
                                LotFieldsList[x].Value = ClassCommon.RangeMC.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "StdDevMC")
                                LotFieldsList[x].Value = ClassCommon.StdMC.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "BalesAssigned")
                                LotFieldsList[x].Value = ClassCommon.BaleAssigned.ToString("#00.00");

                            else if (LotFieldsList[x].Name == "ClosingSize")
                                LotFieldsList[x].Value = ClassCommon.ClosingSize.ToString();
                            else
                                LotFieldsList[x].Value = "0";
                            break;

                        case "smallint":
                            if (LotFieldsList[x].Name == "BaleCount")
                                LotFieldsList[x].Value = ClassCommon.LotBaleNumber.ToString();
                            else
                                LotFieldsList[x].Value = "0";
                            break;

                        case "datetime":

                           // Console.WriteLine("LotFieldsList[x].Name =" + LotFieldsList[x].Name + " - "+ ClassCommon.OpenTD.ToString("MM/dd/yyyy H:mm"));
                          
                            if (ClassCommon.OpenTD != null && LotFieldsList[x].Name == "OpenTD")
                                LotFieldsList[x].Value = "'" + ClassCommon.OpenTD.ToString("MM/dd/yyyy H:mm") + "'";
                            else if (ClassCommon.CloseTD != null && LotFieldsList[x].Name == "CloseTD")
                                LotFieldsList[x].Value = "'" + ClassCommon.CloseTD.ToString("MM/dd/yyyy H:mm") + "'";
                            else
                                LotFieldsList[x].Value = "'" + DateTime.Now.ToString("MM/dd/yyyy H:mm") + "'";

                            break;

                        default:
                            break;
                    }
                    strvalues += LotFieldsList[x].Value.ToString() + ",";
                }
                strvalues = strvalues.Trim(charsToTrim);
                strvalues += ")";

                foreach (var item in LotFieldsList)
                {
                    strQuery += "[" + LotFieldsList[i].Name + "],";
                    i++;
                }
                strQuery = strQuery.Trim(charsToTrim);
                strQuery = "INSERT INTO " + SqlTable + "(" + strQuery + ") VALUES " + strvalues + ";";

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in BuildStringInsertLotSql -> " + ex.Message);
            }
            return strQuery;
        }

      

        /// <summary>
        /// Build insert string and included data from ClassCommon.
        /// 
        /// </summary>
        /// <param name="ArchData"></param>
        /// <returns></returns>
        public string BuildStringInsertSql(List<ArchiveData> ArchData)
        {
            string strQuery =  string.Empty;
            string strvalues = "(";
            char[] charsToTrim = { ',' };
            //string values;

            int iLotstatus = 0;

            switch (ClassCommon.LotStatus)
            {
                case "Open":
                    iLotstatus = 0;
                    break;
                case "Closed":
                    iLotstatus = 1;
                    break;
                case "LotReset":
                    iLotstatus = 2;
                    break;
            }

            try
            {
                //int x = 0;
                int i = 0;
                //  foreach (var item in ArchData)
                for (int x = 0; x < ArchData.Count; x++)
                {
                    //Console.WriteLine("index = " + x.ToString() + "----- ArchField " + ArchData[x].m_Name);
                    switch (ArchData[x].Datatype)
                    {
                        case "int":
                            //  Console.WriteLine("int___I__ " + ArchData[x].m_Name);
                            if (ArchData[x].Name == "UID")
                                ArchData[x].Value = ClassCommon.UID.ToString();
                            if (ArchData[x].Name == "Index")
                                ArchData[x].Value = ClassCommon.IIndex.ToString();
                            if (ArchData[x].Name == "Forte")
                                ArchData[x].Value = ClassCommon.DCalcForte.ToString();
                            else if (ArchData[x].Name == "Forte1")
                                ArchData[x].Value = ClassCommon.DCalcForte1.ToString();
                            else if (ArchData[x].Name == "Forte2")
                                ArchData[x].Value = ClassCommon.DCalcForte2.ToString();
                            else if (ArchData[x].Name == "SerialNumber")
                                ArchData[x].Value = ClassCommon.SerialNumber.ToString();
                            else if (ArchData[x].Name == "LotNumber")
                                ArchData[x].Value = ClassCommon.LotNumber.ToString();
                            else if (ArchData[x].Name == "LotBaleNumber")
                                ArchData[x].Value = ClassCommon.LotBaleNumber.ToString();
                            else if (ArchData[x].Name == "JobNum")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "UnitNumber")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "UpCount")
                                ArchData[x].Value = ClassCommon.DUpCount.ToString();
                            else if (ArchData[x].Name == "DownCount")
                                ArchData[x].Value = ClassCommon.DDCount.ToString();
                            else if (ArchData[x].Name == "Index")
                                ArchData[x].Value = ClassCommon.IIndex.ToString();

                            //UpCount
                            // else
                            //     ArchData[x].m_Value = "0";
                            break;

                        case "bit":
                            //  Console.WriteLine("bit___B__ " + ArchData[x].m_Name);
                            ArchData[x].Value = "0";
                            break;

                        case "nvarchar":
                            //  Console.WriteLine("Varchar___V__ " + ArchData[x].m_Name);

                            if (ArchData[x].Name == "StockName")
                                ArchData[x].Value = "'" + ClassCommon.StockName + "'";
                            else if (ArchData[x].Name == "CalibrationName")
                                ArchData[x].Value = "'" + ClassCommon.CalibrationName + "'";
                            else if (ArchData[x].Name == "ShiftName")
                                ArchData[x].Value = "'" + ClassCommon.ShiftName + "'";
                            else if (ArchData[x].Name == "StockLabel1")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "StockLabel2")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "StockLabel3")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "StockLabel4")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "UnitIdent")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "FC_LotIdentString")
                                ArchData[x].Value = "1234";
                            else if (ArchData[x].Name == "GradeLabel2")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "MoistMes")
                                ArchData[x].Value = "'" + ClassCommon.MoistMes + "'";
                            else if (ArchData[x].Name == "WtMes")
                                ArchData[x].Value = "'" + ClassCommon.WtMes + "'";

                            //ClassCommon._WtMes 
                            //GradeLabel2
                            else
                                ArchData[x].Value = "' " + "'";
                            break;

                        case "float":
                            if (ArchData[x].Name == "TotNW")
                                ArchData[x].Value = "0";

                            break;

                        case "real":
                            // Console.WriteLine("Read___R__ " + ArchData[x].m_Name);

                            if (ArchData[x].Name == "Moisture")
                                if (ClassCommon.RealMoisture > 0)
                                    ArchData[x].Value = ClassCommon.RealMoisture.ToString("#0.00");
                                else
                                    ArchData[x].Value = "0.00";
                            else if (ArchData[x].Name == "Weight")
                                ArchData[x].Value = ClassCommon.DScaleWeight.ToString("#0.00");
                            else if (ArchData[x].Name == "NetWeight")
                                ArchData[x].Value = ClassCommon.DNetWeight.ToString("#0.00");
                            else if (ArchData[x].Name == "TareWeight")
                                ArchData[x].Value = ClassCommon.DTareWeight.ToString("#0.00");
                            else if (ArchData[x].Name == "SpareSngFld3")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "SpareSngFld2")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "SpareSngFld1")
                                ArchData[x].Value = "NULL";
                            else
                                ArchData[x].Value = "0";
                            break;

                        //OrigWeight

                        case "smallint":
                            // Console.WriteLine("smallInt___sI__ " + ArchData[x].m_Name);
                            if (ArchData[x].Name == "WLAStatus")
                                ArchData[x].Value = "0";
                            else if (ArchData[x].Name == "UnitNumberStatus")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "UnitBaleNumber")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "LotNumberStatus")
                                ArchData[x].Value = iLotstatus.ToString();
                            else
                                ArchData[x].Value = "0";
                            break;

                        case "datetime":
                            // Console.WriteLine("Datatime___Dt__ " + ArchData[x].m_Name);
                            if (ArchData[x].Name == "TimeComplete")
                                ArchData[x].Value = "'" + DateTime.Now.ToString("MM/dd/yyyy H:mm") + "'";
                            else if (ArchData[x].Name == "ProdDayStart")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "ProdDayEnd")
                                ArchData[x].Value = "NULL";
                            else if (ArchData[x].Name == "DropTime")
                                ArchData[x].Value = "NULL";
                            break;

                        default:
                            break;
                    }
                    strvalues += ArchData[x].Value.ToString() + ",";
                }
                strvalues = strvalues.Trim(charsToTrim);
                strvalues += ")";

                foreach (var item in ArchData)
                {
                    strQuery += "[" + ArchData[i].Name + "],";
                    i++;
                }
                strQuery = strQuery.Trim(charsToTrim);
                strQuery = "INSERT INTO " + m_CurrentAchivesTable + "(" + strQuery + ") VALUES " + strvalues + ";";
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in BuildInsertSqlString " + ex.Message);
            }
            return strQuery;
        }

        public void DeleteSingleStockRecord(string stockSelectValue)
        {
            string strDelQuery = "DELETE FROM StockTable WHERE Name = '" + stockSelectValue + "'";

            try
            {
                using (var sqlConnection = new SqlConnection(WinAuProdConStr))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    using (SqlCommand command = new SqlCommand(strDelQuery, sqlConnection))
                    {
                        command.ExecuteNonQuery();
                    }
                    sqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in DeleteSingleStockRecord -> " + ex.Message);
            }
        }

        public DataTable GetLotArchiveTable(string strquery)
        {
            //Console.WriteLine("strquery= " + strquery);
            DataTable mytable = new DataTable();
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuTransConStr))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(strquery, sqlConnection))
                    {
                        adapter.Fill(mytable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetLotArchiveTable " + ex.Message);
            }
            return mytable;
        }

      

        public List<Tuple<string, string>> GetTableSchema()
        {
            DataTable dx = new DataTable();
            List<Tuple<string, string>> mylist = new List<Tuple<string, string>>();
            string strQuery = "SELECT ORDINAL_POSITION,COLUMN_NAME,DATA_TYPE FROM ForteData.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + GetCurrentArchivesTable() + "'";

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(strQuery, sqlConnection))
                    {
                        adapter.Fill(dx);
                    }
                }
                foreach (var item in this.RemoveFieldsList)
                {
                    RemoveHrdItem(dx, item);
                }

                foreach (DataRow item in dx.Rows)
                {
                    mylist.Add(new Tuple<string, string>(item[1].ToString(), item[2].ToString()));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetSqlTableHdr -> " + ex.Message);
            }
            return mylist;
        }

        public List<string> GetSqltableList()
        {
            List<string> tablelist = new List<string>();
            string strquery = "SELECT [name],create_date FROM sys.tables WHERE [name] LIKE '%BaleArchive%' ORDER BY create_date DESC";

            try
            {
                using (var sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    using (var command = new SqlCommand(strquery, sqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                                while (reader.Read())
                                {
                                    if (reader.GetString(0) != null)
                                        tablelist.Add(reader.GetString(0));
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GettableList -> " + ex.Message);
                //  MainWindow.AppWindows.LogObject.LogMessage(MsgTypes.WARNING, MsgSources.DBSQL, "GettableList " + ex.Message);
            }
            return tablelist;
        }

        public DataTable GetSqlArchivetable(string strClause)
        {
            DataTable mytable = new DataTable();

            try
            {
                using (var sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    using (var adapter = new SqlDataAdapter(strClause, sqlConnection))
                    {
                        adapter.Fill(mytable);
                    }
                }

                DataColumnCollection columns = mytable.Columns;
                if (columns.Contains("Moisture"))
                {
                    DataRow[] rows = mytable.Select();
                    for (int i = 0; i < rows.Length; i++)
                    {
                        switch (ClassCommon.MoistureType)
                        {
                            case 0: // %MC == moisture from Sql database
                                if ((columns.Contains("Moisture")) & (rows[i]["Moisture"] != null))
                                    rows[i]["Moisture"] = rows[i]["Moisture"];
                                break;

                            case 1: // %MR  = Moisture / ( 1- Moisture / 100)
                                if ((columns.Contains("Moisture")) & (rows[i]["Moisture"] != null))
                                    rows[i]["Moisture"] = rows[i].Field<Single>("Moisture") / (1 - rows[i].Field<Single>("Moisture") / 100);
                                break;

                            case 2: // %AD = (100 - moisture) / 0.9
                                if ((columns.Contains("Moisture")) & (rows[i]["Moisture"] != null))
                                    rows[i]["Moisture"] = (100 - rows[i].Field<Single>("Moisture")) / 0.9;
                                break;

                            case 3: // %BD  = 100 - moisture
                                if ((columns.Contains("Moisture")) & (rows[i]["Moisture"] != null))
                                    rows[i]["Moisture"] = 100 - rows[i].Field<Single>("Moisture");
                                break;
                        }

                        if (ClassCommon.WeightType == 1) //English Unit lb
                        {
                            if ((columns.Contains("Weight")) & (rows[i]["Weight"] != null))
                                rows[i]["Weight"] = (rows[i].Field<Single>("Weight") * 2.20462); //Lb.
                            if ((columns.Contains("BDWeight")) & (rows[i]["BDWeight"] != null))
                                rows[i]["BDWeight"] = (rows[i].Field<Single>("BDWeight") * 2.20462); //Lb.
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in GetSqlArchivetable " + ex.Message);
            }
            return mytable;
        }

        public bool UpdateProductTable(string strUpdateQuery)
        {
            return ExecuteCommand(strUpdateQuery, WinAuProdConStr);
        }


        public string GetCurrentLotArchivesTable()
        {
            return GetCurrentLotArchTable();
        }

        public DataTable GetDatabyLotNumber(int lotNumber, long Balecount)
        {
            DataTable MyTable = new DataTable();
            string strQuery = "SELECT Top " + Balecount + " * from " + m_CurrentAchivesTable + " WHERE LotNumber = " + lotNumber.ToString()  + " ORDER BY UID DESC;";
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(strQuery, sqlConnection))
                    {
                        adapter.Fill(MyTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("SQL Handler ERROR in GetDatabyLotNumber " + ex.Message);
            }
            return MyTable;
        }

        private string GetCurrentLotArchTable()
        {
            List<string> tablelist = new List<string>();
            string strquery = "SELECT top 2 [name],create_date FROM sys.tables WHERE [name] LIKE '%LotArchive%' ORDER BY create_date DESC";

            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    using (var command = new SqlCommand(strquery, sqlConnection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                                while (reader.Read())
                                {
                                    if (reader.GetString(0) != null)
                                        tablelist.Add(reader.GetString(0));
                                }
                        }
                    }
                    if (sqlConnection != null) sqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("SQL Handler Error in GetCurrentLotArchTable " + ex.Message);
            }
            return tablelist[0].ToString();
        }
        public DataTable GetSqlTableHdr()
        {
            DataTable dx = new DataTable();
            string strQuery = "SELECT ORDINAL_POSITION,COLUMN_NAME,DATA_TYPE FROM ForteData.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + GetCurrentArchivesTable() + "'";

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(strQuery, sqlConnection))
                    {
                        adapter.Fill(dx);
                    }
                }
                foreach (var item in this.RemoveFieldsList)
                {
                    RemoveHrdItem(dx, item);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error in GetSqlTableHdr -> " + ex.Message);
                ClassCommon.MyInfoLog.LogMessage(MsgTypes.WARNING, MsgSources.DBSQL, "GetSqlTableHdr " + ex.Message);
            }
            return dx;
        }

        public List<string> GetTableItemLists(string strTableName)
        {
            List<string> itemlist = new List<string>();
            string strquery = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + strTableName + "';";

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(WinAuRealtimeConStr))
                {
                    if (sqlConnection != null) sqlConnection.Open();
                    using (var command = new SqlCommand(strquery, sqlConnection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                                while (reader.Read())
                                {
                                    if (reader.GetString(3) != null)
                                        itemlist.Add(reader.GetString(3));
                                }
                        }
                    }
                    if (sqlConnection != null) sqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR in RunSqlScripts" + ex.Message);
            }
            return itemlist;
        }

        public DataTable GetCalibrationTable()
        {
            DataTable dx = new DataTable();
            string strQuery = "SELECT * FROM CalTable WHERE Status = 0 ORDER by ID ASC;";

            try
            {
                using (var sqlConnection = new SqlConnection(WinAuProdConStr))
                {
                    using (var adapter = new SqlDataAdapter(strQuery, sqlConnection))
                    {
                        adapter.Fill(dx);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetCalibrationTable -> " + ex.Message);
            }
            return dx;
        }

        public DataTable GetSqlCalParamsByProdIdTable(string calId)
        {
            DataTable TempTable = new DataTable();
            string strQuery = "SELECT Name,A,B,C FROM CalTable WHERE ID = " + calId;

            try
            {
                using (var sqlConnection = new SqlConnection(WinAuProdConStr))
                {
                    using (var adapter = new SqlDataAdapter(strQuery, sqlConnection))
                    {
                        adapter.Fill(TempTable);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetSqlCalParamsByProdIdTable -> " + ex.Message);
            }
            return TempTable;
        }



        public void SetRemoveFields()
        {
            if (RemoveFieldsList == null)
            {
                RemoveFieldsList = new List<string>
                {
                    "Index",
                    "Empty",
                    "QualityUID",
                    "AsciiFld1",
                    "AsciiFld2",
                    "OrderStr",
                    "QualityName",
                    "GradeLabel1",
                    "StockLabel1",
                    "StockLabel2",
                    "StockLabel3",
                    "StockLabel4",
                    //"JobNum",
                    //RemoveFieldsList.Add("Forte1");
                    //RemoveFieldsList.Add("Forte2");
                    "ForteAveraging",
                    //ItemRemoveLst.Add("UpCount")
                    //ItemRemoveLst.Add("DownCount")
                    "DownCount2",
                    "Brightness",
                    "BaleHeight",
                    "SourceID",
                    "Finish",
                    "SheetArea",
                    "SheetCount",
                    "CalibrationID",
                    "PkgMoistMethod",
                    //RemoveFieldsList.Add("SpareSngFld1");
                    //RemoveFieldsList.Add("SpareSngFld2");
                    "SpareSngFld3",
                    "LastInGroup",
                    //RemoveFieldsList.Add("MoistMes");
                    "ProdDayStart",
                    "ProdDayEnd",
                    "LineID",
                    "StockID",
                    "GradeID",
                    //RemoveFieldsList.Add("WtMes");
                    "AsciiFld3",
                    "AsciiFld4",
                    "SR",
                    "UID",
                    "Package",
                    "ResultDesc",
                    "GradeLabel2",
                    "WLAlarm",
                    "WLAStatus",
                    //
                    "Status",
                    "WeightStatus",
                    "TemperatureStatus",
                    "OrigWeightStatus",
                    "ForteStatus",
                    "Forte1Status",
                    "Forte2Status",
                    "UpCountStatus",
                    "DownCountStatus",
                    "DownCount2Status",
                    "BrightnessStatus",
                    "TimeStartStatus",
                    "BaleHeightStatus",
                    "TimeCompleteStatus",
                    "SourceIDStatus",
                    "StockIDStatus",
                    "GradeIDStatus",
                    "TareWeightStatus",
                    "AllowanceStatus",
                    "SheetCountStatus",
                    "MoistureStatus",
                    "NetWeightStatus",
                    "CalibrationIDStatus",
                    "SeriAlNumberStatus",
                    //RemoveFieldsList.Add("LotNumberStatus");
                    "TemperatureStatus",
                    "UnitNumberStatus",
                    "UnitIdent",
                    "UnitBaleNumber",
                    "UnitNumber",
                    "Temperature",
                    "FC_IdentString",
                    "FC_LotIdentString",
                    "Dirt",
                    "DropTime",
                    "Position",
                    "DropNumber",
                    "GradeName",
                    //Last removed
                    "SourceName",
                    "LineName",
                    "OrigWeight",
                    // RemoveFieldsList.Add("LotNumber");
                    // RemoveFieldsList.Add("LotBaleNumber");
                    "LotIdent",
                    "LotIdent",
                    "BasisWeight",
                    "BDWeight",
                    "MonthCode"
                };
            }
        }

        private string IndexTablescript(string strTableName)
        {
            return (@"CREATE TABLE[dbo].[" + strTableName + "]" +
                "[Bale] [bigint] NULL," +
                "[Lot] [bigint] NULL," +
                "[Quality] [bigint] NULL," +
                "[Unit] [bigint] NULL" +
                " ON[PRIMARY];");
        }

        private string ArchiveTablescript(string strTableName)
        {
            return (@"CREATE TABLE[dbo].[" + strTableName + "]" +
              "([Index][int] NULL," +
              "[Empty][bit] NULL," +
              "[SourceName][nvarchar](255) NULL," +
              "[LineName] [nvarchar] (255) NULL," +
              "[StockName] [nvarchar] (255) NULL," +
              "[CalibrationName] [nvarchar] (255) NULL," +
              "[GradeName] [nvarchar] (255) NULL," +
              "[Status] [smallint] NULL," +
              "[Weight] [real] NULL," +
              "[WeightStatus] [smallint] NULL," +
              "[OrigWeight] [real] NULL," +
              "[OrigWeightStatus] [smallint] NULL," +
              "[Forte] [int] NULL," +
              "[ForteStatus] [smallint] NULL," +
              "[ForteAveraging] [bit] NULL," +
              "[Forte1] [int] NULL," +
              "[Forte1Status] [smallint] NULL," +
              "[Forte2] [int] NULL," +
              "[Forte2Status] [smallint] NULL," +
              "[UpCount] [int] NULL," +
              "[UpCountStatus] [smallint] NULL," +
              "[DownCount] [int] NULL," +
              "[DownCountStatus] [smallint] NULL," +
              "[DownCount2] [int] NULL," +
              "[DownCount2Status] [smallint] NULL," +
              "[Brightness] [real] NULL," +
              "[BrightnessStatus] [smallint] NULL," +
              "[BaleHeight] [real] NULL," +
              "[BaleHeightStatus] [smallint] NULL," +
              "[TimeStart] [datetime] NULL," +
              "[TimeStartStatus] [smallint] NULL," +
              "[TimeComplete] [datetime] NULL," +
              "[TimeCompleteStatus] [smallint] NULL," +
              "[SourceID] [int] NULL," +
              "[SourceIDStatus] [smallint] NULL," +
              "[LineID] [int] NULL," +
              "[StockID] [int] NULL," +
              "[StockIDStatus] [smallint] NULL," +
              "[GradeID] [int] NULL," +
              "[GradeIDStatus] [smallint] NULL," +
              "[TareWeight] [real] NULL," +
              "[TareWeightStatus] [smallint] NULL," +
              "[Finish] [real] NULL," +
              "[AllowanceStatus] [smallint] NULL," +
              "[DropNumber] [int] NULL," +
              "[DropTime] [datetime] NULL," +
              "[Position] [tinyint] NULL," +
              "[SheetCount] [smallint] NULL," +
              "[SheetCountStatus] [smallint] NULL," +
              "[SheetArea] [real] NULL," +
              "[Moisture] [real] NULL," +
              "[MoistureStatus] [smallint] NULL," +
              "[NetWeight] [real] NULL," +
              "[NetWeightStatus] [smallint] NULL," +
              "[BDWeight] [real] NULL," +
              "[CalibrationID] [int] NULL," +
              "[CalibrationIDStatus] [smallint] NULL," +
              "[SerialNumber] [int] NULL," +
              "[SeriAlNumberStatus] [smallint] NULL," +
              "[LotNumber] [int] NULL," +
              "[LotNumberStatus] [smallint] NULL," +
              "[LotBaleNumber] [int] NULL," +
              "[PkgMoistMethod] [int] NULL," +
              "[SpareSngFld1] [real] NULL," +
              "[SpareSngFld2] [real] NULL," +
              "[QualityUID] [int] NULL," +
              "[LastInGroup] [bit] NULL," +
              "[MoistMes] [nvarchar] (10) NULL," +
              "[WtMes] [nvarchar] (10) NULL," +
              "[ProdDayStart] [datetime] NULL," +
              "[ProdDayEnd] [datetime] NULL," +
              "[ShiftName] [nvarchar] (10) NULL," +
              "[MonthCode] [nvarchar] (1) NULL," +
              "[AsciiFld1] [nvarchar] (25) NULL," +
              "[AsciiFld2] [nvarchar] (25) NULL," +
              "[AsciiFld3] [nvarchar] (25) NULL," +
              "[AsciiFld4] [nvarchar] (25) NULL," +
              "[SR] [real] NULL," +
              "[UID] [int] NULL," +
              "[OrderStr] [nvarchar] (25) NULL," +
              "[Package] [nvarchar] (12) NULL," +
              "[ResultDesc] [nvarchar] (4) NULL," +
              "[Temperature] [real] NULL," +
              "[TemperatureStatus] [smallint] NULL," +
              "[LotIdent] [nvarchar] (16) NULL," +
              "[Dirt] [real] NULL," +
              "[BasisWeight] [real] NULL," +
              "[QualityName] [nvarchar] (50) NULL," +
              "[FC_IdentString] [nvarchar] (50) NULL," +
              "[StockLabel1] [nvarchar] (50) NULL," +
              "[GradeLabel1] [nvarchar] (50) NULL," +
              "[StockLabel2] [nvarchar] (50) NULL," +
              "[GradeLabel2] [nvarchar] (50) NULL," +
              "[FC_LotIdentString] [nvarchar] (25) NULL," +
              "[UnitBaleNumber] [smallint] NULL," +
              "[UnitNumber] [int] NULL," +
              "[UnitNumberStatus] [smallint] NULL," +
              "[UnitIdent] [nvarchar] (20) NULL," +
              "[WLAlarm] [smallint] NULL," +
              "[WLAStatus] [smallint] NULL," +
              "[JobNum] [int] NULL," +
              "[SpareSngFld3] [real] NULL," +
              "[StockLabel3] [nvarchar] (50) NULL," +
              "[StockLabel4] [nvarchar] (50) NULL)" +
              " ON[PRIMARY];");
        }

        private string LotTableScript(string strTableName)
        {
            return (@"CREATE TABLE[dbo].[" + strTableName + "]" +
            "([Index] [int] NULL," +
            "[LotNum] [int] NULL," +
            "[LotStatus] [nvarchar](255) NULL," +
            "[Empty][bit] NULL," +
            "[PriGrp] [smallint] NULL," +
            "[SecGrp] [smallint] NULL," +
            "[GroupingID] [smallint] NULL, " +
            "[GroupText] [nvarchar](255) NULL, " +
            "[OnHold] [bit] NULL," +
            "[JobNum] [bit] NULL," +
            "[OpenTD] [datetime] NULL," +
            "[CloseTD] [datetime] NULL," +
            "[CloseBySize] [bit] NULL," +
            "[CloseByTime] [bit] NULL," +
            "[BaleCount] [smallint] NULL," +
            "[TotNW] [float] NULL," +
            "[MinNW] [real] NULL," +
            "[MinNWBale] [int] NULL," +
            "[MaxNW] [real] NULL," +
            "[MaxNWBale] [int] NULL," +
            "[NW2N] [real] NULL," +
            "[MeanNW] [real] NULL," +
            "[RangeNW] [real] NULL," +
            "[StdDevNW] [real] NULL," +
            "[TotTW] [real] NULL," +
            "[TotBW] [real] NULL," +
            "[TotMC] [real] NULL," +
            "[MinMC] [real] NULL," +
            "[MinMCBale] [int] NULL," +
            "[MaxMC] [real] NULL," +
            "[MaxMCBale] [int] NULL," +
            "[MC2M] [real] NULL," +
            "[RangeMC] [real] NULL," +
            "[StdDevMC] [real] NULL," +
            "[NextBaleNumber] [real] NULL," +
            "[BalesAssigned] [real] NULL," +
            "[ClosingSize] [real] NULL," +
            "[Action] [nvarchar](16) NULL," +
            "[SR] [real] NULL," +
            "[Finish] [real] NULL," +
            "[OrderStr] [nvarchar](25) NULL," +
            "[UID] [int] NULL," +
            "[AsciiFld1] [nvarchar](20) NULL," +
            "[AsciiFld2] [nvarchar](20) NULL," +
            "[AsciiFld3] [nvarchar](20) NULL," +
            "[AsciiFld4] [nvarchar](20) NULL," +
            "[SpareSngFld1] [real] NULL," +
            "[SpareSngFld2] [real] NULL," +
            "[LotIdent] [nvarchar](16) NULL," +
            "[QualityUID] [int] NULL," +
            "[StockName] [nvarchar](20) NULL," +
            "[FC_IdentString] [nvarchar](25) NULL," +
            "[UnitCount] [smallint] NULL," +
            "[SpareSngFld3] [real] NULL," +
            "[MonthCode] [nvarchar](20) NULL)" +
            "ON[PRIMARY];");
        }

        private string GetCreateScript(string strTable)
        {
            string retStr = string.Empty;

            if (strTable == SQLSTOCKTABLE)
                retStr = StockTableScript();
            if (strTable == SQLGRADETABLE)
                retStr = GradesTableScript();
            if (strTable == SQLCALTABLE)
                retStr = CalibrationTableScript();
            if (strTable == SQLLOTTRANSTABLE)
                retStr = LotTransactionScript();
            if (strTable == SQLOPENLOTSTABLE)
                retStr = OpenLotsScript();
           // if (strTable == SQLLOTARCHSTABLE)
           //     retStr = LotArchivesScript();
            return retStr;
        }



        private string StockTableScript()
        {
            return (@"CREATE TABLE[dbo].[" + SQLSTOCKTABLE + "]" +
                "([ID] [smallint] NULL," +
                "[Name] [nvarchar] (20) NULL," +
                "[CalIDLn1] [smallint] NULL," +
                "[CalIDLn2] [smallint] NULL," +
                "[CalIDLn3] [smallint] NULL," +
                "[TareWeight] [real] NULL," +
                "[WrapperStockID] [smallint] NULL," +
                "[WrapperStyleID] [smallint] NULL," +
                "[DefaultWeight] [real] NULL," +
                "[DefaultNetWeight] [real] NULL," +
                "[DefaultForte] [smallint] NULL," +
                "[DefaultMoisture] [real] NULL," +
                "[MoistConv] [smallint] NULL," +
                "[DefaultGradeID] [smallint] NULL," +
                "[DefaultCriteriaID] [smallint] NULL," +
                "[DefaultBrightness] [real] NULL," +
                "[DefaultTemp] [real] NULL," +
                "[DefaultSheetCount] [smallint] NULL," +
                "[DefaultSheetArea] [real] NULL," +
                "[DefaultBasisWt] [real] NULL," +
                "[NextBaleNumber] [int] NULL," +
                "[NextLotNumber] [int] NULL," +
                "[CurrentBaleNumber] [int] NULL," +
                "[TargetCount] [int] NULL," +
                "[TargetBaleWt] [real] NULL," +
                "[TargetBaleWtTol] [real] NULL," +
                "[TargetBaleWtType] [smallint] NULL," +
                "[SampleCount] [smallint] NULL," +
                "[Finish] [real] NULL," +
                "[MinWeight] [real] NULL," +
                "[MaxWeight] [real] NULL," +
                "[MinMoisture] [real] NULL," +
                "[MaxMoisture] [real] NULL," +
                "[WTUseCaution] [bit] NULL," +
                "[MUseCaution] [bit] NULL," +
                "[TareWeight2] [real] NULL," +
                "[TareWeight3] [real] NULL," +
                "[CautionWTLow] [real] NULL," +
                "[CautionWTHigh] [real] NULL," +
                "[CautionMoistLow] [real] NULL," +
                "[CautionMoistHigh] [real] NULL," +
                "[StdRegain] [real] NULL," +
                "[CalRange1Low] [real] NULL," +
                "[CalRange1High] [real] NULL," +
                "[CalRange2Low] [real] NULL," +
                "[CalRange2High] [real] NULL," +
                "[CalRange3Low] [real] NULL," +
                "[CalRange3High] [real] NULL," +
                "[CrossoverMoist] [real] NULL," +
                "[CrossoverCal] [real] NULL," +
                "[HtConst] [float] NULL," +
                "[NomHt] [float] NULL," +
                "[WetLapHtConst] [float] NULL," +
                "[WetLapNomHt] [float] NULL," +
                "[WetLapWtFctr] [float] NULL," +
                "[SheetArea] [real] NULL," +
                "[SRCRestriction] [tinyint] NULL," +
                "[Label1] [nvarchar] (50) NULL," +
                "[Label2] [nvarchar] (50) NULL," +
                "[CalIDLn4] [smallint] NULL," +
                "[CalIDLn5] [smallint] NULL," +
                "[Label3] [nvarchar] (50) NULL," +
                "[Label4] [nvarchar] (50) NULL," +
                "[IsReject] [bit] NULL," +
                "[RejInc] [bit] NULL)" +
                "ON[PRIMARY];");
        }

        private string GradesTableScript()
        {
            return (@"CREATE TABLE[dbo].[" + SQLGRADETABLE + "]" +
            "([ID] [smallint] NULL," +
            "[Name] [nvarchar] (20) NULL," +
            "[Label1] [nvarchar] (50) NULL," +
            "[Label2][nvarchar](50) NULL," +
            "[LnReset] [nvarchar] (25) NULL," +
            "[Size] [smallint] NULL, " +
            "[RailCarCap] [smallint] NULL," +
            "[NextLotNumber] [smallint] NULL," +
            "[DateCreated] [datetime] NULL," +
            "[DateModified] [datetime] NULL," +
            "[StockID] [smallint] NULL," +
            "[BarcodeStencil] [bit] NULL," +
            "[WrapperStockID] [smallint] NULL," +
            "[WrapperStyleID] [smallint] NULL," +
            "[StackSize] [smallint] NULL," +
            "[UnitSize] [smallint] NULL," +
            "[MinMoisture] [real] NULL," +
            "[MaxMoisture] [real] NULL," +
            "[MinWeight] [real] NULL," +
            "[MaxWeight] [real] NULL," +
            "[CautionWTLow] [real] NULL," +
            "[CautionWTHigh] [real] NULL," +
            "[CautionMoistLow] [real] NULL," +
            "[CautionMoistHigh] [real] NULL," +
            "[WTUseCaution] [bit] NULL," +
            "[MUseCaution] [bit] NULL," +
            "[IsReject] [bit] NULL," +
            "[RejInc] [bit] NULL)" +
            "ON[PRIMARY];");
        }

        private string CalibrationTableScript()
        {
            return (@"CREATE TABLE[dbo].[" + SQLCALTABLE + "]" +
                "([ID] [smallint] NOT NULL DEFAULT 0," +
                "[Name] [nvarchar] (20) NULL," +
                "[A] [float] DEFAULT 0," +
                "[B] [float] DEFAULT 0," +
                "[C] [float] DEFAULT 0," +
                "[D] [float] DEFAULT 0," +
                "[Method] [nvarchar] (17) NULL," +
                "[LabBasis] [smallint] DEFAULT 1," +
                "[DateCreated] [datetime]," +
                "[DateModified] [datetime]," +
                "[WtUnits] [nvarchar] (1) NULL," +
                "[Function] [nvarchar] (16) NULL," +
                "[FunctionID] [smallint] DEFAULT 1," +
                "[Status] [smallint] DEFAULT 1," +
                "[DelDate] [datetime]," +
                "[FC_E] [real] DEFAULT 1," +
                "[FC_M] [real] DEFAULT 1," +
                "[Equation] [nvarchar] (MAX) NULL)" +
                "ON[PRIMARY];");

        }

        private string LotTransactionScript()
        {
            return (@"CREATE TABLE[" + SQLLOTTRANSTABLE + "]" +
                "([Index][int]," +
                "[LotNum] [int]," +
                "[LotStatus] [nvarchar](255)," +
                "[Empty] [bit] NOT NULL," +
                "[PriGrp] [smallint]," +
                "[SecGrp] [smallint]," +
                "[GroupingID] [smallint]," +
                "[GroupText] [nvarchar] (255)," +
                "[OnHold] [bit] NOT NULL," +
                "[JobNum] [int]," +
                "[OpenTD] [datetime]," +
                "[CloseTD] [datetime]," +
                "[CloseBySize] [bit] NOT NULL," +
                "[CloseByTime] [bit] NOT NULL," +
                "[BaleCount] [smallint]," +
                "[TotNW] [float]," +
                "[MinNW] [real]," +
                "[MinNWBale] [int]," +
                "[MaxNW] [real]," +
                "[MaxNWBale] [int]," +
                "[NW*NW] [real]," +
                "[MeanNW] [real]," +
                "[RangeNW] [real]," +
                "[StdDevNW] [real]," +
                "[TotTW] [real]," +
                "[TotBW] [real]," +
                "[TotMC] [real]," +
                "[MinMC] [real]," +
                "[MinMCBale] [int]," +
                "[MaxMC] [real]," +
                "[MaxMCBale] [int]," +
                "[MC*MC] [real]," +
                "[RangeMC] [real]," +
                "[StdDevMC] [real]," +
                "[NextBaleNumber] [real]," +
                "[BalesAssigned] [real]," +
                "[ClosingSize] [real]," +
                "[Action] [nvarchar](16)," +
                "[SR] [real] DEFAULT 0," +
                "[Finish] [real] DEFAULT 0," +
                "[OrderStr] [nvarchar](25)," +
                "[UID] [int]," +
                "[SpareSngFld1] [real]," +
                "[SpareSngFld2] [real]," +
                "[AsciiFld1] [nvarchar](20)," +
                "[AsciiFld2] [nvarchar](20)," +
                "[AsciiFld3] [nvarchar](20)," +
                "[AsciiFld4] [nvarchar](20)," +
                "[LotIdent] [nvarchar](16)," +
                "[QualityUID] [int]," +
                "[StockName] [nvarchar](20)," +
                "[FC_IdentString] [nvarchar](25)," +
                "[UnitCount] [smallint]," +
                "[MonthCode] [nvarchar](20)," +
                "[SpareSngFld3] [real])" +
                "ON[PRIMARY];");
        }

        private string OpenLotsScript()
        {
            return (@"CREATE TABLE[" + SQLOPENLOTSTABLE + "]" +
                "([Index] [int] DEFAULT 0," +
                "[LotNum] [int]," +
                "[LotStatus] [nvarchar](15)," +
                "[Empty] [bit] NOT NULL," +
                "[PriGrp] [smallint]," +
                "[SecGrp] [smallint]," +
                "[GroupingID] [smallint]," +
                "[GroupText] [nvarchar](20)," +
                "[OnHold] [bit] NOT NULL," +
                "[JobNum] [int]," +
                "[OpenTD] [datetime]," +
                "[CloseTD] [datetime]," +
                "[CloseBySize] [bit] NOT NULL," +
                "[CloseByTime] [bit] NOT NULL," +
                "[BaleCount] [smallint]," +
                "[TotNW]  [float]," +
                "[MinNW] [real]," +
                "[MinNWBale] [int]," +
                "[MaxNW] [real]," +
                "[MaxNWBale] [int]," +
                "[NW*NW] [real]," +
                "[MeanNW] [real]," +
                "[RangeNW] [real]," +
                "[StdDevNW] [real]," +
                "[TotTW] [real]," +
                "[TotBW] [real]," +
                "[TotMC] [real] DEFAULT 0," +
                "[MinMC] [real]," +
                "[MinMCBale] [int]," +
                "[MaxMC] [real]," +
                "[MaxMCBale] [int]," +
                "[MC*MC] [real]," +
                "[RangeMC] [real]," +
                "[StdDevMC] [real]," +
                "[NextBaleNumber] [real] DEFAULT 0," +
                "[BalesAssigned] [real] DEFAULT 0," +
                "[ClosingSize] [real] DEFAULT 0," +
                "[Action] [nvarchar](16)," +
                "[SR] [real] DEFAULT 0," +
                "[Finish] [real] DEFAULT 0," +
                "[OrderStr] [nvarchar](25)," +
                "[UID] [int]," +
                "[SpareSngFld1] [real]," +
                "[SpareSngFld2] [real]," +
                "[AsciiFld1] [nvarchar](20)," +
                "[AsciiFld2] [nvarchar](20)," +
                "[AsciiFld3] [nvarchar](20)," +
                "[AsciiFld4] [nvarchar](20)," +
                "[LotIdent] [nvarchar](16)," +
                "[QualityUID] [int]," +
                "[StockName] [nvarchar](20)," +
                "[FC_IdentString] [nvarchar](25)," +
                "[UnitCount] [smallint]," +
                "[MonthCode] [nvarchar](20)," +
                "[SpareSngFld3] [real])" +
                "ON[PRIMARY];");
        }


        private string LotArchivesScript()
        {
            return (@"CREATE TABLE[" + SQLLOTARCHSTABLE + "]" +
                "([Index] [int]," +
                "[LotNum] [int]," +
                "[LotStatus] [nvarchar](255)," +
                "[Empty] [bit] NOT NULL," +
                "[PriGrp] [smallint]," +
                "[SecGrp] [smallint]," +
                "[GroupingID] [smallint]," +
                "[GroupText] [nvarchar](255)," +
                "[OnHold] [bit] NOT NULL," +
                "[JobNum] [int]," +
                "[OpenTD] [datetime]," +
                "[CloseTD] [datetime]," +
                "[CloseBySize] [bit] NOT NULL," +
                "[CloseByTime] [bit] NOT NULL," +
                "[BaleCount] [smallint]," +
                "[TotNW] [float]," +
                "[MinNW] [real]," +
                "[MinNWBale] [int]," +
                "[MaxNW] [real]," +
                "[MaxNWBale] [int]," +
                "[NW*NW] [real]," +
                "[MeanNW] [real]," +
                "[RangeNW] [real]," +
                "[StdDevNW] [real]," +
                "[TotTW] [real]," +
                "[TotBW] [real]," +
                "[TotMC] [real]," +
                "[MinMC] [real]," +
                "[MinMCBale] [int]," +
                "[MaxMC] [real]," +
                "[MaxMCBale] [int]," +
                "[MC*MC] [real]," +
                "[RangeMC] [real]," +
                "[StdDevMC] [real]," +
                "[NextBaleNumber] [real]," +
                "[BalesAssigned] [real]," +
                "[ClosingSize] [real]," +
                "[Action] [nvarchar](16)," +
                "[SR] [real] DEFAULT 0," +
                "[Finish] [real] DEFAULT 0," +
                "[OrderStr] [nvarchar](25)," +
                "[UID] [int]," +
                "[SpareSngFld1] [real]," +
                "[SpareSngFld2] [real]," +
                "[AsciiFld1] [nvarchar](20)," +
                "[AsciiFld2] [nvarchar](20)," +
                "[AsciiFld3] [nvarchar](20)," +
                "[AsciiFld4] [nvarchar](20)," +
                "[LotIdent] [nvarchar](16)," +
                "[QualityUID] [int]," +
                "[StockName] [nvarchar](20)," +
                "[FC_IdentString] [nvarchar](25)," +
                "[UnitCount] [smallint]," +
                "[MonthCode] [nvarchar](20)," +
                "[SpareSngFld3] [real])" +
                "ON[PRIMARY];");

        }

        void Createtable()
        {



        }

       
    }
}
