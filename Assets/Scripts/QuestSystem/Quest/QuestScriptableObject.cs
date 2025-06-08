using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuestSaveConnection
{
    public string currentPortName;
    public string nextPortName;
    public string nextNodeName;
}

public class QuestSaveNode
{
    public string ID;
    public string name;
    public QuestSystemNodeType nodeType;
    public Rect position;

    public object data;

    public List<QuestSaveConnection> outNodes = new List<QuestSaveConnection>();
}

public class QuestSaveData
{
    public List<QuestSaveNode> nodes = new List<QuestSaveNode>();
}

public class QuestScriptableObject : SerializedScriptableObject
{
    [SerializeField] public QuestSaveData data;
}
