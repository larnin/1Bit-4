using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewStartGamemode : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveStartGamemode m_subObjective;

    public QuestSubObjectiveViewStartGamemode(QuestSystemNodeObjective node, QuestSubObjectiveStartGamemode subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        var element = new VisualElement();

        element.Add(QuestSystemEditorUtility.CreateTextField(m_subObjective.name, "Name", OnNameChange));

        element.Add(QuestSystemEditorUtility.CreateObjectField("Gamemode", typeof(GamemodeAssetBase), false, m_subObjective.gamemode, OnGamemodeChange));

        return element;
    }

    void OnNameChange(ChangeEvent<string> name)
    {
        m_subObjective.name = name.newValue;
    }

    void OnGamemodeChange(ChangeEvent<UnityEngine.Object> gamemode)
    {
        var scr = gamemode.newValue as GamemodeAssetBase;
        if (scr == null)
            return;

        m_subObjective.gamemode = scr;
    }
}
