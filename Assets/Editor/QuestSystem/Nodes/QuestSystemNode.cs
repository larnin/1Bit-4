using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestSystemNode : Node
{
    public static Color errorBorderColor = new Color(1, 0, 0);
    public static Color errorBackgroundColor = new Color(0.5f, 0.2f, 0.2f);

    public string ID { get; set; }
    public string NodeName { get; set; }

    protected QuestSystemGraphView m_graphView;

    protected bool m_error = false;

    public QuestSystemGraphView GetGraph()
    {
        return m_graphView;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Disconnect Input Port", actionEvent => DisconnectInputPorts());
        evt.menu.AppendAction("Disconnect Output Port", actionEvent => DisconnectOutputPorts());

        base.BuildContextualMenu(evt);
    }

    public virtual void Initialize(string nodeName, QuestSystemGraphView view, Vector2 position, bool createID = true)
    {
        if (createID)
            ID = Guid.NewGuid().ToString();
        NodeName = nodeName;

        SetPosition(new Rect(position, Vector2.zero));

        m_graphView = view;

        mainContainer.AddToClassList("quest-node__main-container");
        extensionContainer.AddToClassList("quest-node__extension-container");

        UpdateStyle(m_error);
    }


    public virtual void Draw()
    {
        /* TITLE CONTAINER */

        TextField dialogueNameTextField = QuestSystemEditorUtility.CreateTextField(NodeName, null, callback =>
        {
            TextField target = (TextField)callback.target;

            target.value = callback.newValue.RemoveSpecialCharacters();

            NodeName = target.value;

            m_graphView.ProcessErrors();
        });

        dialogueNameTextField.AddClasses(
            "quest-node__text-field",
            "quest-node__text-field__hidden",
            "quest-node__filename-text-field"
        );

        titleContainer.Insert(0, dialogueNameTextField);

        RefreshExpandedState();
    }

    public void DisconnectAllPorts()
    {
        DisconnectInputPorts();
        DisconnectOutputPorts();
    }

    private void DisconnectInputPorts()
    {
        DisconnectPorts(inputContainer);
    }

    private void DisconnectOutputPorts()
    {
        DisconnectPorts(outputContainer);
    }

    private void DisconnectPorts(VisualElement container)
    {
        foreach (Port port in container.Children())
        {
            if (!port.connected)
            {
                continue;
            }

            m_graphView.DeleteElements(port.connections);
        }
    }

    public virtual void UpdateStyle(bool error)
    {
        m_error = error;
    }
}
