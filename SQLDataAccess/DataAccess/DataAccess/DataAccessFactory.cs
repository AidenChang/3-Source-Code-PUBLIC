using System;
using System.Collections.Generic;
using System.Text;

namespace Suzsoft.Smart.Data
{
    /// <summary>
    /// ���ݷ��ʹ���//TODO:���ӳ�
    /// </summary>
    public class DataAccessFactory
    {
        // Jane.ren 2007/10/18
        public const string DBTYPE_ORACLE = "ORACLE";
        public const string DBTYPE_SQLSERVER = "SQLSERVER";
        public const string DBTYPE_ODBC = "ODBC";
        /// <summary>
        /// ��ȡĬ�����ݷ���
        /// </summary>
        /// <returns></returns>
        public static DataAccessBroker Instance()
        {
            return Instance("");
        }

        /// <summary>
        /// �������ƻ�ȡ���ݷ���
        /// </summary>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        public static DataAccessBroker Instance(string instanceName)
        {
            DataAccessConfiguration config = DataAccessConfigurationMangement.GetDataAccessConfiguration(instanceName.Trim());
            return Instance(config);
        }

        /// <summary>
        /// �������û�ȡ���ݷ���
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static DataAccessBroker Instance(DataAccessConfiguration config)
        {
            DataAccessBroker _broker = null;
            if (string.Compare(config.DBType, DBTYPE_SQLSERVER, false) == 0)
            {
                _broker = new SQLDataAccessBroker();
                _broker.Configuration = config;
            }
            //else if (string.Compare(config.DBType, DBTYPE_ODBC, false) == 0)
            //{
            //    _broker = new ODBCDataAccessBroker();
            //    _broker.Configuration = config;
            //}
            else
            {
                _broker = new OracelDataAccessBroker();
                _broker.Configuration = config;
            }
            _broker.Create();//�����������ݿ�����
            return _broker;
        }
    }
}
