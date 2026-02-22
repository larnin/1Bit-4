using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestSystemNodeStart : QuestSystemNode
{
    public override void Draw()
    {
        /* TITLE CONTAINER */
        
        Label labelName = new Label(NodeName);
        labelName.style.paddingBottom = 8;
        labelName.style.paddingLeft = 8;
        labelName.style.paddingRight = 8;
        labelName.style.paddingTop = 8;

        titleContainer.Insert(0, labelName);
        

        /* OUTPUT CONTAINER */
        Port outputPort = this.CreatePort("Out", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);

        outputContainer.Add(outputPort);

        RefreshExpandedState();
    }

    public override void UpdateStyle(bool error)
    {
        base.UpdateStyle(error);

        if (error)
        {
            mainContainer.style.backgroundColor = errorBackgroundColor;
            mainContainer.style.borderBottomColor = errorBorderColor;
            mainContainer.style.borderLeftColor = errorBorderColor;
            mainContainer.style.borderRightColor = errorBorderColor;
            mainContainer.style.borderTopColor = errorBorderColor;
        }
        else
        {
            Color backgroundColor = new Color(0.1f, 0.3f, 0.1f);
            Color borderColor = new Color(0.1f, 0.7f, 0.1f);

            mainContainer.style.backgroundColor = backgroundColor;
            mainContainer.style.borderBottomColor = borderColor;
            mainContainer.style.borderLeftColor = borderColor;
            mainContainer.style.borderRightColor = borderColor;
            mainContainer.style.borderTopColor = borderColor;
        }

        float smallRadius = 2;
        float largeRadius = 15;

        mainContainer.style.borderBottomLeftRadius = smallRadius;
        mainContainer.style.borderBottomRightRadius = largeRadius;
        mainContainer.style.borderTopLeftRadius = smallRadius;
        mainContainer.style.borderTopRightRadius = largeRadius;

        UpdateDisplayStateStyle();
    }

    public override VisualElement GetDetailElement()
    {
        return QuestSystemEditorUtility.CreateLabel("This is here everything start.");
    }
}
