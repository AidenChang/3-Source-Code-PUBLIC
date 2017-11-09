
~~~~~~~~~~
CMS������Ϣ

CMS =
  (DESCRIPTION =
    (ADDRESS_LIST =
      (ADDRESS = (PROTOCOL = TCP)(HOST = 10.0.2.193)(PORT = 1521))
    )
    (CONNECT_DATA =
      (SID = CMS)
    )
  )
 

~~~~~~~~~~~~~~~
frmTableMap.cs

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using eContract.DDP.Common;
using Suzsoft.Smart.Data;
using System.Diagnostics;
using eContract.DDP.Server;

namespace eContract.DDP.UI
{
    public partial class frmTableMap : Form
    {
        public JobEntity _jobEntity = null;

        /// <summary>
        /// Դ���ö���
        /// </summary>
        protected DataAccessConfiguration SourceConfig = null;

        /// <summary>
        /// Ŀ�����ö���
        /// </summary>
        protected DataAccessConfiguration TargetConfig = null;

        private JobCfgManager _jobCfgManager = null;


        public frmTableMap()
        {
            InitializeComponent();

        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        public bool Initial(JobCfgManager jobCfgManager, JobEntity jobEntity,out string error)
        {
            error = "";
            try
            {
                //
                this._jobCfgManager = jobCfgManager;

                this._jobEntity = jobEntity;

                // Դ���ö���
                Debug.Assert(this._jobEntity.SourceConnection != null, "��Դ�����ַ���������Ϊ��.");
                this.SourceConfig = new DataAccessConfiguration();
                this.SourceConfig.DBType = DataAccessFactory.DBTYPE_ORACLE;
                this.SourceConfig.Parameters["server"] = this._jobEntity.SourceConnection.Server;
                this.SourceConfig.Parameters["user"] = this._jobEntity.SourceConnection.UserID;
                this.SourceConfig.Parameters["pwd"] = this._jobEntity.SourceConnection.Password;

                // Ŀ�����ö���
                Debug.Assert(this._jobEntity.TargetConnection != null, "Ŀ�������ַ���������Ϊ��.");
                this.TargetConfig = new DataAccessConfiguration();
                this.TargetConfig.DBType = DataAccessFactory.DBTYPE_ORACLE;
                this.TargetConfig.Parameters["server"] = this._jobEntity.TargetConnection.Server;
                this.TargetConfig.Parameters["user"] = this._jobEntity.TargetConnection.UserID;
                this.TargetConfig.Parameters["pwd"] = this._jobEntity.TargetConnection.Password;




                // �Ƿ���ȫ������
                if (this._jobEntity.TableMap != null)
                {
                    if (this._jobEntity.TableMap.IsFullData == true)
                        this.cbIsFullData.Checked = true;
                    else
                        this.cbIsFullData.Checked = false;
                }

                // ~~~~~~~~~
                // �ҵ���Դ������
                this.cbSourceTable.Items.Clear();
                if (this._jobEntity.SourceConnection.Type == ConnectionEntity.TYPE_TEXT)
                {
                    // ��Ϊ������״̬
                    this.cbSourceTable.Enabled = false;
                    this.txtSourceKey.Enabled = false;
                    this.lvSourceField.Enabled = false;
                }
                else
                {
                    // ��Ϊ����״̬
                    this.cbSourceTable.Enabled = true;
                    this.txtSourceKey.Enabled = true;
                    this.lvSourceField.Enabled = true;

                    // ���Table�б�
                    List<string> sourceTableNameList = this.GetSourceTables();
                    for (int i = 0; i < sourceTableNameList.Count; i++)
                    {
                        string tableName = sourceTableNameList[i];
                        this.cbSourceTable.Items.Add(tableName);
                    }

                    // ѡ�ж�Ӧ��Table
                    if (this._jobEntity.TableMap != null)
                        this.cbSourceTable.SelectedItem = this._jobEntity.TableMap.SourceTable;
                    else
                        this.cbSourceTable.SelectedIndex = 0;

                    // key
                    if (this._jobEntity.TableMap != null)
                        this.txtSourceKey.Text = this._jobEntity.TableMap.SourcePrimaryKey;
                }

                //~~~~~~~~~~~
                // �ҵ�Ŀ�������
                this.cbTargetTable.Items.Clear();
                if (this._jobEntity.TargetConnection.Type == ConnectionEntity.TYPE_TEXT)
                {
                    // ��Ϊ������״̬
                    this.cbTargetTable.Enabled = false;
                    this.txtTargetKey.Enabled = false;
                    this.lvTargetField.Enabled = false;
                }
                else
                {
                    // ��Ϊ����״̬
                    this.cbTargetTable.Enabled = true;
                    this.txtTargetKey.Enabled = true;
                    this.lvTargetField.Enabled = true;

                    // ���Table�б�
                    List<string> targetTableNameList = this.GetTargetTables();
                    for (int i = 0; i < targetTableNameList.Count; i++)
                    {
                        string tableName = targetTableNameList[i];
                        this.cbTargetTable.Items.Add(tableName);
                    }

                    // ѡ�ж�Ӧ��Table
                    if (this._jobEntity.TableMap != null)
                        this.cbTargetTable.SelectedItem = this._jobEntity.TableMap.TargetTable;
                    else
                        this.cbTargetTable.SelectedIndex = 0;

                    // key
                    if (this._jobEntity.TableMap != null)
                        this.txtTargetKey.Text = this._jobEntity.TableMap.TargetPrimaryKey;
                }


                // ��ʾ�ֶ���ͬ
                if (this._jobEntity.TableMap != null && this._jobEntity.TableMap.FieldMapList.Count == 0)
                    this.checkBox_fieldSame.Checked = true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false ;
            }

            return true;
        }

        #region ��ͨ�ú������Ժ��Ƶ��������ݿ���

        /// <summary>
        /// �õ���Դ������
        /// </summary>
        /// <returns></returns>
        private List<string> GetSourceTables()
        {
            List<string> tableList = new List<string>();

            using (DataAccessBroker broker = DataAccessFactory.Instance(this.SourceConfig))
            {
                // �ҵ���Դ���еı�
                string tableSqlString = string.Format("select TABLE_NAME from all_all_tables where owner=upper('{0}')", this._jobEntity.SourceConnection.UserID);
                DataSet dsTable = broker.FillSQLDataSet(tableSqlString);
                if (dsTable != null && dsTable.Tables.Count > 0)
                {
                    DataTable dtTable = dsTable.Tables[0];
                    for (int i = 0; i < dtTable.Rows.Count; i++)
                    {
                        DataRow row = dtTable.Rows[i];
                        tableList.Add(row[0].ToString());
                    }
                }

                // �ҵ���Դ���е���ͼ
                string viewSqlString = "select view_name from user_views";
                DataSet dsView = broker.FillSQLDataSet(viewSqlString);
                if (dsView != null && dsView.Tables.Count > 0)
                {
                    DataTable dtView = dsView.Tables[0];
                    for (int i = 0; i < dtView.Rows.Count; i++)
                    {
                        DataRow row = dtView.Rows[i];
                        tableList.Add(row[0].ToString());
                    }
                }
            }

            return tableList;
        }

        /// <summary>
        /// �õ�Ŀ�������
        /// </summary>
        /// <returns></returns>
        private List<string> GetTargetTables()
        {
            List<string> tableList = new List<string>();

            using (DataAccessBroker broker = DataAccessFactory.Instance(this.TargetConfig))
            {
                // �ҵ���Դ���еı�
                string tableSqlString = string.Format("select TABLE_NAME from all_all_tables where owner=upper('{0}')", this._jobEntity.TargetConnection.UserID);
                DataSet dsTable = broker.FillSQLDataSet(tableSqlString);
                if (dsTable != null && dsTable.Tables.Count > 0)
                {
                    DataTable dtTable = dsTable.Tables[0];
                    for (int i = 0; i < dtTable.Rows.Count; i++)
                    {
                        DataRow row = dtTable.Rows[i];
                        tableList.Add(row[0].ToString());
                    }
                }

                // �ҵ���Դ���е���ͼ
                string viewSqlString = "select view_name from user_views";
                DataSet dsView = broker.FillSQLDataSet(viewSqlString);
                if (dsView != null && dsView.Tables.Count > 0)
                {
                    DataTable dtView = dsView.Tables[0];
                    for (int i = 0; i < dtView.Rows.Count; i++)
                    {
                        DataRow row = dtView.Rows[i];
                        tableList.Add(row[0].ToString());
                    }
                }
            }

            return tableList;
        }

        /// <summary>
        /// �õ����ֶ�
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="bSourceTable"></param>
        /// <returns></returns>
        private List<FieldEntity> GetTableFields(DataAccessConfiguration connectionConfig,string tableName)
        {
            List<FieldEntity> fieldList = new List<FieldEntity>();

            using(DataAccessBroker broker = DataAccessFactory.Instance(connectionConfig))
            {
                DataTable dtSchema = broker.GetSchema(tableName);

                for (int i = 0; i < dtSchema.Rows.Count; i++)
                {
                    DataRow row = dtSchema.Rows[i];

                    string name = row["ColumnName"].ToString().Trim();
                    string type = row["DataType"].ToString().Trim();
                    int nIndex = type.LastIndexOf('.');
                    if (nIndex > 0)
                        type = type.Substring(nIndex + 1);
                    string allowNull = row["AllowDBNull"].ToString().Trim();

                    FieldEntity entity = new FieldEntity(name, type, allowNull);
                    fieldList.Add(entity);
               }
            }

            return fieldList;
        }


        /// <summary>
        /// �õ�������
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="bSourceTable"></param>
        /// <returns></returns>
        private string GetTableKeyField(DataAccessConfiguration connectionConfig, string tableName)
        {
            string keyFields = "";
            using(DataAccessBroker broker = DataAccessFactory.Instance(connectionConfig))
            {
                string sqlString = "select col.column_name "
                    + " from user_constraints con,  user_cons_columns col "
                    + " where con.constraint_name = col.constraint_name "
                    + " and con.constraint_type='P' and col.table_name = '" + tableName + "'";

                DataSet ds = broker.FillSQLDataSet(sqlString);
                if (ds == null || ds.Tables.Count == 0)
                    return "";

                DataTable dt = ds.Tables[0];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    if (keyFields != "")
                        keyFields += ",";
                    keyFields += row[0].ToString().Trim();
                }                
            }

            return keyFields;
            
        }

        #endregion


        private void UpdateTableMap()
        {
            if (this._jobEntity.TableMap == null)
                this._jobEntity.TableMap = new TableMap();

            this._jobEntity.TableMap.SourceTable = this.cbSourceTable.Text;
            this._jobEntity.TableMap.SourcePrimaryKey = this.txtSourceKey.Text.Trim();

            this._jobEntity.TableMap.TargetTable = this.cbTargetTable.Text;
            this._jobEntity.TableMap.TargetPrimaryKey = this.txtTargetKey.Text.Trim();

            // �����ԭ��������
            this._jobEntity.TableMap.FieldMapList.Clear();

            if (this.checkBox_fieldSame.Checked == false)
            {
                for (int i = 0; i < this.lvTargetField.Items.Count; i++)
                {
                    ListViewItem item = this.lvTargetField.Items[i];
                    string sourceField = item.Text;
                    if (sourceField == "")
                        continue;

                    string targetField = item.SubItems[1].Text;

                    FieldMap fieldMap = new FieldMap(sourceField, targetField);
                    this._jobEntity.TableMap.FieldMapList.Add(fieldMap);
                }
            }
        
        }



        private void btnOK_Click(object sender, EventArgs e)
        {
            //
            this.UpdateTableMap();

            // ���µ��ļ�
            this._jobCfgManager.EditJob(this._jobEntity);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// ��Դ��ѡ����仯���ֶ��б���ű仯
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSourceTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            string tableName = this.cbSourceTable.Text.Trim();

            this.txtSourceKey.Text = this.GetTableKeyField(this.SourceConfig, tableName);

            // ���ݱ����ֶ�
            List<FieldEntity> fieldList = this.GetTableFields(this.SourceConfig, tableName);


            this.lvSourceField.Items.Clear();
            for (int i = 0; i < fieldList.Count; i++)
            {
                FieldEntity field = fieldList[i];

                ListViewItem item = new ListViewItem(field.Name);
                item.SubItems.Add(field.Type);
                item.SubItems.Add(field.AllowNull);

                this.lvSourceField.Items.Add(item);
            }

        }



        /// <summary>
        /// Ŀ���ѡ����仯���ֶ��б���ű仯
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTargetTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            string tableName = this.cbTargetTable.Text.Trim();

            // ������
            this.txtTargetKey.Text = this.GetTableKeyField(this.TargetConfig,tableName);


            // ���ݱ����ֶ�
            List<FieldEntity> fieldList = this.GetTableFields(this.TargetConfig, tableName);

            this.lvTargetField.Items.Clear();
            for (int i = 0; i < fieldList.Count; i++)
            {
                FieldEntity field = fieldList[i];

                ListViewItem item = new ListViewItem();
                item.ForeColor = Color.Black;

                //ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem(item, "", Color.Red, item.BackColor, item.Font);
                item.SubItems[0] = new ListViewItem.ListViewSubItem(item,"");//subItem;

                ListViewItem.ListViewSubItem subItem2 = new ListViewItem.ListViewSubItem(item, field.Name, Color.Black, item.BackColor, item.Font);
                item.SubItems.Add(subItem2);//field.Name);
                item.SubItems.Add(field.Type);
                item.SubItems.Add(field.AllowNull);                

                this.lvTargetField.Items.Add(item);
            }
        }

        public bool _isClickSource = false;
        public string _sourceFieldName = "";
        private void lvSourceField_Click(object sender, EventArgs e)
        {
            if (this.lvSourceField.SelectedItems.Count == 0)
                return;

            ListViewItem item = this.lvSourceField.SelectedItems[0];
            this._sourceFieldName = item.Text;
            this._isClickSource = true;

        }

        private void lvTargetField_Click(object sender, EventArgs e)
        {
            // ������Դʱ
            if (this._isClickSource == false || this._sourceFieldName == "")
                return;

            if (this.lvTargetField.SelectedItems.Count == 0)
                return;

            ListViewItem item = this.lvTargetField.SelectedItems[0];
            item.Text = this._sourceFieldName;

            // ���Դ
            this._sourceFieldName = "";
            this._isClickSource = false;

        }

        private void checkBox_fieldSame_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_fieldSame.Checked == true)
            {
                this.lvSourceField.Enabled = false;
                this.lvTargetField.Enabled = false;
            }
            else
            {
                if (this._jobEntity.SourceConnection.Type == ConnectionEntity.TYPE_ORACLE)
                    this.lvSourceField.Enabled = true;

                if (this._jobEntity.TargetConnection.Type == ConnectionEntity.TYPE_ORACLE)
                    this.lvTargetField.Enabled = true;
            }
        }


    }

    public class FieldEntity
    { 
        public string Name = "";
        public string Type = "";
        public string AllowNull = "";

        public FieldEntity(string name, string type, string allowNull)
        {
            this.Name = name;
            this.Type = type;
            this.AllowNull = allowNull;
        }

    }
}  
