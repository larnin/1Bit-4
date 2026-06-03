using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Save
{
    public const int maxSaveSlots = 3;

    static Save m_instance = null;
    public static Save instance
    {
        get
        {
            if (m_instance == null)
                m_instance = new Save();
            return m_instance;
        }
    }

    SaveGlobal m_saveGlobal = new SaveGlobal();
    SaveHeader[] m_saveHeaders = new SaveHeader[maxSaveSlots];
    int m_currentSlot = -1;

    Save()
    {
        LoadGlobal();
        LoadHeaders();
    }

    public void LoadHeaders()
    {
        for (int i = 0; i < maxSaveSlots; i++)
            LoadHeader(i);
    }

    void LoadHeader(int index)
    {
        if (index < 0 || index >= maxSaveSlots)
            return;

        string headerPath = GetHeaderPath(index);

        var doc = Json.ReadFromFile(headerPath);

        var header = new SaveHeader();
        if(doc != null)
            header.Load(doc);

        m_saveHeaders[index] = header;
    }

    void SaveHeader(int index)
    {
        if (index < 0 || index >= maxSaveSlots)
            return;

        var doc = m_saveHeaders[index].Save();

        string headerPath = GetHeaderPath(index);

        Json.WriteToFile(headerPath, doc);
    }

    public SaveHeader GetHeader(int index)
    {
        if (index < 0 || index >= maxSaveSlots)
            return null;
        return m_saveHeaders[index];
    }

    public SaveGlobal GetGlobal()
    {
        return m_saveGlobal;
    }

    string GetHeaderPath(int index)
    {
        return GetSavePath(index) + "Header.sav";
    }

    string GetGamePath(int index)
    {
        return GetSavePath(index) + "Game.sav";
    }

    string GetGlobalPath()
    {
        return Application.persistentDataPath + "\\Save\\";
    }

    public string GetSavePath(int index)
    {
        string basePath = GetGlobalPath();

        if(index < 0)
            return basePath + "Default\\";

        return basePath + "Slot_" + index + "\\";
    }

    void LoadGlobal()
    {
        string path = GetGlobalPath();

        var doc = Json.ReadFromFile(path);

        if (doc != null)
            m_saveGlobal.Load(doc);
    }

    public void SaveGlobal()
    {
        var doc = m_saveGlobal.Save();
        
        string path = GetGlobalPath();

        Json.WriteToFile(path, doc);
    }

    public void SelectSaveSlot(int index)
    {
        m_currentSlot = index;
    }

    public int GetCurrentSlot()
    {
        return m_currentSlot;
    }

    public void LoadCurrentSlot()
    {
        if (m_currentSlot < 0 || m_currentSlot > maxSaveSlots)
            return;

        LoadHeader(m_currentSlot);

        LoadGame(m_currentSlot);

    }

    public void SaveCurrentSlot()
    {
        if (m_currentSlot < 0 || m_currentSlot > maxSaveSlots)
            return;

        SaveHeader(m_currentSlot);

        SaveGame(m_currentSlot);

        SaveGlobal();
    }

    public void DeleteSave(int index)
    {
        if (index < 0 || index > maxSaveSlots)
            return;

        m_saveHeaders[index] = new SaveHeader();

        string path = GetHeaderPath(index);
        SaveEx.DeleteFile(path);

        path = GetSavePath(index);
        SaveEx.DeleteFile(path);

        LoadHeader(index);
    }

    void LoadGame(int index)
    {
        string path = GetGamePath(index);
        var doc = Json.ReadFromFile(path);
        if(doc != null)
        {
            var rootJson = doc.GetRoot();
            if(rootJson != null && rootJson.IsJsonObject())
            {
                var rootObj = rootJson.JsonObject();
                var questsJson = rootObj.GetElement("quests");
                if (questsJson != null && questsJson.IsJsonObject() && QuestSystem.instance != null)
                    QuestSystem.instance.LoadGlobalQuests(questsJson.JsonObject());

                var persistantJson = rootObj.GetElement("persistant");
                if (persistantJson != null && persistantJson.IsJsonObject())
                    GameInfos.instance.persistant.Load(persistantJson.JsonObject());
            }
        }
    }

    void SaveGame(int index)
    {
        var doc = new JsonDocument();
        var root = new JsonObject();
        doc.SetRoot(root);

        if (QuestSystem.instance != null)
            root.AddElement("quests", QuestSystem.instance.SaveGlobalQuests());

        root.AddElement("persistant", GameInfos.instance.persistant.Save());

        string path = GetGamePath(index);

        Json.WriteToFile(path, doc);
    }
}
