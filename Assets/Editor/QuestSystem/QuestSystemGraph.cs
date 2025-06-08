using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestSystemGraph : EditorWindow
{
    QuestSystemGraphView m_graphView;
    QuestSystemErrorWindow m_errorWindow;
    QuestSystemDetailWindow m_detailWindow;

    Label m_nameLabel;

    string m_savePath;

    [MenuItem("Game/Quest System")]
    public static QuestSystemGraph Open()
    {
        return GetWindow<QuestSystemGraph>("Quest System");
    }

    public QuestSystemGraphView GetGraphView()
    {
        return m_graphView;
    }

    private void OnEnable()
    {
        AddGraphView();

        AddMenusWindows();

        AddStyles();
    }

    private void AddGraphView()
    {
        m_graphView = new QuestSystemGraphView(this);

        m_graphView.StretchToParentSize();

        VisualElement horizontal = QuestSystemEditorUtility.CreateHorizontalLayout();
        horizontal.style.flexGrow = 2;

        VisualElement element = new VisualElement();
        element.style.minHeight = new StyleLength(new Length(50, LengthUnit.Percent));
        element.style.flexGrow = 2;
        element.Add(m_graphView);


        m_nameLabel = new Label();
        element.Add(m_nameLabel);
        UpdateLabel();

        horizontal.Add(element);

        VisualElement sideMenu = new VisualElement();
        sideMenu.style.width = 250;

        m_detailWindow = new QuestSystemDetailWindow();
        m_detailWindow.SetParent(sideMenu);
        horizontal.Add(sideMenu);

        rootVisualElement.Add(horizontal);
    }
    private void AddStyles()
    {
        rootVisualElement.AddStyleSheets("QuestSystem/QuestSystemVariables.uss");
    }

    void AddMenusWindows()
    {
        VisualElement baseWindow = QuestSystemEditorUtility.CreateHorizontalLayout();
        baseWindow.style.height = new StyleLength(new Length(90, LengthUnit.Pixel));

        VisualElement menuWindow = new VisualElement();
        menuWindow.style.width = 120;
        baseWindow.Add(menuWindow);

        menuWindow.Add(QuestSystemEditorUtility.CreateButton("New", NewFile));
        menuWindow.Add(QuestSystemEditorUtility.CreateButton("Load", Load));
        menuWindow.Add(QuestSystemEditorUtility.CreateButton("Save", SaveChanges));
        menuWindow.Add(QuestSystemEditorUtility.CreateButton("Save As", SaveAs));

        if (m_errorWindow == null)
            m_errorWindow = new QuestSystemErrorWindow();

        VisualElement errorElement = new VisualElement();
        m_errorWindow.SetParent(errorElement);
        errorElement.style.flexGrow = 2;

        baseWindow.Add(errorElement);
        rootVisualElement.Add(baseWindow);
    }

    public void AddError(string error, string source)
    {
        if (m_errorWindow != null)
            m_errorWindow.AddError(error, source);
    }

    public void ClearErrors(string source = null)
    {
        if (m_errorWindow != null)
            m_errorWindow.ClearErrors(source);
    }

    public void SetCurrentNodes(List<QuestSystemNode> nodes)
    {
        if (m_detailWindow != null)
            m_detailWindow.SetNodes(nodes);
    }

    void Save(string path)
    {
        QuestSaveData saveData = new QuestSaveData();

        m_graphView.Save(saveData);

        var obj = AssetDatabase.LoadAssetAtPath<QuestScriptableObject>(path);
        if(obj == null)
        {
            obj = ScriptableObject.CreateInstance<QuestScriptableObject>();
            obj.data = saveData;
            AssetDatabase.CreateAsset(obj, path);
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }
        else
        {
            obj.data = saveData;
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }
    }

    void Load(string path)
    {
        QuestSaveData saveData = new QuestSaveData();

        var obj = AssetDatabase.LoadAssetAtPath<QuestScriptableObject>(path);
        if (obj != null)
            saveData = obj.data;

        m_graphView.Load(saveData);
    }

    public override void SaveChanges()
    {
        base.SaveChanges();

        if (m_savePath == null || m_savePath.Length == 0)
            GetSavePath();

        if (m_savePath == null || m_savePath.Length == 0)
            return;

        Save(m_savePath);
    }

    void GetSavePath()
    {
        string savePath = SaveEx.GetSaveFilePath("Save Behavior", Application.dataPath, "asset");
        if (savePath == null || savePath.Length == 0)
            return;

        m_savePath = SaveEx.GetRelativeAssetPath(savePath);

        UpdateLabel();
    }

    public void SaveAs()
    {
        GetSavePath();
        if (m_savePath == null || m_savePath.Length == 0)
            return;

        SaveChanges();
    }

    public void Load()
    {
        string loadPath = SaveEx.GetLoadFiltPath("Load Behavior", Application.dataPath, "asset");
        if (loadPath == null || loadPath.Length == 0)
            return;

        m_savePath = SaveEx.GetRelativeAssetPath(loadPath);
        Load(m_savePath);

        UpdateLabel();
    }

    public void NewFile()
    {
        var saveData = new QuestSaveData();

        m_graphView.Load(saveData);

        m_savePath = "";

        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (m_nameLabel != null)
        {
            if (m_savePath == null)
                m_nameLabel.text = "";
            else
            {
                int index = m_savePath.IndexOf("Assets");
                if (index != -1)
                    m_nameLabel.text = m_savePath.Substring(index);
                else m_nameLabel.text = m_savePath;
            }
        }
    }
}
