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
        //todo
        //AddSideMenus(horizontal);

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

        //todo
        //if (m_errorWindow == null)
        //    m_errorWindow = new BSMErrorWindow();

        //VisualElement errorElement = new VisualElement();
        //m_errorWindow.SetParent(errorElement);
        //errorElement.style.flexGrow = 2;

        //baseWindow.Add(errorElement);
        rootVisualElement.Add(baseWindow);
    }

    public void AddError(string error, string source)
    {
        //todo
        //if (m_errorWindow != null)
        //    m_errorWindow.AddError(error, source);
    }

    public void ClearErrors(string source = null)
    {
        //todo
        //if (m_errorWindow != null)
        //    m_errorWindow.ClearErrors(source);
    }

    void Save(string path)
    {
        //todo
        //BSMSaveData saveData = new BSMSaveData();

        //m_graphView.Save(saveData);
        //m_attributesWindow.Save(saveData);

        //var obj = AssetDatabase.LoadAssetAtPath<BSMScriptableObject>(path);
        //if (obj == null)
        //{
        //    obj = ScriptableObject.CreateInstance<BSMScriptableObject>();
        //    obj.data = saveData;
        //    AssetDatabase.CreateAsset(obj, path);
        //    EditorUtility.SetDirty(obj);
        //    AssetDatabase.SaveAssets();
        //}
        //else
        //{
        //    obj.data = saveData;
        //    EditorUtility.SetDirty(obj);
        //    AssetDatabase.SaveAssets();
        //}
    }

    void Load(string path)
    {
        //todo
        //BSMSaveData saveData = new BSMSaveData();

        //var obj = AssetDatabase.LoadAssetAtPath<BSMScriptableObject>(path);
        //if (obj != null)
        //    saveData = obj.data;

        //m_attributesWindow.Load(saveData);
        //m_graphView.Load(saveData);
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
        //todo
        //var saveData = new BSMSaveData();

        //m_attributesWindow.Load(saveData);
        //m_graphView.Load(saveData);

        //m_savePath = "";

        //UpdateLabel();
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
