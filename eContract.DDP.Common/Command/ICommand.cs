using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using eContract.DDP.Common;
using System.Collections;

namespace eContract.DDP.Common.Command
{
    /// <summary>
    /// ����ӿ�
    /// </summary>
    public interface ICommand
    {

        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="paramters">����</param>
        /// <param name="node">����ڵ�</param>
        void Initialize(Hashtable paramters, XmlNode node);


        /// <summary>
        /// ִ������
        /// </summary>
        /// <param name="paramters">�������ڸ��������в������ܻ�����</param>
        /// <param name="error">������Ϣ</param>
        /// <returns>�Ƿ�ɹ�</returns>
        ResultCode Execute(ref Hashtable paramters, out string error);
    }


    public enum ResultCode
    {
        Success=0,
        Error=1,
        Break,
    }
}
