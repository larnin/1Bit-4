using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorToolHolder : MonoBehaviour
{
    EditorToolBase m_currentTool;

    static EditorToolHolder m_instance;

    public static EditorToolHolder instance { get { return m_instance; } }

    private void Awake()
    {
        if (m_instance == null)
            m_instance = this;
    }

    private void OnDestroy()
    {
        SetCurrentTool(null);
    }

    public void SetCurrentTool(EditorToolBase tool)
    {
        if (m_currentTool != null)
            m_currentTool.End();

        m_currentTool = tool;
        if (m_currentTool == null)
            return;

        m_currentTool.SetHolder(this);
        m_currentTool.Begin();

        if (EditorLogs.instance != null)
            EditorLogs.instance.AddLog("Tool", "Select tool " + GetToolName(tool));
    }

    string GetToolName(EditorToolBase tool)
    {
        if (tool == null)
            return "NULL";

        string name = tool.GetType().Name;

        string prefix = "EditorTool";

        if (!name.StartsWith(prefix))
            return name;

        return name.Substring(prefix.Length);
    }

    public EditorToolBase GetCurrentTool()
    {
        return m_currentTool;
    }

    void Update()
    {
        if (m_currentTool != null)
            m_currentTool.Update();
    }
}
