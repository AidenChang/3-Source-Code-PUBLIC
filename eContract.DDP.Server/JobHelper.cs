using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using eContract.Log;
using eContract.DDP.Common.CommonJob;
using System.Collections;
using eContract.DDP.Common;
using System.Net.Mail;

namespace eContract.DDP.Server
{
    public class JobHelper
    {
        private IJob _job = null;
        public Hashtable Parameters = null;
        public string ThreadName = "";
        public JobEntity JobEntity = null;
        public bool IsRunError = false;

        /// <summary>
        /// ��������¼�
        /// </summary>
        public event EventHandler RunFinished;

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="job"></param>
        /// <param name="parameters"></param>
        /// <param name="threadName"></param>
        /// <param name="jobEntity"></param>
        public JobHelper(IJob job, Hashtable parameters,string threadName,JobEntity jobEntity)
        {
            this._job = job;
            this.Parameters = parameters;
            this.ThreadName = threadName;
            this.JobEntity = jobEntity;
        }
        
        /// <summary>
        /// ����
        /// </summary>
        public void Run()
        {

            LogManager.Current.WriteCommonLog(this.JobEntity.Code, "���п�ʼ" + this.ThreadName, this.ThreadName);
            try
            {
                // ִ��
                this._job.RunSafe(Parameters, this.JobEntity, this.ThreadName);
                IsRunError = true;
            }
            catch 
            {
                IsRunError = false;
                
            }
            finally
            {
                LogManager.Current.WriteCommonLog(this.JobEntity.Code, "���н���" + this.ThreadName, this.ThreadName);
                // ��������¼�
                if (this.RunFinished != null)
                {
                    this.RunFinished(this, EventArgs.Empty);
                }
            }
        }

    }
}
