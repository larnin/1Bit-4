using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuestElementList : MonoBehaviour
{
    List<QuestElement> m_elements = new List<QuestElement>();
    List<NamedQuestObject> m_namedObjects = new List<NamedQuestObject>();

    static QuestElementList m_instance = null;
    public static QuestElementList instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    public void Register(NamedQuestObject element)
    {
        if(element is QuestElement)
            m_elements.Add(element as QuestElement);
        m_namedObjects.Add(element);
    }

    public void UnRegister(NamedQuestObject element)
    {
        if(element is QuestElement)
            m_elements.Remove(element as QuestElement);
        m_namedObjects.Remove(element);
    }

    public int GetElementNb()
    {
        return m_elements.Count;
    }

    public QuestElement GetElementFromIndex(int index)
    {
        if (index < 0 || index >= m_elements.Count)
            return null;

        return m_elements[index];
    }

    public int GetNamedObjectsNb()
    {
        return m_namedObjects.Count();
    }

    public NamedQuestObject GetNamedObjectFromIndex(int index)
    {
        if (index < 0 || index >= m_namedObjects.Count)
            return null;

        return m_namedObjects[index];
    }

    public List<NamedQuestObject> GetNamedObjectsByName(string name)
    {
        List<NamedQuestObject> elements = new List<NamedQuestObject>();

        foreach (var e in m_namedObjects)
        {
            if (e.GetName() == name)
                elements.Add(e);
        }

        return elements;
    }

    public NamedQuestObject GetFirstNamedObjectByName(string name)
    {
        foreach (var e in m_namedObjects)
        {
            if (e.GetName() == name)
                return e;
        }

        return null;
    }

    public void Clear()
    {
        //destroying elements can change the list
        var elements = m_elements.ToList();
        m_elements.Clear();

        foreach(var e in elements)
        {
            Destroy(e.gameObject);
        }
    }

    public void Load(JsonObject obj)
    {
        Clear();

        var jsonData = obj.GetElement("data");
        if (jsonData == null || !jsonData.IsJsonArray())
            return;

        var jsonArray = jsonData.JsonArray();
        foreach(var jsonElement in jsonArray)
        {
            if (jsonElement.IsJsonObject())
                QuestElement.Create(jsonElement.JsonObject());
        }
    }

    public JsonObject Save()
    {
        JsonObject obj = new JsonObject();

        var jsonArray = new JsonArray();
        obj.AddElement("data", jsonArray);

        foreach(var e in m_elements)
        {
            jsonArray.Add(e.Save());
        }

        return obj;
    }
}
