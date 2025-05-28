using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestSystemGraphView : GraphView
{
    QuestSystemGraph m_editorWindow;

    List<QuestSystemNode> m_nodes = new List<QuestSystemNode>();
    QuestSystemNodeStart m_startNode;

    public QuestSystemGraphView(QuestSystemGraph editorWindow)
    {
        m_editorWindow = editorWindow;

        AddManipulators();
        AddGridBackground();
        AddStyles();

        OnElementsDeleted();
        OnGraphViewChanged();

        AddStartNode();
    }

    public QuestSystemGraph GetWindow()
    {
        return m_editorWindow;
    }

    private void AddGridBackground()
    {
        GridBackground gridBackground = new GridBackground();

        gridBackground.StretchToParentSize();

        Insert(0, gridBackground);
    }

    private void AddStyles()
    {
        this.AddStyleSheets(
            "QuestSystem/QuestSystemGraphViewStyles.uss",
            "QuestSystem/QuestSystemNodeStyles.uss"
        );
    }

    void OnElementsDeleted()
    {
        //todo
        deleteSelection = (operationName, askUser) =>
        {
            Type edgeType = typeof(Edge);

            List<QuestSystemNode> nodesToDelete = new List<QuestSystemNode>();
            List<Edge> edgesToDelete = new List<Edge>();

            foreach (GraphElement selectedElement in selection)
            {
                if (selectedElement is QuestSystemNode node)
                {
                    if (node == m_startNode)
                        continue;

                    nodesToDelete.Add(node);

                    continue;
                }

                if (selectedElement.GetType() == edgeType)
                {
                    Edge edge = (Edge)selectedElement;

                    edgesToDelete.Add(edge);

                    continue;
                }
            }

            DeleteElements(edgesToDelete);

            foreach (QuestSystemNode nodeToDelete in nodesToDelete)
            {
                RemoveNode(nodeToDelete, false);

                nodeToDelete.DisconnectAllPorts();

                RemoveElement(nodeToDelete);
            }

            ProcessErrors();
        };
    }

    void OnGraphViewChanged()
    {
        //todo
        graphViewChanged = (changes) =>
        {
            ProcessErrors(); //does not works for edges, need to delay this call

            return changes;
        };
    }

    public void AddNode(QuestSystemNode node, bool checkError = true)
    {
        m_nodes.Add(node);
        if (checkError)
            ProcessErrors();
    }

    public void RemoveNode(QuestSystemNode node, bool checkError = true)
    {
        m_nodes.Remove(node);
        if (checkError)
            ProcessErrors();
    }

    private void AddStartNode()
    {
        var node = CreateNode("Start", QuestSystemNodeType.Start, new Vector2(10, 10), false, false);
        node.Draw();
        AddNode(m_startNode);
        AddElement(node);
    }

    private void AddManipulators()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        this.AddManipulator(CreateNodeContextualMenu("Add Objective", QuestSystemNodeType.Objective));
        this.AddManipulator(CreateNodeContextualMenu("Add Complete", QuestSystemNodeType.Complete));
        this.AddManipulator(CreateNodeContextualMenu("Add Fail", QuestSystemNodeType.Fail));
    }

    private IManipulator CreateNodeContextualMenu(string actionTitle, QuestSystemNodeType dialogueType)
    {
        string name = "New Node";
        if (dialogueType == QuestSystemNodeType.Objective)
            name = "New Objective";
        else if (dialogueType == QuestSystemNodeType.Complete)
            name = "New Complete Node";
        else if (dialogueType == QuestSystemNodeType.Fail)
            name = "New Fail Node";
        else return null;

        ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(name, dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
        );

        return contextualMenuManipulator;
    }

    public QuestSystemNode CreateNode(string nodeName, QuestSystemNodeType dialogueType, Vector2 position, bool shouldDraw = true, bool addList = true)
    {
        Type nodeType = null;

        //todo
        if (dialogueType == QuestSystemNodeType.Start)
            nodeType = typeof(QuestSystemNodeStart);
        //else if (dialogueType == BSMNodeType.Condition)
        //    nodeType = typeof(BSMNodeCondition);
        //else if (dialogueType == BSMNodeType.Label)
        //    nodeType = typeof(BSMNodeLabel);
        //else if (dialogueType == BSMNodeType.Goto)
        //    nodeType = typeof(BSMNodeGoto);

        if (nodeType == null)
            return null;

        QuestSystemNode node = (QuestSystemNode)Activator.CreateInstance(nodeType);

        node.Initialize(nodeName, this, position);

        if (shouldDraw)
        {
            node.Draw();
        }

        if (addList)
            AddNode(node);

        return node;
    }

    public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
    {
        Vector2 worldMousePosition = mousePosition;

        if (isSearchWindow)
        {
            worldMousePosition = m_editorWindow.rootVisualElement.ChangeCoordinatesTo(m_editorWindow.rootVisualElement.parent, mousePosition - m_editorWindow.position.position);
        }

        Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

        return localMousePosition;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            //todo
            //if (startPort.node is BSMNodeLabel)
            //{
            //    var nodeLabel = startPort.node as BSMNodeLabel;

            //    if (nodeLabel.nodeType == BSMNodeLabelType.AnyState && !(port.node is BSMNodeCondition))
            //        return;

            //    if (nodeLabel.nodeType != BSMNodeLabelType.AnyState && !(port.node is BSMNodeState))
            //        return;
            //}

            //if (startPort.node is BSMNodeGoto && port.node is BSMNodeLabel)
            //    return;

            //if (startPort.node is BSMNodeState && port.node is BSMNodeState)
            //    return;

            //if (startPort.node is BSMNodeCondition && port.node is BSMNodeCondition)
            //    return;

            if (startPort == port)
                return;

            if (startPort.node == port.node)
                return;

            if (startPort.direction == port.direction)
                return;

            compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }

    public void ProcessErrors()
    {
        //todo
    }

    void SetEdgeStyle(Edge edge, bool error)
    {
        Color borderColor = new Color(.8f, .8f, .8f);

        if (error)
        {
            edge.style.borderLeftColor = QuestSystemNode.errorBorderColor;
            edge.style.borderBottomColor = QuestSystemNode.errorBorderColor;
            edge.style.borderRightColor = QuestSystemNode.errorBorderColor;
            edge.style.borderTopColor = QuestSystemNode.errorBorderColor;
        }
        else
        {
            edge.style.borderLeftColor = borderColor;
            edge.style.borderBottomColor = borderColor;
            edge.style.borderRightColor = borderColor;
            edge.style.borderTopColor = borderColor;
        }
    }

    void AddError(string error)
    {
        if (m_editorWindow != null)
            m_editorWindow.AddError(error, "Graph");
    }

    void ClearErrors()
    {
        if (m_editorWindow != null)
            m_editorWindow.ClearErrors("Graph");
    }

    void Clean()
    {
        foreach (var node in m_nodes)
            RemoveElement(node);

        var elements = edges.ToList();
        foreach (var element in elements)
        {
            if (element is Edge)
                RemoveElement(element as Edge);
        }

        m_nodes.Clear();
        
        m_startNode = null;
    }

    //todo
    //public void Load(BSMSaveData data)
    //{
    //    Clean();

    //    List<BSMNode> nodes = new List<BSMNode>();

    //    foreach (var dataNode in data.nodes)
    //    {
    //        var node = LoadNode(dataNode);
    //        nodes.Add(node);
    //    }

    //    foreach (var node in nodes)
    //    {
    //        node.Draw();
    //        AddElement(node);
    //        AddNode(node, false);
    //    }

    //    if (m_startNode == null)
    //        AddStartNode();

    //    if (m_anyStateNode == null)
    //        AddAnyStateNode();

    //    CreateConnexions(nodes, data.nodes);

    //    ProcessErrors();
    //}

    //todo
    //BSMNode LoadNode(BSMSaveNode data)
    //{
    //    BSMNode node = null;

    //    if (data.nodeType == BSMNodeType.Label)
    //    {
    //        var nodeLabel = new BSMNodeLabel();
    //        InitializeNode(nodeLabel, data);
    //        node = nodeLabel;
    //        if (data.name == "Start")
    //        {
    //            m_startNode = nodeLabel;
    //            m_startNode.nodeType = BSMNodeLabelType.Start;
    //        }
    //        else if (data.name == "Any State")
    //        {
    //            m_anyStateNode = nodeLabel;
    //            m_anyStateNode.nodeType = BSMNodeLabelType.AnyState;
    //        }
    //    }
    //    else if (data.nodeType == BSMNodeType.Goto)
    //    {
    //        var nodeGoto = new BSMNodeGoto();
    //        InitializeNode(nodeGoto, data);
    //        nodeGoto.SetLabelID(data.data as string);
    //        node = nodeGoto;
    //    }
    //    else if (data.nodeType == BSMNodeType.Condition)
    //    {
    //        var nodeConditon = new BSMNodeCondition();
    //        InitializeNode(nodeConditon, data);
    //        nodeConditon.SetCondition(data.data as BSMConditionBase);
    //        node = nodeConditon;
    //    }
    //    else if (data.nodeType == BSMNodeType.State)
    //    {
    //        var nodeState = new BSMNodeState();
    //        InitializeNode(nodeState, data);
    //        nodeState.SetState(data.data as BSMStateBase);
    //        node = nodeState;
    //    }

    //    return node;
    //}

    //todo
    //void InitializeNode(BSMNode node, BSMSaveNode data)
    //{
    //    node.ID = data.ID;
    //    node.NodeName = data.name;
    //    node.SetPosition(data.position);
    //    node.Initialize(data.name, this, data.position.position, false);
    //}

    //todo
    //void CreateConnexions(List<BSMNode> nodes, List<BSMSaveNode> datas)
    //{
    //    List<Edge> edges = new List<Edge>();

    //    foreach (var data in datas)
    //    {
    //        var node = GetFromID(nodes, data.ID);
    //        if (node == null)
    //            continue;
    //        foreach (var connexion in data.outNodes)
    //        {
    //            var outNode = GetFromID(nodes, connexion);
    //            if (outNode == null)
    //                continue;

    //            var edge = BSMEditorUtility.ConnectNodes(node, outNode);
    //            if (edge != null)
    //                edges.Add(edge);
    //        }
    //    }

    //    foreach (var edge in edges)
    //        AddElement(edge);
    //}

    static QuestSystemNode GetFromID(List<QuestSystemNode> nodes, string id)
    {
        foreach (var node in nodes)
        {
            if (node.ID == id)
                return node;
        }

        return null;
    }

    //todo
    //public void Save(BSMSaveData data)
    //{
    //    foreach (var node in m_nodes)
    //        data.nodes.Add(SaveNode(node));
    //}

    //todo
    //BSMSaveNode SaveNode(BSMNode node)
    //{
    //    BSMSaveNode data = new BSMSaveNode();

    //    data.ID = node.ID;
    //    data.name = node.NodeName;
    //    data.position = node.GetPosition();

    //    foreach (Port port in node.outputContainer.Children())
    //    {
    //        if (port == null)
    //            continue;

    //        foreach (var connexion in port.connections)
    //        {
    //            if (connexion.input == null)
    //                continue;
    //            if (connexion.input.node == null)
    //                continue;

    //            var nextNode = connexion.input.node as BSMNode;
    //            if (nextNode == null)
    //                continue;
    //            data.outNodes.Add(nextNode.ID);
    //        }
    //    }

    //    data.nodeType = BSMEditorUtility.GetType(node);

    //    if (data.nodeType == BSMNodeType.Condition)
    //    {
    //        var nodeCondition = node as BSMNodeCondition;
    //        if (nodeCondition != null)
    //            data.data = nodeCondition.GetCondition();
    //    }
    //    else if (data.nodeType == BSMNodeType.State)
    //    {
    //        var nodeState = node as BSMNodeState;
    //        if (nodeState != null)
    //            data.data = nodeState.GetState();
    //    }
    //    else if (data.nodeType == BSMNodeType.Goto)
    //    {
    //        var nodeGoto = node as BSMNodeGoto;
    //        if (nodeGoto != null)
    //            data.data = nodeGoto.GetLabelID();
    //    }

    //    return data;
    //}
}

