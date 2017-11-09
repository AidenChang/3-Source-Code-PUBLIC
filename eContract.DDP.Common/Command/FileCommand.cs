using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using eContract.DDP.Common.CommonJob;
using System.IO;
using System.Xml;

namespace eContract.DDP.Common.Command
{
    public class FileCommand:BaseCommand
    {
        /// <summary>
        /// ������Ϣ
        /// </summary>
        public string Type = "";                 // rename
        public string SourceFileName = "";   // Դ�ļ�
        public string TargetFileName = "";    // Ŀ���ļ�

        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="appDir"></param>
        /// <param name="node"></param>
        public override void Initialize(Hashtable parameters, XmlNode node)
        {
            // �ȵ����ຯ��
            base.Initialize(parameters, node);

            // ��������
            this.Type = XmlUtil.GetAttrValue(node, "Type");
            this.SourceFileName = XmlUtil.GetAttrValue(node, "SourceFileName");
            this.SourceFileName = BaseCommand.ReplaceParameters(parameters, this.SourceFileName);

            this.TargetFileName = XmlUtil.GetAttrValue(node, "TargetFileName");
            this.TargetFileName = BaseCommand.ReplaceParameters(parameters, this.TargetFileName);
        }

        /// <summary>
        /// ִ��
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public override ResultCode Execute(ref Hashtable paramters, out string error)
        {
            error = "";

            // ����
            if (this.Type == DDPConst.File_Type_Rename)
            {
                if (File.Exists(this.TargetFileName) == true)
                    File.Delete(this.TargetFileName);

                FileInfo fi = new FileInfo(this.SourceFileName);
                fi.MoveTo(this.TargetFileName);
            }

            return ResultCode.Success; ;
        }
    }
}
