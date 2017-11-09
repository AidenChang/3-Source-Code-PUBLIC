using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;
using Suzsoft.Smart.EntityCore;
using System.Data.OracleClient;
using System.Collections;
using System.Data.SqlClient;

namespace Suzsoft.Smart.Data
{
    public delegate void DbExceptionHandler(DataAccessBroker sender, Exception ex);

    /// <summary>
    /// ���ݷ���Broker����
    /// </summary>
    public abstract class DataAccessBroker : IDisposable
    {
        protected DataAccessBroker()
        {
            //Initialize();
        }

        #region attributes&commom method
        private bool _disposed = false;//��Դ�ͷű�־
        bool _inTransaction;//�Ƿ���������

        protected DbConnection _connection;
        /// <summary>
        /// ���ݿ�����
        /// </summary>
        public DbConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        protected DbTransaction _transaction;
        /// <summary>
        /// ���ݿ�����
        /// </summary>
        protected DbTransaction Transaction
        {
            get
            {
                return _transaction;
            }
        }

        DataAccessConfiguration _configuration;
        /// <summary>
        /// ���ݷ�������
        /// </summary>
        public DataAccessConfiguration Configuration
        {
            get
            {
                return _configuration;
            }
            set
            {
                _configuration = value;
            }

        }

        public static event DbExceptionHandler DBException;
        /// <summary>
        /// �������ݿ��쳣
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        public void OnDBException(DataAccessBroker sender, Exception ex)
        {
            if (DBException != null)
            {
                DBException(sender, ex);
            }
        }
        #endregion

        #region Abstract Method
        /// <summary>
        /// �����ഴ����Ӧ��DBConnection����
        /// </summary>
        internal abstract void Create();

        /// <summary>
        /// �����Ӧ���ݿ�Ĳ���ǰ׺
        /// jane.ren 2007/10/19 ��Ϊpublic,ԭΪprotected
        /// </summary>
        public abstract string ParameterPrefix
        {
            get;
        }

        /// <summary>
        /// �����ഴ��Command
        /// </summary>
        /// <param name="commandString"></param>
        /// <returns></returns>
        protected abstract DbCommand CreateCommand(string commandString);

        /// <summary>
        /// �����ഴ��Parameter
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        protected abstract void AddParameter(DbCommand command, DataAccessParameter parameter);

        /// <summary>
        /// ������������ӦAdapterִ��command������Dataset
        /// </summary>
        /// <param name="command"></param>
        /// <param name="mapping"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract int ExecuteDataSet(DbCommand command, DataTableMapping mapping, ref DataSet result);
        #endregion

        #region NonQuery
        DbCommand _command;
        public void CreateCommandE(string executeString)
        {
            if (_command != null)
                _command.Dispose();
            _command = CreateCommand(executeString);
            _command.CommandType = CommandType.Text;
        }

        public int ExecuteNonQueryE(EntityBase entity)//DataAccessParameterCollection parameters)
        {
            if (_inTransaction)//���ʹ���������commandӦ��ʹ������
            {
                _command.Transaction = _transaction;
            }

            OracleCommand comm = _command as OracleCommand;
            foreach (ColumnInfo field in entity.OringTableSchema.AllColumnInfo)
            {
                comm.Parameters.AddWithValue(ParameterPrefix + field.ColumnName, entity.GetData(field.ColumnName));
                comm.Parameters[ParameterPrefix + field.ColumnName].Direction = ParameterDirection.Input;
            }
            int iReturn = _command.ExecuteNonQuery();
            _command.Parameters.Clear();
            return iReturn;
        }

        public int ExecuteNonQueryE(DataRow row, string sqlstring, Dictionary<string, string> serverclientdic)
        {
            if (_inTransaction)//���ʹ���������commandӦ��ʹ������
            {
                _command.Transaction = _transaction;
            }
            OracleCommand comm = _command as OracleCommand;
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                if (serverclientdic.ContainsKey(row.Table.Columns[i].ColumnName))   //paralst.Contains(ParameterPrefix + row.Table.Columns[i].ColumnName))
                {
                    comm.Parameters.AddWithValue(ParameterPrefix + serverclientdic[row.Table.Columns[i].ColumnName], row[i]);
                    comm.Parameters[ParameterPrefix + serverclientdic[row.Table.Columns[i].ColumnName]].Direction = ParameterDirection.Input;
                }
            }
            int iReturn = _command.ExecuteNonQuery();
            _command.Parameters.Clear();
            return iReturn;
        }

        public int ExecuteNonQuery(string executeString, DataAccessParameterCollection parameters, CommandType cmdType)
        {
            DbCommand command = CreateCommand(executeString);
            command.CommandType = cmdType;
            command.CommandTimeout = 0;
            PrepareCommand(command, parameters);
            int iReturn = command.ExecuteNonQuery();
            command.Parameters.Clear();
            command.Dispose();
            return iReturn;
        }

        public int ExecuteNonQuery(WhereBuilder wb)
        {
            return ExecuteNonQuery(wb.SQLString, wb.Parameters, CommandType.Text);
        }

        /// <summary>
        /// ִ�д洢����-�޲���
        /// </summary>
        /// <param name="commandString"></param>
        /// <returns></returns>
        public int ExecuteCommand(string commandString)
        {
            return ExecuteCommand(commandString, null);
        }

        /// <summary>
        /// ִ�д洢����-�в���
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteCommand(string commandString, DataAccessParameterCollection parameters)
        {
            return ExecuteNonQuery(commandString, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// ִ��SQL���-�޲���
        /// </summary>
        /// <param name="sqlString"></param>
        /// <returns></returns>
        public int ExecuteSQL(string sqlString)
        {
            return ExecuteSQL(sqlString, null);
        }

        /// <summary>
        /// ִ��SQL���-�в���
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteSQL(string sqlString, DataAccessParameterCollection parameters)
        {
            return ExecuteNonQuery(sqlString, parameters, CommandType.Text);
        }
        #endregion

        #region Scalar
        /// <summary>
        /// ִ�д洢���̲�����ִ�н��-�޲���
        /// </summary>
        /// <param name="commandString"></param>
        /// <returns></returns>
        public object ExecuteCommandScalar(string commandString)
        {
            return ExecuteScalar(commandString, null, CommandType.StoredProcedure);
        }

        /// <summary>
        /// ִ�д洢���̲�����ִ�н��-�в���
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteCommandScalar(string commandString, DataAccessParameterCollection parameters)
        {
            return ExecuteScalar(commandString, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// ִ��SQL��䲢����ִ�н��-�޲���
        /// </summary>
        /// <param name="sqlString"></param>
        /// <returns></returns>
        public object ExecuteSQLScalar(string sqlString)
        {
            return ExecuteScalar(sqlString, null, CommandType.Text);
        }

        /// <summary>
        /// ִ��SQL��䲢����ִ�н��-�в���
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteSQLScalar(string sqlString, DataAccessParameterCollection parameters)
        {
            return ExecuteScalar(sqlString, parameters, CommandType.Text);
        }

        /// <summary>
        /// ִ��executeString
        /// </summary>
        /// <param name="executeString"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public object ExecuteScalar(string executeString, DataAccessParameterCollection parameters, CommandType cmdType)
        {
            DbCommand command = CreateCommand(executeString);
            command.CommandType = cmdType;
            PrepareCommand(command, parameters);
            object val = command.ExecuteScalar();
            command.Parameters.Clear();
            command.Dispose();
            return val;
        }

        public object ExecuteScalar(WhereBuilder wb)
        {
            return ExecuteScalar(wb.SQLString, wb.Parameters, CommandType.Text);
        }
        #endregion

        #region Reader
        /// <summary>
        /// ִ�д洢���̲�����DataReader-�޲���
        /// </summary>
        /// <param name="commandString"></param>
        /// <returns></returns>
        public IDataReader ExecuteCommandReader(string commandString)
        {
            return ExecuteReader(commandString, null, CommandType.StoredProcedure);
        }

        /// <summary>
        /// ִ�д洢���̲�����DataReader-�в���
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDataReader ExecuteCommandReader(string commandString, DataAccessParameterCollection parameters)
        {
            return ExecuteReader(commandString, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// ִ��SQl��䲢����DataReader-�޲���
        /// </summary>
        /// <param name="sqlString"></param>
        /// <returns></returns>
        public IDataReader ExecuteSQLReader(string sqlString)
        {
            return ExecuteReader(sqlString, null, CommandType.Text);
        }

        /// <summary>
        /// ִ��SQl��䲢����DataReader-�в���
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDataReader ExecuteSQLReader(string sqlString, DataAccessParameterCollection parameters)
        {
            return ExecuteReader(sqlString, parameters, CommandType.Text);
        }

        /// <summary>
        /// ִ��queryString��䲢����DataReader
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string queryString, DataAccessParameterCollection parameters, CommandType cmdType)
        {
            DbCommand command = CreateCommand(queryString);
            command.CommandType = cmdType;
            PrepareCommand(command, parameters);
            DbDataReader rdr = command.ExecuteReader();
            command.Parameters.Clear();
            command.Dispose();
            return (IDataReader)rdr;
        }

        public IDataReader ExecuteReader(WhereBuilder wb)
        {
            return ExecuteReader(wb.SQLString, wb.Parameters, CommandType.Text);
        }
        #endregion

        #region Dataset

        /// <summary>
        /// ִ�д洢���̲�����DataReader-�޲���
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <param name="mapping"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public int ExecuteDataSet(string queryString, DataAccessParameterCollection parameters, CommandType cmdType, DataTableMapping mapping, ref DataSet result)
        {
            DbCommand command = CreateCommand(queryString);
            command.CommandType = cmdType;
            command.CommandTimeout = 0;
            PrepareCommand(command, parameters);
            return ExecuteDataSet(command, mapping, ref result);
        }

        /// <summary>
        /// ִ�д洢���̲�����Dataset-�޲���
        /// </summary>
        /// <param name="commandString"></param>
        /// <returns></returns>
        public DataSet FillCommandDataSet(string commandString)
        {
            DataSet result = new DataSet();
            ExecuteDataSet(commandString, null, CommandType.StoredProcedure, null, ref result);
            return result;
        }

        /// <summary>
        /// ִ�д洢���̲�����Dataset-�в���
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet FillCommandDataSet(string commandString, DataAccessParameterCollection parameters)
        {
            DataSet result = new DataSet();
            ExecuteDataSet(commandString, parameters, CommandType.StoredProcedure, null, ref result);
            return result;
        }

        /// <summary>
        /// ִ��SQL��䲢����Dataset-�޲���
        /// </summary>
        /// <param name="sqlString"></param>
        /// <returns></returns>
        public DataSet FillSQLDataSet(string sqlString)
        {
            DataSet result = new DataSet();
            ExecuteDataSet(sqlString, null, CommandType.Text, null, ref result);
            return result;
        }

        /// <summary>
        /// ִ��SQL��䲢����Dataset-�в���
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet FillSQLDataSet(string sqlString, DataAccessParameterCollection parameters)
        {
            DataSet result = new DataSet();
            ExecuteDataSet(sqlString, parameters, CommandType.Text, null, ref result);
            return result;
        }

        public DataSet FillSQLDataSet(WhereBuilder wb)
        {
            return FillSQLDataSet(wb.SQLString, wb.Parameters);
        }
        #endregion

        #region function
        /// <summary>
        /// �ر�����-���ж������Ƿ�Ϊ��
        /// </summary>
        public virtual void Close()
        {
            if (null != _connection)
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// ������-���ж������Ƿ�Ϊ��
        /// </summary>
        public virtual void Open()
        {
            if (null != _connection)
            {
                _connection.Open();
            }
        }

        /// <summary>
        /// ��ʼ����-���ж�����״̬
        /// </summary>
        public virtual void BeginTransaction()
        {
            BeginTransaction(IsolationLevel.Unspecified);
        }

        /// <summary>
        /// ��ʼ����-���ж�����״̬
        /// </summary>
        /// <param name="isolationLevel"></param>
        public virtual void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (_connection != null && _connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
            _inTransaction = true;
            _transaction = _connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// �ύ����-���ж������Ƿ�Ϊ��
        /// </summary>
        public virtual void Commit()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
            }
            _inTransaction = false;
        }

        /// <summary>
        /// �ع�����-���ж������Ƿ�Ϊ��
        /// </summary>
        public virtual void RollBack()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
            }
            _inTransaction = false;
        }

        /// <summary>
        /// ׼��Command����Ӧ����
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        protected virtual void PrepareCommand(DbCommand command, DataAccessParameterCollection parameters)
        {
            if (null != parameters && parameters.Count > 0)
            {
                ResolveParameters(command, parameters);
            }
            if (_inTransaction)//���ʹ���������commandӦ��ʹ������
            {
                command.Transaction = _transaction;
            }
        }

        /// <summary>
        /// ��Ӳ���
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        protected virtual void ResolveParameters(DbCommand command, DataAccessParameterCollection parameters)
        {
            foreach (DataAccessParameter parameter in parameters.Values)
            {
                AddParameter(command, parameter);
            }
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// ����
        /// </summary>
        ~DataAccessBroker()
        {
            Dispose(false);
        }

        /// <summary>
        /// �ͷ���Դ
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        protected virtual void Dispose(bool disposeManageResourse)
        {
            if (!_disposed)
            {
                if (disposeManageResourse)
                {
                    //�ͷ��й���Դ
                }
                Close();
                _disposed = true;
            }
        }

        #endregion

        /// <summary>
        /// ��ȡ��ṹ
        /// Jane.ren 2007/11/5
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public virtual DataTable GetSchema(string tableName)
        {
            return null;
        }


        /// <summary>
        /// ִ�д洢���̲����س���ֵ�б� Lucas 2011-01-04
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns>outputList</returns>
        public Dictionary<string,object> ExecuteProcReturnOutput(string commandString, DataAccessParameterCollection parameters)
        {
            Dictionary<string, object> outputKeyValues = new Dictionary<string, object>();

            DbCommand command = CreateCommand(commandString);
            command.CommandType = CommandType.StoredProcedure;
            PrepareCommand(command, parameters);
            int iReturn = command.ExecuteNonQuery();
            foreach (DbParameter param in command.Parameters)
            {
                if (param.Direction == ParameterDirection.Output)
                {
                    outputKeyValues.Add(param.ParameterName,param.Value);
                }
            }
            command.Parameters.Clear();
            command.Dispose();
            return outputKeyValues;
        }

        /// <summary>
        /// ������������
        /// </summary>
        /// <param name="dt">DataTable����</param>
        /// <param name="tableName">������</param>
        public abstract void BulkInsert(DataTable dt, string tableName);
    }
}
