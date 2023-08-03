using System;
using System.Collections;
using System.Threading;

namespace Nashet.Utils
{
	public abstract class ThreadedJob
    {
        private bool m_IsDone;
        private string status = "Not started yet";
        private object m_Handle = new object();
        private Thread m_Thread;

        public bool IsDone
        {
            get
            {
                bool tmp;
                lock (m_Handle)
                {
                    tmp = m_IsDone;
                }
                return tmp;
            }
            set
            {
                lock (m_Handle)
                {
                    m_IsDone = value;
                }
            }
        }

        public void updateStatus(String status)
        {
            lock (this.status)
            {
                this.status = status;
            }
        }

        public string getStatus()
        {
            //tmp = status;
            lock (status)
            {
                return status;
            }
        }

        public virtual void Start()
        {
            m_Thread = new Thread(Run);
            m_Thread.Start();
        }

        public virtual void Abort()
        {
            m_Thread.Abort();
        }

        protected abstract void ThreadFunction();

        protected virtual void OnFinished()
        {
        }

        public virtual bool Update()
        {
            if (IsDone)
            {
                OnFinished();
                return true;
            }
            return false;
        }

        public IEnumerator WaitFor()
        {
            while (!Update())
            {
                yield return null;
            }
        }

        private void Run()
        {
            ThreadFunction();
            IsDone = true;
        }
    }
}