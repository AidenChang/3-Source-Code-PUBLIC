using System;
using System.Collections.Generic;
using System.Text;
using eContract.DDP.Common.Command;
using System.Collections;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using eContract.DDP.Common.CommonJob;

namespace eContract.DDP.Common.Command
{
    public class ZipCommand:BaseCommand
    {
        /// <summary>
        /// ������Ϣ
        /// </summary>
        public string Type = "";                 // Compressѹ����Decompress��ѹ
        public string SourceFileName = "";   // Դ�ļ�
        public string SourceDir = "";           // ԴĿ¼ ��SourceFileName����ѡһ
        public string TargetFileName = "";    // Ŀ���ļ�
        public string TargetDir = "";            // Ŀ��Ŀ¼

        #region ICommand Members


        /*
		<Zip Type="Compress"
				SourceFileName="%FileName%.txt" 
				TargetFileName="%FileName%.zip"/>
         */
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

            // ѹ��
            if (this.Type == DDPConst.Zip_Type_Compress)
            {
                FastZip fastZip = new FastZip();

                string dir = "";
                string fileFilter = "";

                // ֧�ֶ����Դ�ļ�
                string[] sourceFileList = this.SourceFileName.Split(new char[] { ';' });
                for (int i = 0; i < sourceFileList.Length; i++)
                {
                    string oneFile = sourceFileList[i].Trim();
                    if (oneFile != "")
                    {
                        if (dir == "")
                            dir = Path.GetDirectoryName(oneFile);

                        if (fileFilter != "")
                            fileFilter += ";";

                        fileFilter += Path.GetFileName(oneFile);

                    }
                }

                fastZip.CreateZip(this.TargetFileName, dir, false, fileFilter);

            }
            else if (this.Type == DDPConst.Zip_Type_Decompress) // ��ѹ
            {

            }

            return ResultCode.Success; ;
        }


        #endregion

    }
}
