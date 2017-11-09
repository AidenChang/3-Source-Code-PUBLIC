using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.Data;
using Suzsoft.Smart.Data;
using eContract.DDP.Common.CommonJob;
using System.IO;
using System.Windows.Forms;
using Suzsoft.Smart.EntityCore;
using System.Diagnostics;
using eContract.Log;

namespace eContract.DDP.Common.Command
{
    public class DBHeaderDetailCommand : DatabaseCommand
    {
        public TableMap HeaderTableMap = null;
        public TableMap DetailTableMap = null;


        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="appDir"></param>
        /// <param name="node"></param>
        public override void Initialize(Hashtable parameters, XmlNode node)
        {
            base.Initialize(parameters, node);


            // �ֳ�Header��Detail��
            for (int i = 0; i < this.TableMapList.Count; i++)
            {
                TableMap tableMap = this.TableMapList[i];
                if (tableMap.Name == "Header")
                    this.HeaderTableMap = tableMap;
                else if (tableMap.Name == "Detail")
                    this.DetailTableMap = tableMap;
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

            // �滻�ļ���
            string dataSplitOperator = DDPConst.C_ValueSplitOperator;

            List<string> dataIDList = null;

            DataTable dtHeader = null;

            bool hasDataIDList = false;
            if (parameters.ContainsKey(DDPConst.Param_HasDataIDList) == true)
                hasDataIDList = (bool)parameters[DDPConst.Param_HasDataIDList];
            if (hasDataIDList == true)
            {
                // ��ȡ��Դ����
                dataIDList = (List<string>)parameters[DDPConst.Param_DataIDList];
                // д��־
                LogManager.Current.WriteCommonLog(this.JobCode, "������header��Դ���ݣ���¼��Ϊ" + dataIDList.Count.ToString() + "", this.ThreadName);

                dataSplitOperator = (string)parameters[DDPConst.Param_SplitOperator];
                dtHeader = this.GetSourceByIDList(this.HeaderTableMap,
                    this.HeaderTableMap.SourceTable.PrimaryKeys,
                    dataIDList,
                    dataSplitOperator);

                // д��־
                LogManager.Current.WriteCommonLog(this.JobCode, "����ID���header��ʵ�ʼ�¼��Ϊ" + dtHeader.Rows.Count.ToString(), this.ThreadName);


            }
            else
            {
                // �õ�Header��
                dtHeader = this.GetSourceData(parameters, this.HeaderTableMap);
            }

            
            // ���û��header���ݣ��򲻽��к����Ĵ������������Ҳ������ִ�У�
            if (dtHeader == null || dtHeader.Rows.Count == 0)
                return ResultCode.Break;
            

            // ȡheader�������ֵ����ŵ�list���Ա�����ĸ��±�־������ʹ��
            if (this.HeaderTableMap.SourceTable.PrimaryKeyList != null && dtHeader != null)
            {
                // ���������������������
                parameters[DDPConst.Param_PrimaryKeys] = this.HeaderTableMap.SourceTable.PrimaryKeys;
                if (hasDataIDList == false)
                {
                    dataIDList = GetDataIDList(dtHeader, this.HeaderTableMap.SourceTable.PrimaryKeys, DDPConst.C_ValueSplitOperator);
                    parameters[DDPConst.Param_DataIDList] = dataIDList;
                }

                if (this.SourceDataAccessCfg != null)
                    parameters[DDPConst.Param_DataAccessCfg] = this.SourceDataAccessCfg;
                else
                    parameters[DDPConst.Param_DataAccessCfg] = this.TargetDataAccessCfg;
            }

            // �õ�Detail�������޼�¼
            DataTable dtDetail = this.GetDetailData(parameters, dtHeader, dataIDList, dataSplitOperator);

            // д��־
            LogManager.Current.WriteCommonLog(this.JobCode, "����header��õ�detail��¼��Ϊ" + dtDetail.Rows.Count.ToString(), this.ThreadName);


            if (this.TargetConnEntity.Type == DDPConst.Conn_TYPE_TEXT)
            {
                // ���浽Ŀ��
                this.SaveToTargetFile(this.HeaderTableMap, dtHeader, true);
                this.SaveToTargetFile(this.DetailTableMap, dtDetail, false);
            }
            else
            {
                this.SaveHeaderDetailToTarget(ref parameters, dtHeader, dtDetail, dataSplitOperator);
            }

            return ResultCode.Success;
        }

        /// <summary>
        /// ����header��detail������,һ��header���Ӧ�ļ���detail��Ϊһ������
        /// </summary>
        /// <param name="dtHeader"></param>
        /// <param name="dtDetail"></param>
        private void SaveHeaderDetailToTarget(ref Hashtable parameters, DataTable dtHeader, DataTable dtDetail, string dataSplitOperator)
        {
            List<string> dataIDList = (List<string>)parameters[DDPConst.Param_DataIDList];
            int nError = 0;
            int nSuccess = 0;

            DataAccessBroker brokerTarget = DataAccessFactory.Instance(this.TargetDataAccessCfg);
            try
            {
                // header sql
                this.HeaderTableMap.InitialFieldMapByDataTable(dtHeader);
                string insertSqlHeader = this.HeaderTableMap.GetTargetInsertSQL(brokerTarget.ParameterPrefix.ToString());

                // detail sql
                if (dtDetail.Rows.Count > 0)
                    this.DetailTableMap.InitialFieldMapByDataTable(dtDetail.Rows[0]);
                string insertSqlDetail = this.DetailTableMap.GetTargetInsertSQL(brokerTarget.ParameterPrefix.ToString());

                for (int i = 0; i < dtHeader.Rows.Count; i++)
                {
                    Application.DoEvents();

                    string recordsCondition = "";
                    // һ��header
                    DataRow rowHeader = dtHeader.Rows[i];

                    // һ�������ļ�¼һ�����񣬰���header��detail
                    brokerTarget.BeginTransaction();
                    try
                    {

                        // ���������ֶμ�ֵ���������detail
                        string refFieldSql = "";
                        string refValueSql = "";
                        string[] refFieldList = this.DetailTableMap.SourceTable.RefFields.Split(new char[] { ',' });
                        for (int x = 0; x < refFieldList.Length; x++)
                        {
                            string refField = refFieldList[x];
                            if (refFieldSql != "")
                                refFieldSql += "+'|'+";
                            refFieldSql += refField;

                            if (refValueSql != "")
                                refValueSql += "|";
                            refValueSql += rowHeader[refField].ToString();
                        }
                        recordsCondition = refFieldSql + "='" + refValueSql + "'";

                        // �ҵ�Detail����
                        DataRow[] detailRowList = dtDetail.Select(recordsCondition);
                        LogManager.Current.WriteCommonLog(this.JobCode, refValueSql + "��Ӧ" + detailRowList.Length.ToString() + "��detail��¼��", this.ThreadName);


                        // ��header���뵽���ݿ�
                        DataAccessParameterCollection dpcHeader = this.HeaderTableMap.GetTargetParameters(rowHeader, this.HeaderTableMap.TargetTable.NullFieldNameList);
                        brokerTarget.ExecuteSQL(insertSqlHeader, dpcHeader);

                        // ��detail���뵽���ݿ�    

                        for (int j = 0; j < detailRowList.Length; j++)
                        {
                            // detail
                            DataRow rowDetail = detailRowList[j];
                            DataAccessParameterCollection dpcDetail = this.DetailTableMap.GetTargetParameters(rowDetail, this.DetailTableMap.TargetTable.NullFieldNameList);
                            brokerTarget.ExecuteSQL(insertSqlDetail, dpcDetail);
                        }

                        // �����ύ
                        brokerTarget.Commit();

                        LogManager.Current.WriteCommonLog(this.JobCode, recordsCondition + "��¼�������ݿ�ɹ���", this.ThreadName);
                        nSuccess++;
                    }
                    catch (Exception ex)
                    {
                        // ����ع�
                        brokerTarget.RollBack();

                        LogManager.Current.WriteErrorLog(this.JobCode, recordsCondition + "��¼�������ݿ����:" + ex.Message, this.ThreadName);

                        // ��DataIDList���Ƴ��˱ʼ�¼�������Ͳ����������ʱ����
                        string dataID = this.GetFieldValue(rowHeader, this.HeaderTableMap.SourceTable.PrimaryKeys, dataSplitOperator);
                        dataIDList.Remove(dataID);

                        nError++;

                        // ��������ļ�¼
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                brokerTarget.Close();
            }

            // д��־
            string errorInfo = "��Դ��" + dtHeader.Rows.Count.ToString() + "�ʼ�¼��" + nSuccess.ToString() + "�ʱ���ɹ���" + nError.ToString() + "�ʱ���ʧ�ܡ�";
            LogManager.Current.WriteCommonLog(this.JobCode, errorInfo, this.ThreadName);

            // ���浽�������������
            parameters[DDPConst.Param_Info] = errorInfo;

            // ������ļ�¼�������ڲ�����
            if (nError > 0)
                parameters[DDPConst.Param_ErrorCount] = nError;

        }

        /// <summary>
        /// �����ݿ�õ���Դ����
        /// </summary>
        /// <returns></returns>
        public DataTable GetDetailData(Hashtable parameters, DataTable dtHeader, List<string> dataIDList, string dataSplitOperator)
        {
            if (this.SourceConnEntity.Type == DDPConst.Conn_TYPE_ORACLE || this.SourceConnEntity.Type == DDPConst.Conn_TYPE_SQLSERVER)
            {
                // �õ���Ӧ��ID�б�
                if (dataIDList == null)
                    dataIDList = GetDataIDList(dtHeader, this.DetailTableMap.SourceTable.RefFields, dataSplitOperator);
                if (dataIDList.Count == 0) // û�ж�Ӧ�Ĺ�������
                    return null;
                return this.GetSourceByIDList(this.DetailTableMap, this.DetailTableMap.SourceTable.RefFields, dataIDList, dataSplitOperator);
            }
            else
            {
                return this.GetDetailDataFromFile(parameters,this.DetailTableMap, dtHeader);
            }            
        }

        /// <summary>
        /// ���ļ��õ�detail
        /// </summary>
        /// <param name="tableMap"></param>
        /// <param name="dtHeader"></param>
        /// <returns></returns>
        public DataTable GetDetailDataFromFile(Hashtable parameters,TableMap tableMap, DataTable dtHeader)
        {
            string fileName = BaseCommand.ReplaceParameters(parameters, tableMap.SourceTable.FileName);


            DataTable dtDetail = new DataTable(); ;

            if (dtHeader == null)
                return null;

            string[] refFieldList = tableMap.SourceTable.RefFields.Split(new char[] { ',' });

            // ƴ����������
            List<string> refDataIDList = GetDataIDList(dtHeader, tableMap.SourceTable.RefFields, DDPConst.C_ValueSplitOperator);
            if (refDataIDList.Count == 0) // û�ж�Ӧ�Ĺ�������
                return null;

            // ������ṹ
            string[] fields = tableMap.SourceTable.FieldNames.Split(new char[] { ',' });
            for (int i = 0; i < fields.Length; i++)
            {
                string field = fields[i].Trim();
                DataColumn col = new DataColumn(field);
                dtDetail.Columns.Add(col);
            }

            // ��ȡ�ļ��е�����            
            StreamReader sr = new StreamReader(fileName, Encoding.Default);  //this.JobEntity.TableMap.SourceFile
            try
            {
                int nCount = 0;
                while (sr.EndOfStream == false)
                {
                    Application.DoEvents();

                    // ��ȡһ��
                    string line = sr.ReadLine().Trim();
                    if (line == "")
                        continue;

                    // ���ָ�������ֶ�
                    string[] fieldValues = line.Split(new char[] {tableMap.SourceTable.FieldSplitOperator });

                    // �����У��������ֶθ�ֵ
                    DataRow row = dtDetail.NewRow();
                    for (int i = 0; i < fieldValues.Length && i < dtDetail.Columns.Count; i++)
                    {
                        string fieldName = fields[i].Trim();
                        string fieldValue = fieldValues[i];
                        row[fieldName] = fieldValue;
                    }

                    // ֧�ֶ������
                    string keyValues = "";
                    for (int x = 0; x < refFieldList.Length; x++)
                    {
                        string key = refFieldList[x];
                        string value = row[key].ToString();
                        if (keyValues != "")
                            keyValues += DDPConst.C_ValueSplitOperator;
                        keyValues += value;
                    }

                    if (refDataIDList.IndexOf(keyValues) == -1)
                    {
                        LogManager.Current.WriteCommonLog(this.JobCode, keyValues + "δ�ҵ�ƥ��ĵ�header��¼��",this.ThreadName);
                    }
                    else
                    {
                        dtDetail.Rows.Add(row);
                    }
                }
            }
            finally
            {
                sr.Close();
            }

            return dtDetail;
        }

    }
}
