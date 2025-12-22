using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewSpawnObject : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveSpawnObject m_subObjective;

    public QuestSubObjectiveViewSpawnObject(QuestSystemNodeObjective node, QuestSubObjectiveSpawnObject subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        var element = new VisualElement();

        VisualElement prefabElement = QuestSystemEditorUtility.CreateObjectField("Prefab", typeof(GameObject), false, m_subObjective.prefab, OnPrefabChange);
        element.Add(prefabElement);

        VisualElement nameElement = QuestSystemEditorUtility.CreateTextField(m_subObjective.name, "Name", OnNameChange);
        element.Add(nameElement);

        VisualElement locationElement = QuestSystemEditorUtility.CreateTextField(m_subObjective.location, "Location", OnLocationChange);
        element.Add(locationElement);

        VisualElement waitElement = QuestSystemEditorUtility.CreateCheckbox("Wait task completion", m_subObjective.waitTaskComplete, OnWaitChange);
        element.Add(waitElement);

        return element;
    }

    void OnPrefabChange(ChangeEvent<UnityEngine.Object> prefab)
    {
        var scr = prefab.newValue as GameObject;
        if (scr == null)
            return;

        m_subObjective.prefab = scr;
    }

    void OnNameChange(ChangeEvent<string> name)
    {
        m_subObjective.name = name.newValue;
    }

    void OnLocationChange(ChangeEvent<string> location)
    {
        m_subObjective.location = location.newValue;
    }

    void OnWaitChange(ChangeEvent<bool> wait)
    {
        m_subObjective.waitTaskComplete = wait.newValue;
    }
}
