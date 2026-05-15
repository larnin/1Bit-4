using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewSetAllBuildingsUnlocked : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveSetAllBuildingsUnlocked m_subObjective;

    public QuestSubObjectiveViewSetAllBuildingsUnlocked(QuestSystemNodeObjective node, QuestSubObjectiveSetAllBuildingsUnlocked subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        var element = new VisualElement();

        element.Add(QuestSystemEditorUtility.CreateCheckbox("Unlocked", m_subObjective.unlocked, OnLockChange));

        element.Add(QuestSystemEditorUtility.CreateCheckbox("Globaly", m_subObjective.globalyUnlocked, OnGlobalyChange));

        return element;
    }

    void OnLockChange(ChangeEvent<bool> unlocked)
    {
        m_subObjective.unlocked = unlocked.newValue;
    }

    void OnGlobalyChange(ChangeEvent<bool> globaly)
    {
        m_subObjective.globalyUnlocked = globaly.newValue;
    }
}
