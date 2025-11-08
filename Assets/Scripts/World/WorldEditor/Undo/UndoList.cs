using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UndoList : MonoBehaviour
{
    [SerializeField] int m_maxUndoSize = -1;

    static UndoList m_instance = null;
    public static UndoList instance { get { return m_instance; } }

    List<UndoElementBase> m_undoList = new List<UndoElementBase>();
    List<UndoElementBase> m_redoList = new List<UndoElementBase>();

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    public void AddStep(UndoElementBase element)
    {
        m_undoList.Add(element);
        m_redoList.Clear();

        if(m_maxUndoSize >= 0)
        {
            while (m_undoList.Count > m_maxUndoSize)
                m_undoList.RemoveAt(0);
        }

    }

    public void Undo()
    {
        if (m_undoList.Count <= 0)
            return;

        var elem = m_undoList[m_undoList.Count - 1];
        m_redoList.Add(elem.GetRevertElement());
        elem.Apply();

        m_undoList.RemoveAt(m_undoList.Count - 1);
    }

    public void Redo()
    {
        if (m_redoList.Count <= 0)
            return;

        var elem = m_redoList[m_redoList.Count - 1];
        m_undoList.Add(elem.GetRevertElement());
        elem.Apply();

        m_redoList.RemoveAt(m_redoList.Count - 1);
    }

    public void Clear()
    {
        m_undoList.Clear();
        m_redoList.Clear();
    }
}
