using System;
using System.Collections.Generic;
using System.Text;

namespace Suzsoft.Smart.Data
{
    /// <summary>
    /// ���ݿ���������
    /// </summary>
    public class DataAccessConfiguration
    {
        #region var
        private Dictionary<string, string> _parameters;
        /// <summary>
        /// ��������
        /// </summary>
        public Dictionary<string, string> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        private string _configName;
        /// <summary>
        /// ����������
        /// </summary>
        public string ConfigName
        {
            get
            {
                return _configName;
            }
            set
            {
                _configName = value;
            }
        }

        private string _dBType;
        /// <summary>
        /// ���ݿ�����
        /// </summary>
        public string DBType
        {
            get
            {
                return _dBType;
            }
            set
            {
                _dBType = value;
            }
        }

        private string _connectionString;
        /// <summary>
        /// ���ݿ������ִ�
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (_connectionString == null || _connectionString.Length == 0)
                {
                    if (string.Compare(DBType, "ORACLE", false) == 0)
                    {
                        _connectionString = "user id=" + _parameters["user"] + ";password=" + _parameters["pwd"] + ";Data Source=" + _parameters["server"];
                    }
                    else if (DBType==DataAccessFactory.DBTYPE_SQLSERVER&&_parameters.ContainsKey("IntegratedSecurity") && _parameters["IntegratedSecurity"]=="True")
                    {
                        _connectionString = "Data Source="+_parameters["server"]+";initial catalog=" + _parameters["database"] + ";Integrated Security=True;";
                    }
                    else if(DBType==DataAccessFactory.DBTYPE_SQLSERVER)
                    {
                        _connectionString = "Data Source=" + _parameters["server"] + ";User Id=" + _parameters["user"] + ";initial catalog=" + _parameters["database"] + ";Password=" + _parameters["pwd"] + "";
                    }
                      else if (this.DBType == DataAccessFactory.DBTYPE_ODBC)
                    {
                        _connectionString = _parameters["connstring"];
                    }
                     

                }
                return _connectionString; 
            }
        }

        #endregion

        public DataAccessConfiguration()
        {
            _parameters = new Dictionary<string, string>();
        }
    }
}
