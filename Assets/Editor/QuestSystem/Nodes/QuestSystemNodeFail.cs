using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestSystemNodeFail : QuestSystemNode
{
    public override void Initialize(string nodeName, QuestSystemGraphView view, Vector2 position, bool createID = true)
    {
        base.Initialize(nodeName, view, position, createID);

        if (createID)
            NodeName = "Fail_" + ID;
    }

    public override void Draw()
    {
        /* TITLE CONTAINER */

        Label labelName = new Label("Fail");
        labelName.style.paddingBottom = 8;
        labelName.style.paddingLeft = 8;
        labelName.style.paddingRight = 8;
        labelName.style.paddingTop = 8;

        titleContainer.Insert(0, labelName);


        /* OUTPUT CONTAINER */
        Port inputPort = this.CreatePort("In", Orientation.Horizontal, Direction.Input, Port.Capacity.Single);

        inputContainer.Add(inputPort);

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
            Color backgroundColor = new Color(0.3f, 0.3f, 0.1f);
            Color borderColor = new Color(0.7f, 0.7f, 0.1f);

            mainContainer.style.backgroundColor = backgroundColor;
            mainContainer.style.borderBottomColor = borderColor;
            mainContainer.style.borderLeftColor = borderColor;
            mainContainer.style.borderRightColor = borderColor;
            mainContainer.style.borderTopColor = borderColor;
        }

        float smallRadius = 2;
        float largeRadius = 15;

        mainContainer.style.borderBottomLeftRadius = largeRadius;
        mainContainer.style.borderBottomRightRadius = smallRadius;
        mainContainer.style.borderTopLeftRadius = largeRadius;
        mainContainer.style.borderTopRightRadius = smallRadius;
    }
}
