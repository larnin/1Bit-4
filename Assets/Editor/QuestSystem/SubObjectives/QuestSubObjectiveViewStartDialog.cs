using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewStartDialog : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveStartDialog m_subObjective;

    VisualElement m_textsContainer;
    VisualElement m_delayElement;

    public QuestSubObjectiveViewStartDialog(QuestSystemNodeObjective node, QuestSubObjectiveStartDialog subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        var element = new VisualElement();

        EnumField endType = new EnumField("End type", m_subObjective.dialogEndType);
        endType.RegisterValueChangedCallback(OnEndTypeChange);
        element.Add(endType);

        m_delayElement = QuestSystemEditorUtility.CreateFloatField(m_subObjective.delayToClose, "Delay to close", OnDelayChange);
        m_delayElement.tooltip = "Only active if the End type is CloseAfterTimer";
        element.Add(m_delayElement);
        UpdateDelayVisibility();

        m_textsContainer = new VisualElement();
        element.Add(m_textsContainer);
        UpdateTextsList();

        element.Add(QuestSystemEditorUtility.CreateButton("Add", AddTextClick));

        return element;
    }

    void OnEndTypeChange(ChangeEvent<Enum> value)
    {
        m_subObjective.dialogEndType = value.newValue as QuestDialogEndType? ?? QuestDialogEndType.InputToEnd;
        UpdateDelayVisibility();
    }

    void OnDelayChange(ChangeEvent<float> value)
    {
        m_subObjective.delayToClose = value.newValue;
    }

    void AddTextClick()
    {
        m_subObjective.AddNewText();
        UpdateTextsList();
    }

    void RemoveText(int index)
    {
        m_subObjective.RemoveTextAt(index);
        UpdateTextsList();
    }

    void TextUpdate(ChangeEvent<string> text, int index)
    {
        m_subObjective.SetTextAt(index, text.newValue);
    }

    void UpdateTextsList()
    {
        if (m_textsContainer == null)
            return;

        m_textsContainer.Clear();

        int nbText = m_subObjective.GetTextCount();
        for(int i = 0; i < nbText; i++)
        {
            VisualElement elem = new VisualElement();
            elem.style.flexDirection = FlexDirection.Row;

            int index = i;

            elem.Add(QuestSystemEditorUtility.CreateTextArea(m_subObjective.GetTextAt(i), null, (ChangeEvent<string> newValue) => { TextUpdate(newValue, index); }));

            var deleteButton = QuestSystemEditorUtility.CreateButton("  X", () => { RemoveText(index); });
            deleteButton.style.width = 15;
            elem.Add(deleteButton);

            m_textsContainer.Add(elem);
        }
    }

    void UpdateDelayVisibility()
    {
        m_delayElement.SetEnabled(m_subObjective.dialogEndType == QuestDialogEndType.CloseAfterTimer);
    }
}
