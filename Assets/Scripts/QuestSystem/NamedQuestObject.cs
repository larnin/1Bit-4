using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NamedQuestObject : MonoBehaviour
{
    [SerializeField] string m_name;

    bool m_added = false;
    bool m_isCursor = false;

    public virtual bool IsTaskComplete() { return true; }

    public string GetName()
    {
        return m_name;
    }

    public void SetName(string name)
    {
        m_name = name;
    }

    private void Awake()
    {
        Add();
    }

    private void OnDestroy()
    {
        Remove();
    }

    public void SetAsCursor(bool asCursor)
    {
        m_isCursor = asCursor;

        if (!m_isCursor)
            Add();
        else Remove();
    }

    void Add()
    {
        if (m_isCursor)
            return;

        var manager = QuestElementList.instance;
        if (manager != null)
        {
            m_added = true;
            manager.Register(this);
        }
    }

    void Remove()
    {
        if (!m_added)
            return;

        var manager = QuestElementList.instance;
        if (manager != null)
            manager.UnRegister(this);

        m_added = false;
    }
}
