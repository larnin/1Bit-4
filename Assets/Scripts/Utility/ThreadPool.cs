using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class ThreadPool
{
    class Job
    {
        public System.Object obj;
        public Action job;
        public Action endCallback;
        public int priority;

        public Job(System.Object _obj, Action _job, Action _endCallback, int _priority = 1)
        {
            obj = _obj;
            job = _job;
            endCallback = _endCallback;
            priority = _priority;
        }
    }

    const int m_threadCount = 4;

    static bool m_started = false;
    static bool m_aborted = false;
    static Thread[] m_threads = new Thread[m_threadCount];

    static readonly object m_pendingJobsLock = new object();
    static List<Job> m_pendingJobs = new List<Job>();

    static bool WantsToQuit()
    {
        if (!m_started)
            return true;

        Debug.Log("Stopping thread pool");

        m_aborted = true;
        for (int i = 0; i < m_threadCount; i++)
        {
            if (m_threads[i] != null)
                m_threads[i].Join();
        }

        return true;
    }

    [RuntimeInitializeOnLoadMethod]
    static void RunOnStart()
    {
        Application.wantsToQuit += WantsToQuit;
    }

    static void InitThreads()
    {
        m_started = true;
        for (int i = 0; i < m_threadCount; i++)
        {
            m_threads[i] = new Thread(new ThreadStart(Process));
            m_threads[i].Start();
        }
    }

    static void Process()
    {
        while (!m_aborted)
        {
            Job j = null;
            lock (m_pendingJobsLock)
            {
                if (m_pendingJobs.Count() != 0)
                {
                    j = m_pendingJobs[0];
                    m_pendingJobs.RemoveAt(0);
                }
            }
            if (j == null)
                Thread.Sleep(10);
            else DoJob(j);
        }
    }

    static void DoJob(Job j)
    {
        j.job();
        j.endCallback();
    }

    public static void StartJob(Action job, Action endCallback, int priority = 1, System.Object obj = null)
    {
        if (!m_started)
            InitThreads();

        if (m_aborted)
            return;

        Job j = new Job(obj, job, endCallback, priority);

        lock (m_pendingJobsLock)
        {
            if (m_pendingJobs.Count == 0 || m_pendingJobs[m_pendingJobs.Count - 1].priority >= priority)
                m_pendingJobs.Add(j);
            else
            {
                for (int i = 0; i < m_pendingJobs.Count; i++)
                {
                    if (priority > m_pendingJobs[i].priority)
                    {
                        m_pendingJobs.Insert(i, j);
                        break;
                    }
                }
            }
        }
    }
}