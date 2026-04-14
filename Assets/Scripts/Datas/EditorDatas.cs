using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class EntityChoice : ChoiceString
{
    protected override List<string> GetChoices()
    {
        List<string> choices = new List<string>();
        foreach(var e in Global.instance.editorDatas.entities)
        {
            choices.Add(e.type);
        }

        return choices;
    }
}

[Serializable]
public class NavigationProfileChoice : ChoiceString
{
    protected override List<string> GetChoices()
    {
        List<string> choices = new List<string>();
        foreach (var e in Global.instance.editorDatas.navigationProfiles)
        {
            choices.Add(e.name);
        }

        return choices;
    }
}

[Serializable]
public class ProjectileChoice : ChoiceString
{
    protected override List<string> GetChoices()
    {
        List<string> choices = new List<string>();
        foreach (var e in Global.instance.editorDatas.projectiles)
        {
            choices.Add(e.type);
        }

        return choices;
    }
}

[Serializable]
public class GameEntityData
{
    public string type;
    public GameObject prefab;
    public NavigationProfileChoice navigationProfile;
}

[Serializable]
public class NavigationData
{
    public string name;
    public NavigationProfile profile;
}

[Serializable]
public class ProjectileData
{
    public string type;
    public GameObject prefab;
}

[Serializable]
public class QuestElementData
{
    public QuestElementType type;
    public GameObject prefab;
}

[Serializable]
public class EditorDatas
{
    public string editorLayer;
    public Material cursorMaterial;
    public LayerMask toolHoverLayer;

    public LayerMask groundLayer;

    public Material questElementMaterial;

    public List<GameEntityData> entities;
    public List<NavigationData> navigationProfiles;
    public List<ProjectileData> projectiles;
    public List<QuestElementData> questElements;

    public GameObject GetQuestElementPrefab(QuestElementType type)
    {
        foreach(var e in questElements)
        {
            if (e.type == type)
                return e.prefab;
        }

        return null;
    }

    public GameObject GetEntityPrefab(string type)
    {
        int index = GetEntityIndex(type);
        if (index < 0)
            return null;

        return entities[index].prefab;
    }

    public NavigationProfile GetEntityNavigationProfile(string type)
    {
        int index = GetEntityIndex(type);
        if (index < 0)
            return null;

        return GetNavigationProfile(entities[index].navigationProfile.GetValue());
    }

    public int GetEntityIndex(string type)
    {
        for(int i = 0; i < entities.Count; i++)
        {
            if (entities[i].type == type)
                return i;
        }

        return -1;
    }

    public NavigationProfile GetNavigationProfile(string name)
    {
        foreach (var n in navigationProfiles)
        {
            if (n.name == name)
                return n.profile;
        }

        return null;
    }

    public GameObject GetProjectilePrefab(string type)
    {
        foreach(var e in projectiles)
        {
            if (e.type == type)
                return e.prefab;
        }

        return null;
    }
}
