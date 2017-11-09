using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using eContract.DDP.Common.CommonJob;

namespace eContract.DDP.Common.Command
{
    public class BaseCommand:ICommand
    {
        #region ��Ա����

        /// <summary>
        /// ��ʼ��ʱ����Ĳ���
        /// </summary>
        public Hashtable InitialParameters = null;

        /// <summary>
        /// ����ڵ�
        /// </summary>
        public XmlNode CommandNode = null;

        /// <summary>
        /// ����������룬���ڲ���ʱ������
        /// </summary>
        public string JobCode = "";

        public string ThreadName = "";

        #endregion

        #region ICommand Members

        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="paramters">�������</param>
        /// <param name="node">����ڵ�</param>
        public virtual void Initialize(Hashtable paramters, XmlNode node)
        {
            this.InitialParameters = paramters;
            this.CommandNode = node;

            // �Ӳ�����ȡ��������룬����д��־
            this.JobCode = paramters[DDPConst.C_JobCode].ToString();
            if (paramters.ContainsKey(DDPConst.C_ThreadName) == true)
                this.ThreadName = paramters[DDPConst.C_ThreadName].ToString();
        }

        /// <summary>
        /// ִ������
        /// </summary>
        /// <param name="paramters">�������ڸ��������в������ܻ�����</param>
        /// <param name="error">������Ϣ</param>
        /// <returns>�Ƿ�ɹ�</returns>
        public virtual ResultCode Execute(ref Hashtable paramters, out string error)
        {
            error = "";

            return ResultCode.Success;
        }



        #endregion

        #region ��̬����

        /// <summary>
        /// �滻����
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="text"></param>
        public static string ReplaceParameters(Hashtable parameters, string text)
        {
            foreach (DictionaryEntry parameter in parameters)
            {
                if (parameter.Value != null && parameter.Value is string)
                    text = text.Replace(parameter.Key.ToString(), parameter.Value.ToString());
            }

            return text;
        }
        /// <summary>
        /// ����where���
        /// </summary>
        /// <param name="fieldNames"></param>
        /// <param name="dataIDList"></param>
        /// <param name="dataSplitOperator"></param>
        /// <returns></returns>
        public static string MakeDataIDWhereSql(string fieldNames, List<string> dataIDList, string dataSplitOperator)
        {
            string whereSql = "";

            // ���õĶ�����������Զ��ŷָ�
            string[] fieldList = fieldNames.Split(new char[] { ',' });


            // ����������orƴ����
            for (int i = 0; i < dataIDList.Count; i++)
            {
                if (whereSql != "")
                    whereSql += " or ";

                // ���������andƴ����
                string sql2 = "";
                string values = dataIDList[i];
                string[] valueList = new string[1];
                if (dataSplitOperator.Length > 0)
                    valueList = values.Split(new char[] { dataSplitOperator[0] });
                else
                    valueList[0] = values;
                for (int j = 0; j < valueList.Length; j++)
                {
                    if (sql2 != "")
                        sql2 += " and ";
                    sql2 += fieldList[j] + "='" + valueList[j] + "' ";
                }

                if (sql2 != "")
                    whereSql += "(" + sql2 + ")";
            }

            // ��ѯsql���
            if (whereSql != "")
                whereSql = " where " + whereSql;

            return whereSql;
        }

        #endregion
    }
}
