using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class EditorLogs : MonoBehaviour
{
    class OneLog
    {
        public string log;
        public float timer;
    }

    [SerializeField] float m_logDuration = 2;

    static EditorLogs m_instance;

    TMP_Text m_text;
    Dictionary<string, OneLog> m_logs = new Dictionary<string, OneLog>();

    public static EditorLogs instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;

        m_text = GetComponentInChildren<TMP_Text>();
    }

    public void AddLog(string category, string log)
    {
        OneLog oneLog = new OneLog();
        oneLog.log = log;
        oneLog.timer = m_logDuration;
        m_logs[category] = oneLog;
    }

    private void Update()
    {
        List<string> toRemove = new List<string>();

        string fullText = "";

        foreach(var l in m_logs)
        {
            l.Value.timer -= Time.deltaTime;
            if (l.Value.timer <= 0)
                toRemove.Add(l.Key);

            fullText += l.Value.log + "\n";
        }

        foreach (var r in toRemove)
            m_logs.Remove(r);

        if (m_text != null)
            m_text.text = fullText;
    }
}
