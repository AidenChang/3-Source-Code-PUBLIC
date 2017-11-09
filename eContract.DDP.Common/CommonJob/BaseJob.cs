using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using eContract.DDP.Common.Command;
using eContract.DDP.Common.CommonJob;
using eContract.DDP.Common;
using System.Threading;
using System.Windows.Forms;
using System.Net.Mail;
using eContract.Log;

namespace eContract.DDP.Common.CommonJob
{
    public class BaseJob:IJob
    {
        /// <summary>
        /// Jobʵ��
        /// </summary>
        public JobEntity JobEntity = null;

        /// <summary>
        /// �����
        /// </summary>
        public List<ICommand> CommandList = new List<ICommand>();

        private string _jobCode = "";
        /// <summary>
        /// �������
        /// </summary>
        public string JobCode
        {
            get
            {
                return this._jobCode;
            }
            set
            {
                this._jobCode = value;
            }
        }

        private string _threadName = "";
        public string ThreadName
        {
            get
            {
                return this._threadName;
            }
            set
            {
                LogManager.Current.WriteCommonLog(this.JobCode, "��_threadName��ʼ,ԭ:" + this._threadName + " ��:" + value, value);

                this._threadName = value;

                LogManager.Current.WriteCommonLog(this.JobCode, "��_threadName����", value);

            }
        }

        /// <summary>
        /// ��ȫ��ʽ�µ�����
        /// </summary>
        /// <param name="parameters"></param>
        public virtual void RunSafe(Hashtable parameters, JobEntity jobEntity,string threadName)
        {
            lock (this)
            {
                try
                {
                    LogManager.Current.WriteCommonLog(this.JobCode, "RunSafe��ʼ", threadName);


                    this.ThreadName = threadName;

                    this.Initialize(parameters, jobEntity);


                    this.Run(parameters);

                    LogManager.Current.WriteCommonLog(this.JobCode, "RunSafe����", threadName);

                }
                catch (Exception ex)
                {
                    // д��־
                    LogManager.Current.WriteExceptionLog(this.JobEntity.Code, "ִ��ʱ�쳣", ex, this.ThreadName);

                    throw ex;
                }
                //catch
                //{
                //    // д������־
                //    LogManager.Current.WriteErrorLog(this.JobEntity.Code, "δʶ����쳣", this.ThreadName);
                //}
            }
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="jobEntity"></param>
        public virtual void Initialize(Hashtable internalParameters, JobEntity jobEntity)
        {
            // ��ȡ�ⲿ����
            Hashtable parameters = new Hashtable();
            this.JobEntity = jobEntity;

            XmlNodeList parameterList = jobEntity.JobNode.SelectNodes("Parameters/Parameter");
            for (int i = 0; i < parameterList.Count; i++)
            {
                XmlNode parameterNode = parameterList[i];
                string paramName = XmlUtil.GetAttrValue(parameterNode,"Name");
                if (internalParameters.ContainsKey(paramName) == true)
                    continue;

                string paramValue = XmlUtil.GetAttrValue(parameterNode,"Value");

                // ���滻���ڲ��Ĳ���
                paramValue = BaseCommand.ReplaceParameters(internalParameters, paramValue);
                // ���滻һ���ⲿǰ�涨��Ĳ���
                paramValue = BaseCommand.ReplaceParameters(parameters, paramValue);

                parameters[paramName] = paramValue;
            }

            // ��������Ĳ�����Ҫ�����ڲ��������ⲿ����
            foreach (DictionaryEntry parameter in internalParameters)
            {
                parameters[parameter.Key.ToString()] =parameter.Value.ToString();
            }

            CommandList.Clear();
            string appDir = (string)parameters[DDPConst.Param_AppDir];
            parameters[DDPConst.C_JobCode] = this.JobEntity.Code;
            parameters[DDPConst.C_ThreadName] = this.ThreadName;
            XmlNode commandsNode = jobEntity.JobNode.SelectSingleNode("Commands");
            if (commandsNode != null)
            {
                for (int i = 0; i < commandsNode.ChildNodes.Count; i++)
                {
                    XmlNode node = commandsNode.ChildNodes[i];
                    if (!(node is XmlElement))
                        continue;

                    ICommand command = CommandFactory.CreateCommand(appDir,node);                 
                    if (command != null)
                    {                       
                        // ��ʼ��
                        command.Initialize(parameters, node);

                        // ���������б�
                        this.CommandList.Add(command);
                    }
                }
            }

            LogManager.Current.WriteCommonLog(this.JobCode, "��ʼ�����", this.ThreadName);
        }

        /// <summary>
        /// ִ��
        /// </summary>
        /// <param name="parameters"></param>
        public virtual void Run(Hashtable parameters)
        {
            string error = "";
            ResultCode retCode = ResultCode.Success;

            if (this.CommandList != null)
            {
                for (int i = 0; i < this.CommandList.Count; i++)
                {
                    ICommand command = this.CommandList[i];

                    //// д��־
                    LogManager.Current.WriteCommonLog(this.JobCode, command.ToString() + " ��ʼ", this.ThreadName);


                    retCode = command.Execute(ref parameters, out error);

                    //// д��־
                    LogManager.Current.WriteCommonLog(this.JobCode, command.ToString() +" ���", this.ThreadName);


                    if (retCode == ResultCode.Error)
                    {
                        // д��־
                        LogManager.Current.WriteErrorLog(JobEntity.Code, command.ToString() + "�������:" + error, this.ThreadName);
                        return;
                    }
                    else if (retCode == ResultCode.Break)
                    {
                        // д��־
                        LogManager.Current.WriteCommonLog(JobEntity.Code, "�����������", this.ThreadName);
                        continue;
                    }
                }
            }


        }




        #region IJob Members


        public virtual JobCfgForm GetCfgForm()
        {
            return new BaseJobCfgForm();
        }

        public virtual JobRunControl GetRunControl()
        {
            return new JobRunControl();
        }

        public event GetParamsEventHandle GetParameters;

        /// <summary>
        /// �����¼�
        /// </summary>
        /// <param name="info"></param>
        public void OnGetParameters(Hashtable parameters)
        {
            if (GetParameters != null)
            {
                GetParamsEventArge e = new GetParamsEventArge();
                e.Parameters = parameters;
                this.GetParameters(this, e);
            }
        }
        #endregion
    }
}
