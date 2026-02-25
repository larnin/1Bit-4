using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestSystemGraphView : GraphView
{
    QuestSystemGraph m_editorWindow;

    List<QuestSystemNode> m_nodes = new List<QuestSystemNode>();
    QuestSystemNodeStart m_startNode;

    bool m_lastPlaying = false;

    public QuestSystemGraphView(QuestSystemGraph editorWindow)
    {
        m_editorWindow = editorWindow;

        AddManipulators();
        AddGridBackground();
        AddStyles();

        OnElementsDeleted();
        OnGraphViewChanged();

        AddStartNode();

        schedule.Execute(UpdateSelection).Every(100);
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

    void UpdateSelection()
    {
        List<QuestSystemNode> nodes = new List<QuestSystemNode>();

        foreach(var s in selection)
        {
            var n = s as QuestSystemNode;

            if (n != null)
                nodes.Add(n);
        }

        m_editorWindow.SetCurrentNodes(nodes);
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
        m_startNode = node as QuestSystemNodeStart;
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

        if (dialogueType == QuestSystemNodeType.Start)
            nodeType = typeof(QuestSystemNodeStart);
        else if (dialogueType == QuestSystemNodeType.Complete)
            nodeType = typeof(QuestSystemNodeComplete);
        else if (dialogueType == QuestSystemNodeType.Fail)
            nodeType = typeof(QuestSystemNodeFail);
        else if (dialogueType == QuestSystemNodeType.Objective)
            nodeType = typeof(QuestSystemNodeObjective);

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


        node.RefreshExpandedState();

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
            if (startPort.node is QuestSystemNodeStart && (port.node is QuestSystemNodeFail || port.node is QuestSystemNodeComplete))
                return;

            if (port.node is QuestSystemNodeStart && (startPort.node is QuestSystemNodeFail || startPort.node is QuestSystemNodeComplete))
                return;

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

    class QuestSystemLoopNodeTest
    {
        public QuestSystemNode node;
        public List<KeyValuePair<QuestSystemNode, bool>> parentNodes = new List<KeyValuePair<QuestSystemNode, bool>>();

        public QuestSystemLoopNodeTest(QuestSystemNode _node) { node = _node; }
    }

    public void ProcessErrors()
    {
        ClearErrors();

        foreach (var node in m_nodes)
        {
            node.UpdateStyle(false);

            foreach (var edge in QuestSystemEditorUtility.GetAllOutEdge(node))
                SetEdgeStyle(edge, false);
        }

        for (int i = 0; i < m_nodes.Count; i++)
        {
            int namesError = 1;

            var node = m_nodes[i];
            for (int j = i + 1; j < m_nodes.Count; j++)
            {
                var node2 = m_nodes[j];

                if (node.NodeName == node2.NodeName)
                {
                    if (namesError == 1)
                        node.UpdateStyle(true);
                    node2.UpdateStyle(true);

                    namesError++;
                }
            }

            if (namesError > 1)
                AddError(namesError + " Nodes are named " + node.NodeName + ", names must be unique");

            foreach (var edge in QuestSystemEditorUtility.GetAllOutEdge(node))
            {
                SetEdgeStyle(edge, false);

                if (edge.input == null)
                    continue;
                if (edge.input.node == null)
                    continue;

                var nextNode = edge.input.node as QuestSystemNode;
                if (node == null)
                    continue;

                var inType = QuestSystemEditorUtility.GetType(node);
                var outType = QuestSystemEditorUtility.GetType(nextNode);

                if(inType == QuestSystemNodeType.Start && (outType == QuestSystemNodeType.Complete || outType == QuestSystemNodeType.Fail))
                {
                    AddError("Start node must be connected to a objective node");
                    SetEdgeStyle(edge, true);
                }
            }
        }

        //detect unwanted loops
        List<QuestSystemLoopNodeTest> loopNodesInfo = new List<QuestSystemLoopNodeTest>();
        foreach(var node in m_nodes)
            loopNodesInfo.Add(new QuestSystemLoopNodeTest(node));

        foreach (var node in m_nodes)
        {
            var edges = QuestSystemEditorUtility.GetAllOutEdge(node);
            foreach(var e in edges)
            {
                if (e.input == null)
                    continue;
                if (e.input.node == null)
                    continue;

                var nextNode = e.input.node as QuestSystemNode;
                if (nextNode == null)
                    continue;

                var loopNode = loopNodesInfo.Find(x => { return x.node == nextNode; });
                if (loopNode != null)
                    loopNode.parentNodes.Add(new KeyValuePair<QuestSystemNode, bool>(node, false));
            }
        }

        List<QuestSystemLoopNodeTest> openNodes = new List<QuestSystemLoopNodeTest>();
        openNodes.Add(new QuestSystemLoopNodeTest(m_startNode));

        while(openNodes.Count > 0)
        {
            QuestSystemLoopNodeTest current = null;
            foreach(var n in openNodes)
            {
                bool ok = true;
                foreach(var p in n.parentNodes)
                {
                    if(!p.Value)
                    {
                        ok = false;
                        break;
                    }
                }
                if(ok)
                {
                    current = n;
                    break;
                }    
            }

            if (current == null)
                break;

            openNodes.Remove(current);

            var edges = QuestSystemEditorUtility.GetAllOutEdge(current.node);
            foreach(var e in edges)
            {
                if (e.input == null)
                    continue;
                if (e.input.node == null)
                    continue;

                var nextNode = e.input.node as QuestSystemNode;
                if (nextNode == null)
                    continue;

                var loopNode = loopNodesInfo.Find(x => { return x.node == nextNode; });
                if (loopNode != null)
                {
                    bool alreadyFull = true;
                    foreach(var parentNode in loopNode.parentNodes)
                    {
                        if(!parentNode.Value)
                        {
                            alreadyFull = false;
                            break;
                        }
                    }

                    if (alreadyFull)
                        continue;

                    for(int i = 0; i < loopNode.parentNodes.Count; i++)
                    {
                        if(loopNode.parentNodes[i].Key == current.node)
                        {
                            loopNode.parentNodes[i] = new KeyValuePair<QuestSystemNode, bool>(current.node, true);

                            if (!openNodes.Exists(x => { return x.node == loopNode.node; }))
                                openNodes.Add(loopNode);
                        }    
                    }
                }
            }
        }

        foreach(var n in openNodes)
        {
            foreach(var parent in n.parentNodes)
            {
                if(!parent.Value)
                {
                    AddError("Loops have been detected between nodes " + n.node.NodeName + " and " + parent.Key.NodeName);

                    n.node.UpdateStyle(true);
                    parent.Key.UpdateStyle(true);
                }
            }
        }
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

    public void Load(QuestSaveData data)
    {
        Clean();

        List<QuestSystemNode> nodes = new List<QuestSystemNode>();

        foreach(var dataNode in data.nodes)
        {
            var node = LoadNode(dataNode);
            nodes.Add(node);
        }

        foreach(var node in nodes)
        {
            node.Draw();
            AddElement(node);
            AddNode(node, false);
            node.RefreshExpandedState();
        }

        if (m_startNode == null)
            AddStartNode();

        CreateConnections(nodes, data.nodes);

        ProcessErrors();
    }
    
    QuestSystemNode LoadNode(QuestSaveNode data)
    {
        QuestSystemNode node = null;

        if(data.nodeType == QuestSystemNodeType.Start)
        {
            var nodeStart = new QuestSystemNodeStart();
            InitializeNode(nodeStart, data);
            m_startNode = nodeStart;
            node = nodeStart;
        }
        else if(data.nodeType == QuestSystemNodeType.Objective)
        {
            var nodeObjective = new QuestSystemNodeObjective();
            InitializeNode(nodeObjective, data);
            nodeObjective.SetObjective(data.data as QuestObjective);
            node = nodeObjective;
        }
        else if(data.nodeType == QuestSystemNodeType.Complete)
        {
            node = new QuestSystemNodeComplete();
            InitializeNode(node, data);
        }
        else if(data.nodeType == QuestSystemNodeType.Fail)
        {
            node = new QuestSystemNodeFail();
            InitializeNode(node, data);
        }

        return node;
    }

    void InitializeNode(QuestSystemNode node, QuestSaveNode data)
    {
        node.ID = data.ID;
        node.NodeName = data.name;
        node.SetPosition(data.position);
        node.Initialize(data.name, this, data.position.position, false);
    }

    void CreateConnections(List<QuestSystemNode> nodes, List<QuestSaveNode> datas)
    {
        List<Edge> edges = new List<Edge>();
        
        foreach(var data in datas)
        {
            var node = GetFromID(nodes, data.ID);
            if (node == null)
                continue;

            foreach(var connexion in data.outNodes)
            {
                var outNode = GetFromID(nodes, connexion.nextNodeID);
                if (outNode == null)
                    continue;

                var edge = QuestSystemEditorUtility.ConnectNodes(node, connexion.currentPortName, outNode, connexion.nextPortName);
                if (edge != null)
                    edges.Add(edge);
            }
        }

        foreach (var edge in edges)
            AddElement(edge);
    }

    static QuestSystemNode GetFromID(List<QuestSystemNode> nodes, string id)
    {
        foreach (var node in nodes)
        {
            if (node.ID == id)
                return node;
        }

        return null;
    }

    public void Save(QuestSaveData data)
    {
        foreach (var node in m_nodes)
            data.nodes.Add(SaveNode(node));
    }

    QuestSaveNode SaveNode(QuestSystemNode node)
    {
        QuestSaveNode data = new QuestSaveNode();

        data.ID = node.ID;
        data.name = node.NodeName;
        data.position = node.GetPosition();

        foreach(Port port in node.outputContainer.Children())
        {
            if (port == null)
                continue;

            foreach(var connexion in port.connections)
            {
                if (connexion.input == null || connexion.input.node == null)
                    continue;

                var nextNode = connexion.input.node as QuestSystemNode;
                if (nextNode == null)
                    continue;

                var saveConnection = new QuestSaveConnection();
                saveConnection.currentPortName = port.portName;
                saveConnection.nextPortName = connexion.input.portName;
                saveConnection.nextNodeID = nextNode.ID;
                data.outNodes.Add(saveConnection);
            }
        }

        data.nodeType = QuestSystemEditorUtility.GetType(node);

        if(data.nodeType == QuestSystemNodeType.Objective)
        {
            var nodeObjective = node as QuestSystemNodeObjective;
            if (nodeObjective != null)
                data.data = nodeObjective.GetObjective();
        }

        return data;
    }

    public void UpdatePlaying(bool playing)
    {
        if (!m_lastPlaying && !playing)
            return;

        m_lastPlaying = playing;

        if (!playing)
        {
            foreach (var node in m_nodes)
                node.UpdateDisplayState(QuestSystemNodeDisplayState.Default);

            return;
        }

        if (QuestSystem.instance == null)
            return;

        string currentQuest = "";

        var questNames = QuestSystem.instance.GetActiveQuestsNames();
        questNames.AddRange(QuestSystem.instance.GetCompletedQuestNames());
        foreach (var name in questNames)
        {
            var obj = QuestSystem.instance.GetQuestObject(name);
            var path = AssetDatabase.GetAssetPath(obj);
            if (path == m_editorWindow.GetSavePath())
            {
                currentQuest = name;
                break;
            }
        }

        foreach (var node in m_nodes)
        {
            var obj = node as QuestSystemNodeObjective;
            if (obj == null)
                continue;

            QuestObjectiveCompletionType completion = QuestSystem.instance.GetQuestObjectiveStatus(currentQuest, obj.NodeName);
            switch(completion)
            {
                case QuestObjectiveCompletionType.Completed:
                    obj.UpdateDisplayState(QuestSystemNodeDisplayState.Completed);
                    break;
                case QuestObjectiveCompletionType.Ongoing:
                    obj.UpdateDisplayState(QuestSystemNodeDisplayState.Ongoing);
                    break;
                case QuestObjectiveCompletionType.NotStarted:
                default:
                    obj.UpdateDisplayState(QuestSystemNodeDisplayState.Default);
                    break;
            }
        }
    }
}

