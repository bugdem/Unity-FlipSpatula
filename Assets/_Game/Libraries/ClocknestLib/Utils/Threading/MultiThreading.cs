
using UnityEngine;
using System.Collections;

namespace ClocknestGames.Library.Threading
{
    public class MultiThreading
    {
        public class ThreadedJob
        {
            private System.Threading.Thread m_Thread = null;
            private bool m_IsDone = false;
            public bool IsDone
            {
                get
                {
                    bool tmp;
                    tmp = m_IsDone;
                    return tmp;
                }
                set
                {
                    m_IsDone = value;
                }
            }
            public virtual void SetAffinity()
            {
                m_Thread.Priority = System.Threading.ThreadPriority.Highest;
                m_Thread.IsBackground = true;
            }
            public virtual void Start()
            {
                m_Thread = new System.Threading.Thread(Run);
                m_Thread.Start();
            }
            public virtual void Abort()
            {
                m_Thread.Abort();
            }

            protected virtual void ThreadFunction() { }

            protected virtual void OnFinished() { }

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
}