using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewSpawnCanvasObject : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveSpawnCanvasObject m_subObjective;

    public QuestSubObjectiveViewSpawnCanvasObject(QuestSystemNodeObjective node, QuestSubObjectiveSpawnCanvasObject subObjective) : base(node, subObjective)
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

        VisualElement locationElement = QuestSystemEditorUtility.CreateVector3Field(m_subObjective.location, "Location", OnLocationChange);
        element.Add(locationElement);

        EnumField anchorElement = new EnumField("Anchor", m_subObjective.anchor);
        anchorElement.RegisterValueChangedCallback(OnAnchorChange);
        element.Add(anchorElement);

        VisualElement waitElement = QuestSystemEditorUtility.CreateCheckbox("Wait task completion", m_subObjective.waitTaskComplete, OnWaitChange);
        element.Add(waitElement);

        VisualElement destroyElement = QuestSystemEditorUtility.CreateCheckbox("Destroy on objective completion", m_subObjective.destroyOnObjectiveComplete, OnDestroyChange);
        element.Add(destroyElement);

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

    void OnLocationChange(ChangeEvent<Vector3> location)
    {
        m_subObjective.location = location.newValue;
    }

    void OnWaitChange(ChangeEvent<bool> wait)
    {
        m_subObjective.waitTaskComplete = wait.newValue;
    }

    void OnDestroyChange(ChangeEvent<bool> value)
    {
        m_subObjective.destroyOnObjectiveComplete = value.newValue;
    }

    void OnAnchorChange(ChangeEvent<Enum> value)
    {
        m_subObjective.anchor = value.newValue as QuestSpawnCanvasObjectAnchor? ?? QuestSpawnCanvasObjectAnchor.Center;
    }
}

