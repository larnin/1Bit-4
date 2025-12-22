using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuestSubObjectiveSpawnObject : QuestSubObjectiveBase
{
    [SerializeField] GameObject m_prefab;
    public GameObject prefab { get { return m_prefab; } set { m_prefab = value; } }

    [SerializeField] string m_name;
    public string name { get { return m_name; } set { m_name = value; } }

    [SerializeField] string m_location;
    public string location { get { return m_location; } set { m_location = value; } }

    [SerializeField] bool m_waitTaskComplete = false;
    public bool waitTaskComplete { get { return m_waitTaskComplete; } set { m_waitTaskComplete = value; } }

    NamedQuestObject m_instance = null;

    public override bool IsCompleted()
    {
        if (!m_waitTaskComplete)
            return true;

        if (m_instance == null)
            return true;

        return m_instance.IsTaskComplete();
    }

    public override void Start()
    {
        if (m_prefab == null)
            return;

        if (QuestElementList.instance == null)
            return;

        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        if (m_location != null && m_location.Length > 0)
        {
            NamedQuestObject pivot = QuestElementList.instance.GetFirstNamedObjectByName(m_location);
            if (pivot == null)
                return;
            pos = pivot.transform.position;
            rot = pivot.transform.rotation;
        }

        var obj = GameObject.Instantiate(m_prefab);
        obj.transform.position = pos;
        obj.transform.rotation = rot;

        m_instance = obj.GetComponent<NamedQuestObject>();
        if (m_instance != null)
            m_instance.SetName(m_name);
    }

    public override void Update(float deltaTime) { }

    public override void End() { }
}
