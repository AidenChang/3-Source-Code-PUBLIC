using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using eContract.DDP.Common;
using Suzsoft.Smart.Data;
using System.Xml;
using System.IO;
using System.Collections;
using eContract.DDP.Common.CommonJob;
using System.Windows.Forms;
using Suzsoft.Smart.EntityCore;
using eContract.Log;

namespace eContract.DDP.Common.Command
{
    /// <summary>
    /// ���ݿ�����
    /// </summary>
    public class DatabaseCommand : BaseCommand
    {
        #region ��Ա����

        /// <summary>
        /// Դ��Ϣ
        /// </summary>
        public ConnectionEntity SourceConnEntity = null;

        /// <summary>
        /// Ŀ����Ϣ
        /// </summary>
        public ConnectionEntity TargetConnEntity = null;

        /// <summary>
        /// Դ���ö���
        /// </summary>
        public DataAccessConfiguration SourceDataAccessCfg = null;

        /// <summary>
        /// Ŀ�����ö���
        /// </summary>
        public DataAccessConfiguration TargetDataAccessCfg = null;

        /// <summary>
        /// ��ӳ���ϵ
        /// </summary>
        public List<TableMap> TableMapList = new List<TableMap>();

        /// <summary>
        /// ����¼��
        /// </summary>
        public int MaxCount = -1;

        #endregion

        #region ICommand Members



        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="node"></param>
        public override void Initialize(Hashtable parameters, XmlNode node)
        {
            // �ȵ����ຯ��
            base.Initialize(parameters, node);


            // ȡ������¼������
            if (parameters.ContainsKey(DDPConst.Param_MaxCount) == true)
            {
                this.MaxCount = Convert.ToInt32(parameters[DDPConst.Param_MaxCount].ToString());
            }

            // Դ����
            XmlNode nodeSourceConnection = node.SelectSingleNode("SourceConnection");
            if (nodeSourceConnection != null)
            {
                this.SourceConnEntity = new ConnectionEntity(nodeSourceConnection);
                if (this.SourceConnEntity.Type == DDPConst.Conn_TYPE_ORACLE)
                {
                    this.SourceDataAccessCfg = new DataAccessConfiguration();
                    this.SourceDataAccessCfg.DBType = DataAccessFactory.DBTYPE_ORACLE;
                    this.SourceDataAccessCfg.Parameters["server"] = this.SourceConnEntity.Server;
                    this.SourceDataAccessCfg.Parameters["user"] = this.SourceConnEntity.UserID;
                    this.SourceDataAccessCfg.Parameters["pwd"] = this.SourceConnEntity.Password;
                }
                else if (this.SourceConnEntity.Type == DDPConst.Conn_TYPE_SQLSERVER)
                {
                    this.SourceDataAccessCfg = new DataAccessConfiguration();
                    this.SourceDataAccessCfg.DBType = "SQLSERVER";//DataAccessFactory.DBTYPE_SQL;
                    this.SourceDataAccessCfg.Parameters["server"] = this.SourceConnEntity.Server;
                    this.SourceDataAccessCfg.Parameters["user"] = this.SourceConnEntity.UserID;
                    this.SourceDataAccessCfg.Parameters["pwd"] = this.SourceConnEntity.Password;
                    this.SourceDataAccessCfg.Parameters["database"] = this.SourceConnEntity.Database;
                }
            }

            // Ŀ������
            XmlNode nodeTargetConnection = node.SelectSingleNode("TargetConnection");
            if (nodeTargetConnection != null)
            {
                this.TargetConnEntity = new ConnectionEntity(nodeTargetConnection);
                if (this.TargetConnEntity.Type == DDPConst.Conn_TYPE_ORACLE)
                {
                    this.TargetDataAccessCfg = new DataAccessConfiguration();
                    this.TargetDataAccessCfg.DBType = DataAccessFactory.DBTYPE_ORACLE;
                    this.TargetDataAccessCfg.Parameters["server"] = this.TargetConnEntity.Server;
                    this.TargetDataAccessCfg.Parameters["user"] = this.TargetConnEntity.UserID;
                    this.TargetDataAccessCfg.Parameters["pwd"] = this.TargetConnEntity.Password;
                }
                else if (this.TargetConnEntity.Type == DDPConst.Conn_TYPE_SQLSERVER)
                {
                    this.TargetDataAccessCfg = new DataAccessConfiguration();
                    this.TargetDataAccessCfg.DBType = DataAccessFactory.DBTYPE_ORACLE;
                    this.TargetDataAccessCfg.Parameters["server"] = this.TargetConnEntity.Server;
                    this.TargetDataAccessCfg.Parameters["user"] = this.TargetConnEntity.UserID;
                    this.TargetDataAccessCfg.Parameters["pwd"] = this.TargetConnEntity.Password;
                    this.TargetDataAccessCfg.Parameters["database"] = this.TargetConnEntity.Database;
                }
            }

            // ��ӳ���ϵ,֧�ֶ��
            XmlNodeList tableMapNodeList = node.SelectNodes("TableMap");
            for (int i = 0; i < tableMapNodeList.Count; i++)
            {
                TableMap tableMap = new TableMap(tableMapNodeList[i]);
                this.TableMapList.Add(tableMap);
            }

        }

        /// <summary>
        /// ִ��
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public override ResultCode Execute(ref Hashtable parameters, out string error)
        {
            error = "";

            bool hasDataIDList = false;
            if (parameters.ContainsKey(DDPConst.Param_HasDataIDList) == true)
                hasDataIDList = (bool)parameters[DDPConst.Param_HasDataIDList];
            if (hasDataIDList == true)
            {
                TableMap tableMap = this.TableMapList[0];// Ӧ��ֻ��һ����

                // ��ȡ��Դ����
                List<string> dataIDList = (List<string>)parameters[DDPConst.Param_DataIDList];
                // д��־
                LogManager.Current.WriteCommonLog(this.JobCode, "��������Դ���ݣ���¼��Ϊ" + dataIDList.Count.ToString() + "",this.ThreadName);

                string splitOperator = (string)parameters[DDPConst.Param_SplitOperator];
                DataTable dtSource = this.GetSourceByIDList(tableMap,
                    tableMap.SourceTable.PrimaryKeys,
                    dataIDList,
                    splitOperator);              

                // д��־
                LogManager.Current.WriteCommonLog(this.JobCode, "����ID��õ�ʵ�ʼ�¼��Ϊ" + dtSource.Rows.Count.ToString(), this.ThreadName);

                // ������ʱ���ټ���
                if (dtSource.Rows.Count == 0)
                    return ResultCode.Break;

                // ���������������������
                parameters[DDPConst.Param_PrimaryKeys] = tableMap.SourceTable.PrimaryKeys;
                parameters[DDPConst.Param_DataAccessCfg] = this.SourceDataAccessCfg;

                // ���浽Ŀ��
                this.SaveToTarget(tableMap, dtSource, true);
            }
            else
            {
                for (int i = 0; i < TableMapList.Count; i++)
                {
                    TableMap tableMap = TableMapList[i];
                    tableMap.SourceTable.FileName = BaseCommand.ReplaceParameters(parameters, tableMap.SourceTable.FileName);

                    // ��ȡ��Դ����
                    DataTable dtSource = this.GetSourceData(parameters, tableMap);

                    // ������ʱ���ټ���
                    if (dtSource.Rows.Count == 0)
                        return ResultCode.Break;

                    // ֻȡ��1���������ֵ����ŵ�list
                    if (i == 0 && tableMap.SourceTable.PrimaryKeyList != null && dtSource != null)
                    {
                        List<string> dataIDList = GetDataIDList(dtSource, tableMap.SourceTable.PrimaryKeys, DDPConst.C_ValueSplitOperator);

                        // ���������������������
                        parameters[DDPConst.Param_PrimaryKeys] = tableMap.SourceTable.PrimaryKeys;
                        parameters[DDPConst.Param_DataIDList] = dataIDList;
                        parameters[DDPConst.Param_DataAccessCfg] = this.SourceDataAccessCfg;
                    }

                    // ���浽Ŀ��
                    this.SaveToTarget(tableMap, dtSource, true);

                }
            }

            return ResultCode.Success;
        }



        /// <summary>
        /// ����ID���ϵ�����
        /// </summary>
        /// <param name="dtSource">����table</param>
        /// <param name="fieldNames">�ֶ����������;�ֺŸ���</param>
        /// <param name="paramters"></param>
        public static List<string> GetDataIDList(DataTable dtSource, string fieldNames,string valueSplitOperator)
        {
            string[] fieldList = fieldNames.Split(new char[] {','});

            // ���������ID����
            List<string> dataIDList = new List<string>();
            for (int j = 0; j < dtSource.Rows.Count; j++)
            {
                DataRow row = dtSource.Rows[j];
               
                string keyValues = GetFieldValue(row,fieldList,valueSplitOperator);
                dataIDList.Add(keyValues);
            }

            return dataIDList;
        }

        public string GetFieldValue(DataRow row, string fieldNames, string valueSplitOperator)
        {
            string[] fieldList = fieldNames.Split(new char[] { ',' });
            return GetFieldValue(row, fieldList, valueSplitOperator);
        }

        /// <summary>
        /// fieldNames������","���ָ�
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fieldNames"></param>
        /// <returns></returns>
        public static string GetFieldValue(DataRow row, string[] fieldList, string valueSplitOperator)
        {
            string keyValues = "";

            // ֧�ֶ������
            for (int x = 0; x < fieldList.Length; x++)
            {
                string key = fieldList[x];
                string value = row[key].ToString();
                if (keyValues != "")
                    keyValues += valueSplitOperator;
                keyValues += value;
            }

            return keyValues;
        }


        #endregion

        #region ��ȡ��Դ����

        /// <summary>
        /// �õ���Դ����
        /// </summary>
        /// <param name="tableMap"></param>
        /// <returns></returns>
        public DataTable GetSourceData(Hashtable parameters,TableMap tableMap)
        {
            DataTable dt = null;
            if (this.SourceConnEntity.Type == DDPConst.Conn_TYPE_ORACLE
                || this.SourceConnEntity.Type == DDPConst.Conn_TYPE_SQLSERVER)
            {
                dt = this.GetSourceDataFromDB(parameters,tableMap);
            }
            else
            {
                dt = DatabaseCommand.GetSourceDataFromFile(parameters,tableMap,false,"",0);
            }

            return dt;
        }

        /// <summary>
        /// �����ݿ�õ���Դ����
        /// </summary>
        /// <returns></returns>
        public DataTable GetSourceDataFromDB(Hashtable parameters, TableMap tableMap)
        {
            DataTable dtSource = null;

            DataAccessBroker brokerSource = DataAccessFactory.Instance(this.SourceDataAccessCfg);
            try
            {
                string sqlString = tableMap.GetSourceSelectSQL(this.MaxCount);

                // �滻sql�еĺ����
                sqlString = BaseCommand.ReplaceParameters(this.InitialParameters, sqlString);

                DataSet dataSetSource = brokerSource.FillSQLDataSet(sqlString);

                // Դ����Ϊ��ʱ�������κβ���
                if (dataSetSource == null || dataSetSource.Tables.Count == 0)
                    return null;

                // ȡ��һ����
                dtSource = dataSetSource.Tables[0];
            }
            finally
            {
                brokerSource.Close();
            }

            // д��־
            LogManager.Current.WriteCommonLog(this.JobCode, "�����ݱ�" + tableMap.SourceTable.TableName + "���" + dtSource.Rows.Count.ToString() + "�ʼ�¼��", this.ThreadName);

            return dtSource;
        }

     

        /// <summary>
        /// ���ļ��еõ���Դ����
        /// </summary>
        /// <param name="tableMap"></param>
        /// <returns></returns>
        public static DataTable GetSourceDataFromFile(Hashtable parameters, TableMap tableMap, bool bIgnoreInsert, string strIgnoreFields, int iStartLine)
        {
            List<string> IgnoreFields = new List<string>();

            if (bIgnoreInsert && !string.IsNullOrEmpty(strIgnoreFields))
            {
                string[] IgnoreFieldNameArray = strIgnoreFields.Split(new char[] { ',' });
                for (int i = 0; i < IgnoreFieldNameArray.Length; i++)
                {
                    string field = IgnoreFieldNameArray[i].Trim();
                    if (field != "")
                        IgnoreFields.Add(field);
                }
            }

            string fileName = BaseCommand.ReplaceParameters(parameters, tableMap.SourceTable.FileName);

            // ������ṹ
            string[] fields = tableMap.SourceTable.FieldNames.Split(new char[] { ',' });
            DataTable dtSource = new DataTable();
            for (int i = 0; i < fields.Length; i++)
            {
                string field = fields[i].Trim();
                DataColumn col = new DataColumn(field);
                dtSource.Columns.Add(col);
            }

            if (tableMap.TargetTable.GUIDPrimaryKey != "")
            {
                DataColumn col = new DataColumn(tableMap.TargetTable.GUIDPrimaryKey);
                dtSource.Columns.Add(col);
            }

            int iLine = 0;

            // ��ȡ�ļ��е�����            
            StreamReader sr = new StreamReader(fileName, Encoding.UTF8);//Default);  //this.JobEntity.TableMap.SourceFile
            try
            {
                while (sr.EndOfStream == false)
                {
                    Application.DoEvents();

                    iLine += 1;

                    // ��ȡһ��
                    string line = sr.ReadLine().Trim();

                    if (iLine == iStartLine)
                        continue;

                    if (line == "")
                        continue;

                    // ���ָ�������ֶ�
                    string[] fieldValues = line.Split(new char[] { tableMap.SourceTable.FieldSplitOperator });
                    if (bIgnoreInsert)
                    {
                        if ((fields.Length + IgnoreFields.Count) != fieldValues.Length)
                        {
                            LogManager.Current.WriteCommonLog("��ȡ����", fieldValues.ToString() + "���õ��ֶ�����'" + fields.Length.ToString() + "'���������е��ֶ�����'" + fieldValues.Length.ToString() + "'����һ��", Guid.NewGuid().ToString());
                            continue;
                        }

                    }
                    else
                    {
                        if (fields.Length != fieldValues.Length)
                        {
                            LogManager.Current.WriteCommonLog("��ȡ����", fieldValues.ToString() + "���õ��ֶ�����'" + fields.Length.ToString() + "'���������е��ֶ�����'" + fieldValues.Length.ToString() + "'����һ��", Guid.NewGuid().ToString());
                            continue;
                        }
                    }

                    // �����У��������ֶθ�ֵ
                    DataRow row = dtSource.NewRow();
                    int Ignore = 0;
                    for (int i = 0; i < fieldValues.Length; i++)
                    {
                        bool bIgnore = false;
                        if (bIgnoreInsert)
                        {
                            foreach (string strIgnore in IgnoreFields)
                            {
                                if (strIgnore.Equals(i.ToString()))
                                {
                                    bIgnore = true;
                                    Ignore += 1;
                                    break;
                                }
                            }
                        }

                        if (!bIgnore)
                        {
                            string fieldName = fields[i - Ignore].Trim();
                            string fieldValue = fieldValues[i];

                            if (tableMap.SourceTable.UTCTimeFields.IndexOf(fieldName) != -1)
                            {
                                DateTime leftDate = DateTime.Parse(fieldValue);
                                fieldValue = leftDate.ToString("yyyy-MM-dd HH:mm:ss");
                            }

                            if (tableMap.SourceTable.IntFields.IndexOf(fieldName) != -1)
                            {
                                fieldValue = ((int)decimal.Parse(fieldValue)).ToString();
                            }

                            if (tableMap.SourceTable.ArticleFields.IndexOf(fieldName) != -1)
                            {
                                string[] strArticleNos = fieldValue.Split('-');
                                foreach (string strArticle in strArticleNos)
                                {
                                    if (strArticle.Trim().Length == 6)
                                        fieldValue = strArticle;
                                }

                            }

                            row[fieldName] = fieldValue;
                        }
                    }

                    if (tableMap.TargetTable.GUIDPrimaryKey != "")
                    {
                        row[tableMap.TargetTable.GUIDPrimaryKey] = Guid.NewGuid().ToString();
                    }

                    dtSource.Rows.Add(row);
                }
            }
            finally
            {
                sr.Close();
            }

            // д��־
            //LogManager.Current.WriteCommonLog(jobCode, "���ļ�" + fileName + "���" + dtSource.Rows.Count.ToString() + "�ʼ�¼��", threadName);


            return dtSource;
        }

        /// <summary>
        /// ����Data ID Listȡ����
        /// </summary>
        /// <returns></returns>
        public DataTable GetSourceByIDList(TableMap tableMap,
            string fieldNames, 
            List<string> dataIDList,
            string dataSplitOperator)
        {
            DataTable retTable = new DataTable();

            DataAccessBroker brokerSource = DataAccessFactory.Instance(this.SourceDataAccessCfg);
            try
            {
                for (int x = 0; x < dataIDList.Count;)
                {
                    // ÿ��50��
                    int count = 50;
                    if (x + count > dataIDList.Count)
                        count = dataIDList.Count - x;

                    // ����������orƴ����
                    List<string> tempDataIDList = new List<string>();
                    for (int y = 0; y < count; y++)
                    {
                        tempDataIDList.Add(dataIDList[x+y]);
                    }
                    x += count;

                    // �õ�where���
                    string whereSql = BaseCommand.MakeDataIDWhereSql(fieldNames, tempDataIDList, dataSplitOperator);

                    string sql = "select " + tableMap.SourceTable.FieldNames
                        + " from " + tableMap.SourceTable.TableName
                        + whereSql;

                    DataSet dataSet = brokerSource.FillSQLDataSet(sql);                    
                    if (dataSet == null || dataSet.Tables.Count == 0)
                        continue;// ������һ��50

                    DataTable dtTemp = dataSet.Tables[0];

                    // ��һ��ʱ��ʼ����                   
                    if (retTable.Columns.Count == 0)
                    {
                        for (int x1 = 0; x1 < dtTemp.Columns.Count; x1++)
                        {
                            retTable.Columns.Add(dtTemp.Columns[x1].ColumnName, dtTemp.Columns[x1].DataType);
                        }
                    }

                    // ����ʱ������ݸ��Ƶ����صı���
                    for (int i = 0; i < dtTemp.Rows.Count; i++)
                    {
                        DataRow row = retTable.NewRow();
                        DataRow dtRow = dtTemp.Rows[i];
                        for (int j = 0; j < retTable.Columns.Count; j++)
                        {
                            row[j] = dtRow[j];
                        }
                        retTable.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                brokerSource.Close();
            }

            return retTable;
        }

       #endregion

        #region ���浽Ŀ��

        /// <summary>
        /// ���浽Ŀ��
        /// </summary>
        public void SaveToTarget(TableMap tableMap, DataTable dtSource,bool bMaxCount)
        {
            if (this.TargetConnEntity.Type == DDPConst.Conn_TYPE_TEXT)
            {
                this.SaveToTargetFile(tableMap, dtSource, bMaxCount);
            }
            else
            {
                SaveToTargetDB(tableMap, dtSource);
            }
        }

        /// <summary>
        /// ���浽Ŀ���ļ�
        /// </summary>
        /// <param name="dtSource"></param>
        public void SaveToTargetFile(TableMap tableMap, DataTable dtSource,bool bMaxCount)
        {
            // �Ƚ��ļ����еĲ����滻��
            string targetFileName = BaseCommand.ReplaceParameters(this.InitialParameters, tableMap.TargetTable.FileName);

            // ȷ��Ŀ¼����
            int nIndex = targetFileName.LastIndexOf("\\");
            if (nIndex != -1)
            {
                string dir = targetFileName.Substring(0, nIndex);
                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);
            }

            // �����ݱ��浽Ŀ���ļ�
            StreamWriter sw = null;
            if (String.Compare(tableMap.TargetTable.Encoding, "UTF8", true) == 0)
                sw = new StreamWriter(targetFileName, false, Encoding.UTF8);  //C2-CIָ��utf8
            else
                sw = new StreamWriter(targetFileName, false);
            try
            {

                // ����¼������
                int nCount = dtSource.Rows.Count;
                if (bMaxCount == true)
                {
                    if (this.MaxCount != -1 && nCount > this.MaxCount)
                        nCount = this.MaxCount;
                }

                // �ж��Ƿ��һ����������
                if(tableMap.TargetTable.OneRowRecordsNum == true)
                    sw.WriteLine("#" + nCount.ToString());



                int addFieldCount = 0;
                if (tableMap.SourceTable.PrimaryKeys != "")
                    addFieldCount = tableMap.SourceTable.PrimaryKeyList.Length;

                // ����ÿһ��
                for (int i = 0; i < nCount; i++)
                {
                    DataRow row = dtSource.Rows[i];

                    string line = "";
                    for (int j = 0; j < row.Table.Columns.Count - addFieldCount; j++)  //ע��Ҫ��ȥ���Ѽӵ�������
                    {
                        if (line != "")
                            line += tableMap.TargetTable.FieldSplitOperator;
                        line += row[j].ToString();
                    }
                    sw.WriteLine(line);
                }
            }
            finally
            {
                sw.Close();
            }

            // д��־
            LogManager.Current.WriteCommonLog(this.JobCode, "�ɹ������ݱ��浽Ŀ���ļ�" + targetFileName, this.ThreadName);
        }

        /// <summary>
        /// ���浽Ŀ�����ݿ�
        /// </summary>
        /// <param name="tableMap"></param>
        /// <param name="dtSource"></param>
        private  void SaveToTargetDB(TableMap tableMap, DataTable dtSource)
        {         

            // Ŀ��Ϊ���ݱ�����
            string curProcess = "";
            DataAccessBroker broker = DataAccessFactory.Instance(this.TargetDataAccessCfg);
            try
            {
                // ����ʼ
                curProcess = "����ʼ";
                broker.BeginTransaction();

                DatabaseCommand.SaveToTargetDB(broker, tableMap, dtSource);

                // �����ύ
                curProcess = "�����ύ";
                broker.Commit();
            }
            catch (Exception ex)
            {
                // ����ع�
                broker.RollBack();

                throw ex;
            }
            finally
            {

                broker.Close();
            }

            // д��־
            LogManager.Current.WriteCommonLog(this.JobCode, "�ɹ������ݱ��浽Ŀ���" + tableMap.TargetTable.TableName, this.ThreadName);
        }


        public static void SaveToTargetDB(DataAccessBroker broker,TableMap tableMap, DataTable dtSource)
        {

            // Ŀ��Ϊ���ݱ�����
            string curProcess = "";
            try
            {
                if (tableMap.IsFullData == true)
                {
                    // ���Ŀ�������
                    curProcess = "���Ŀ�������";
                    string deleteSql = tableMap.GetTargetDeleteAllSQL();
                    broker.ExecuteSQL(deleteSql);
                }

                // ��������
                curProcess = "��������";
                // ȷ��TableMap��FieldMap
                tableMap.InitialFieldMapByDataTable(dtSource);
                string insertSql = tableMap.GetTargetInsertSQL(broker.ParameterPrefix.ToString());
                for (int i = 0; i < dtSource.Rows.Count; i++)
                {
                    Application.DoEvents();

                    // ��Ŀ���������¼
                    DataRow row = dtSource.Rows[i];
                    DataAccessParameterCollection dpc = tableMap.GetTargetParameters(row, tableMap.TargetTable.NullFieldNameList);
                    broker.ExecuteSQL(insertSql, dpc);
                }
            }
            catch (Exception ex)
            {
                // д��־
                string description = "�������쳣��Ŀǰִ�е�'" + curProcess + "',��һ��������ع�.";

                // �����׳��쳣
                throw new InvalidOperationException(description + ex.Message);
            }

        }

        #endregion

        public static void Insert<T>(DataAccessBroker broker, List<T> list) where T : EntityBase
        {

            // Ŀ��Ϊ���ݱ�����
            string curProcess = "";
            try
            {
                // ��������
                curProcess = "��������";
                if (list.Count > 0)
                {
                    string sqlString = "INSERT INTO " + list[0].OringTableSchema.TableName + " ( " + ParseInsertSQL(list[0].OringTableSchema.AllColumnInfo, "") + " ) VALUES( " + ParseInsertSQL(list[0].OringTableSchema.AllColumnInfo, ParameterPrefix) + ")";
                    foreach (T entity in list)
                    {
                        Application.DoEvents();

                        // ��Ŀ���������¼
                        DataAccessParameterCollection dpc = new DataAccessParameterCollection();
                        foreach (ColumnInfo field in entity.OringTableSchema.AllColumnInfo)
                        {
                            dpc.AddWithValue(field, entity.GetData(field.ColumnName));
                        }
                        broker.ExecuteSQL(sqlString, dpc);
                    }
                }
            }
            catch (Exception ex)
            {
                // д��־
                string description = "�������쳣��Ŀǰִ�е�'" + curProcess + "',��һ��������ع�.";

                // �����׳��쳣
                throw new InvalidOperationException(description + ex.Message);
            }

        }

        public static void Update<T>(DataAccessBroker broker, List<T> list) where T : EntityBase
        {

            // Ŀ��Ϊ���ݱ�����
            string curProcess = "";
            try
            {
                // ��������
                curProcess = "�޸�����";
                if (list.Count > 0)
                {
                    string sqlString = "UPDATE " + list[0].OringTableSchema.TableName + " SET " + ParseSQL(list[0].OringTableSchema.ValueColumnInfo, ",") + " WHERE " + ParseSQL(list[0].OringTableSchema.KeyColumnInfo, " AND ");
                    foreach (T entity in list)
                    {
                        Application.DoEvents();

                        // ��Ŀ���������¼
                        DataAccessParameterCollection dpc = new DataAccessParameterCollection();
                        foreach (ColumnInfo field in entity.OringTableSchema.AllColumnInfo)
                        {
                            dpc.AddWithValue(field, entity.GetData(field.ColumnName));
                        }
                        broker.ExecuteSQL(sqlString, dpc);
                    }
                }
            }
            catch (Exception ex)
            {
                // д��־
                string description = "�������쳣��Ŀǰִ�е�'" + curProcess + "',��һ��������ع�.";

                // �����׳��쳣
                throw new InvalidOperationException(description + ex.Message);
            }

        }

        public const string ParameterPrefix = "@";

        private static string ParseInsertSQL(List<ColumnInfo> fields, string pre)
        {
            string result = "";
            foreach (ColumnInfo field in fields)
            {
                result += pre + field.ColumnName + ",";
            }
            if (result.Length > 2)
                result = result.Substring(0, result.Length - 1);
            return result;
        }

        private static string ParseSQL(List<ColumnInfo> fields, string sep)
        {
            string result = "";
            foreach (ColumnInfo field in fields)
            {
                result += "\"" + field.ColumnName + "\"=" + ParameterPrefix + field.ColumnName + sep;
            }
            if (result.Length > 2)
                result = result.Substring(0, result.Length - sep.Length);
            return result;
        }
     
    }
}
