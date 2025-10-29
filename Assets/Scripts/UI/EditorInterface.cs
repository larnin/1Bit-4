using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

public class EditorInterface : MonoBehaviour
{
    [SerializeField] string m_menuName;

    SubscriberList m_subscriberList = new SubscriberList();

    string m_currentPath = ""; 
    bool m_selected = false;

    private void Awake()
    {
        m_subscriberList.Add(new Event<IsMouseOverUIEvent>.Subscriber(IsMouseOverUI));
        m_subscriberList.Add(new Event<EditorSystemButtonClickedEvent>.Subscriber(OnEditorSystemButtonClick));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        UpdateFilename();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            Undo();

        if (Input.GetKeyDown(KeyCode.Y) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            Redo();
    }

    void IsMouseOverUI(IsMouseOverUIEvent e)
    {
        e.overUI = false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);

        for (int index = 0; index < raysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = raysastResults[index];

            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                e.overUI = true;
                break;
            }
        }
    }

    public void OnSystemClick()
    {
        Event<ToggleEditorToolCategoryEvent>.Broadcast(new ToggleEditorToolCategoryEvent(EditorToolCategoryType.System));
    }

    public void OnEntitiesClick()
    {
        Event<ToggleEditorToolCategoryEvent>.Broadcast(new ToggleEditorToolCategoryEvent(EditorToolCategoryType.Entities));
    }

    public void OnTerraformationClick()
    {
        Event<ToggleEditorToolCategoryEvent>.Broadcast(new ToggleEditorToolCategoryEvent(EditorToolCategoryType.Terraformation));
    }

    public void OnGenerationClick()
    {
        Event<ToggleEditorToolCategoryEvent>.Broadcast(new ToggleEditorToolCategoryEvent(EditorToolCategoryType.Generation));
    }

    void OnEditorSystemButtonClick(EditorSystemButtonClickedEvent e)
    {
        switch(e.button)
        {
            case EditorSystemButtonType.New:
                New();
                break;
            case EditorSystemButtonType.Load:
                Load();
                break;
            case EditorSystemButtonType.Save:
                Save();
                break;
            case EditorSystemButtonType.SaveAs:
                SaveAs();
                break;
            case EditorSystemButtonType.Undo:
                Undo();
                break;
            case EditorSystemButtonType.Redo:
                Redo();
                break;
            case EditorSystemButtonType.Exit:
                Exit();
                break;
            default:
                break;
        }
    }

    void New()
    {
        if (m_currentPath.Length > 0)
        {
#if UNITY_EDITOR
            if (!EditorUtility.DisplayDialog("New Level", "Create a new Level ?\nUnsaved changes can be lost", "Yes", "No"))
                return;
#endif
        }

        m_currentPath = "";
        SaveWorld.EditorReset();
        if (UndoList.instance != null)
            UndoList.instance.Clear();
    }

    void Load()
    {
        if(m_currentPath.Length > 0)
        {
#if UNITY_EDITOR
            if (!EditorUtility.DisplayDialog("Open Level", "Open an other Level ?\nUnsaved changes can be lost", "Yes", "No"))
                return;
#endif
        }

        string path = SaveEx.GetLoadFiltPath("Load world", m_currentPath, "asset");
        if (path.Length == 0)
            return;

        m_currentPath = path;

        SaveWorld.EditorReset();

        UpdateFilename();

        JsonDocument doc = null;

#if UNITY_EDITOR
        string relativePath = SaveEx.GetRelativeAssetPath(m_currentPath);
        if (relativePath != m_currentPath)
        {
            doc = SaveEx.LoadFromEditor(relativePath);
        }
        else
#endif
        {
            doc = Json.ReadFromFile(m_currentPath);
        }

        if (doc == null)
            return;

        var root = doc.GetRoot();
        if (root == null || !root.IsJsonObject())
            return;

        SaveWorld.Load(root.JsonObject());
        if (UndoList.instance != null)
            UndoList.instance.Clear();
    }

    void Save()
    {
        if (m_currentPath.Length == 0)
        {
            SaveAs();
            return;
        }

        JsonDocument doc = new JsonDocument();
        doc.SetRoot(new JsonObject());

        var data = SaveWorld.Save();
        doc.SetRoot(data);

#if UNITY_EDITOR
        string relativePath = SaveEx.GetRelativeAssetPath(m_currentPath);
        if (relativePath != m_currentPath)
        {
            SaveEx.SaveFromEditor(relativePath, doc);
        }
        else
#endif
        {
            Json.WriteToFile(m_currentPath, doc);
        }
    }

    void SaveAs()
    {
        string path = SaveEx.GetSaveFilePath("Save world", m_currentPath, "asset");
        if (path.Length == 0)
            return;

        m_currentPath = path;

        UpdateFilename();

        Save();
    }

    void Undo()
    {
        if (UndoList.instance != null)
            UndoList.instance.Undo();
    }

    void Redo()
    {
        if (UndoList.instance != null)
            UndoList.instance.Redo();
    }

    void Exit()
    {
        if (m_selected)
            return;

        m_selected = true;

        SceneSystem.changeScene(new ChangeSceneParams(m_menuName));
    }

    void UpdateFilename()
    {
        //todo update filename

        if (m_currentPath.Length == 0)
        {
            //m_filename.text = "New Level";
            return;
        }

        string filename = "";
        int posSlash = m_currentPath.LastIndexOfAny(new char[] { '/', '\\' });
        if (posSlash >= 0)
            filename = m_currentPath.Substring(posSlash + 1);
        else String.Copy(m_currentPath);

        int posDot = filename.LastIndexOf('.');
        if (posDot > 0)
            filename = filename.Substring(0, posDot);

        //m_filename.text = filename;
    }
}
