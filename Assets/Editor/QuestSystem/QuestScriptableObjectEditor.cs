using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestScriptableObject))]
public class QuestScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Label("Click on this button to edit the asset\nOr open the tool 'Game->Quest System'");
        if(GUILayout.Button("Open"))
        {
            if (!(target is QuestScriptableObject))
                return;

            QuestSystemGraph graph = QuestSystemGraph.Open();
            if (graph == null)
                return;

            graph.Load(target as QuestScriptableObject);
        }
    }
}