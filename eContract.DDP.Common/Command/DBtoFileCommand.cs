using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Suzsoft.Smart.Data;
using System.Collections;
using eContract.DDP.Common.CommonJob;
using System.Data;
using System.IO;
using eContract.Log;

namespace eContract.DDP.Common.Command
{
    public class DBtoFileCommand : BaseCommand
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
        /// ��ӳ���ϵ
        /// </summary>
        TableMap _tableMap =null;

        /// <summary>
        /// ����¼��
        /// </summary>
        public int MaxCount = -1;

        #endregion

        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="node"></param>
        public override void Initialize(Hashtable parameters, XmlNode node)
        {
            // �ȵ����ຯ��
            base.Initialize(parameters, node);


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
                    this.SourceDataAccessCfg.DBType = "SQLSERVER"; //DataAccessFactory.DBTYPE_SQL;
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

            }

            // ��ӳ���ϵ
            XmlNode tableMapNode = node.SelectSingleNode("TableMap");
            this._tableMap = new TableMap(tableMapNode);
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

            // �Ƚ��ļ����еĲ����滻��
            string targetFileName = BaseCommand.ReplaceParameters(this.InitialParameters, this._tableMap.TargetTable.FileName);

            // ȷ��Ŀ¼����
            int nIndex = targetFileName.LastIndexOf("\\");
            if (nIndex != -1)
            {
                string dir = targetFileName.Substring(0, nIndex);
                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);
            }

            int count = 0;

            DataAccessBroker brokerSource = DataAccessFactory.Instance(this.SourceDataAccessCfg);

            // �����ݱ��浽Ŀ���ļ�
            StreamWriter sw = null;
            if (String.Compare(this._tableMap.TargetTable.Encoding, "UTF8", true) == 0)
                sw = new StreamWriter(targetFileName, false, Encoding.UTF8);  //C2-CIָ��utf8
            else
                sw = new StreamWriter(targetFileName, false);
            try
            {
                string sqlString = "";
                sw.BaseStream.Seek(0, SeekOrigin.Begin);

                                
                // �ж��Ƿ��һ������ܼ�¼��
                if (this._tableMap.TargetTable.OneRowRecordsNum == true)
                {
                    string header = "#";
                    header = header.PadRight(10, '#');
                    sw.WriteLine(header);
                }

                // ����
                sqlString = this._tableMap.GetSourceSelectSQL(this.MaxCount);
                IDataReader dataReader = brokerSource.ExecuteSQLReader(sqlString);
                while (dataReader.Read())
                {
                    string line = "";
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        if (line != "")
                            line += this._tableMap.TargetTable.FieldSplitOperator;
                        line += dataReader[i].ToString();
                    }
                    sw.WriteLine(line);

                    count++;
                }
                sw.Flush();


                // �ж��Ƿ��һ������ܼ�¼��
                if (this._tableMap.TargetTable.OneRowRecordsNum == true)
                {
                    sw.BaseStream.Seek(0, SeekOrigin.Begin);
                    string header = "#" + count.ToString();
                    sw.WriteLine(header.PadRight(10),' ');
                    sw.Flush();
                }


            }
            finally
            {
                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                }

                brokerSource.Close();

            }

            // д��־
            LogManager.Current.WriteCommonLog(this.JobCode, "�����ݱ�" + this._tableMap.SourceTable.TableName + "���" + count.ToString() + "�ʼ�¼��", this.ThreadName);


            return ResultCode.Success;
        }

    }
}
