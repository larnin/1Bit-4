using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class QuestSubObjectiveDestroyObject : QuestSubObjectiveBase
{
    [SerializeField] string m_name;
    public string name { get { return m_name; } set { m_name = value; } }

    public override bool IsCompleted()
    {
        return true;
    }

    public override void Start()
    {
        if (QuestElementList.instance == null)
            return;

        var elems = QuestElementList.instance.GetNamedObjectsByName(m_name);
        foreach (var e in elems)
            GameObject.Destroy(e.gameObject);
    }

    public override void Update(float deltaTime) { }

    public override void End() { }
}