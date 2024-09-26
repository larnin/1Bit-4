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

    SaveHeader[] m_saveHeaders = new SaveHeader[maxSaveSlots];
    int m_currentSlot = -1;

    Save()
    {
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

    string GetHeaderPath(int index)
    {
        return GetSavePath(index) + "Header.sav";
    }

    public string GetSavePath(int index)
    {
        if(index < 0)
            return Application.persistentDataPath + "\\Default\\";

        return Application.persistentDataPath + "\\Slot_" + index + "\\";
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

        //todo load others files
    }

    public void SaveCurrentSlot()
    {
        if (m_currentSlot < 0 || m_currentSlot > maxSaveSlots)
            return;

        SaveHeader(m_currentSlot);

        //todo save others files
    }

    public void DeleteSave(int index)
    {
        if (index < 0 || index > maxSaveSlots)
            return;

        m_saveHeaders[index] = new SaveHeader();

        string path = GetHeaderPath(index);
        SaveEx.DeleteFile(path);
    }
}
