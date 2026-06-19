using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRand;
using UnityEngine;

public class GameInfos
{
    static GameInfos m_instance = null;

    public Settings settings = new Settings();
    public GameParams gameParams = new GameParams();
    public GamePersistant persistant = new GamePersistant();
    public bool paused = false;
    public int lastTip = -1;

    public static GameInfos instance
    {
        get
        {
            if (m_instance == null)
                m_instance = new GameInfos();
            return m_instance;
        }
    }
}

[Serializable]
public class Settings
{
    [SerializeField] float m_musicVolume = 1;
    [SerializeField] float m_soundVolume = 1;

    [SerializeField] float m_screenShakeIntensity = 1;

    [SerializeField] string m_colorName;
    [SerializeField] bool m_colorFlip = false;

    [SerializeField] bool m_inverseZoom = false;

    [SerializeField] bool m_displayMap = true;

    [SerializeField] bool m_fullScreen = true;

    public Settings()
    {

        if (Global.instance.colorsDatas.colors.Count > 0)
            m_colorName = Global.instance.colorsDatas.colors[0].name;

        m_fullScreen = Screen.fullScreen;

        //todo load settings
    }

    public void Save()
    {

    }

    public void SetMusicVolume(float value)
    {
        var mixer = Global.instance.soundsDatas.audioMixer;
        if (mixer == null)
            return;

        mixer.SetFloat("MusicVolume", GetMixerValueFromVolume(value));

        m_musicVolume = value;
    }

    public float GetMusicVolume()
    {
        return m_musicVolume;
    }

    public void SetSoundVolume(float value)
    {
        var mixer = Global.instance.soundsDatas.audioMixer;
        if (mixer == null)
            return;

        mixer.SetFloat("SoundVolume", GetMixerValueFromVolume(value));

        m_soundVolume = value;
    }

    public float GetSoundVolume()
    {
        return m_soundVolume;
    }

    float GetMixerValueFromVolume(float value)
    {
        value = Mathf.Sqrt(value);
        value = (value * 80) - 80;
        return value;
    }

    public void SetColorName(string name)
    {
        m_colorName = name;

        Event<SettingsColorChangedEvent>.Broadcast(new SettingsColorChangedEvent());
    }

    public void SetColorFlip(bool flip)
    {
        m_colorFlip = flip;

        Event<SettingsColorChangedEvent>.Broadcast(new SettingsColorChangedEvent());
    }

    public string GetColorName()
    {
        return m_colorName;
    }

    public bool GetColorFlip()
    {
        return m_colorFlip;
    }

    public void SetInverseZoom(bool zoom)
    {
        m_inverseZoom = zoom;
    }

    public bool IsInverseZoom()
    {
        return m_inverseZoom;
    }

    public void SetDisplayMap(bool display)
    {
        m_displayMap = display;

        Event<SettingsDisplayMapChangedEvent>.Broadcast(new SettingsDisplayMapChangedEvent());
    }

    public bool GetDisplayMap()
    {
        return m_displayMap;
    }

    public void SetFullScreen(bool fullScreen)
    {
        m_fullScreen = fullScreen;

        if(m_fullScreen)
        {
            var res = Screen.currentResolution;
            Screen.SetResolution(res.width, res.height, true);
        }
        else
        {
            Screen.fullScreen = false;

            var res = Screen.currentResolution;

            Screen.SetResolution(res.width / 2, res.height / 2, false);
        }
    }

    public bool GetFullScreen()
    {
        return m_fullScreen;
    }

    public void SetScreenShakeIntensity(float intensity)
    {
        m_screenShakeIntensity = intensity;
    }

    public float GetScreenShakeIntensity()
    {
        return m_screenShakeIntensity;
    }
}

public class GameParams
{
    public int seed;
    public string seedStr;
    public WorldSize worldSize;

    public LevelInfo level;
    public bool infiniteMode;

    public GameParams()
    {
        worldSize = WorldSize.Small;
        SetRandomSeed();

        level = null;
        infiniteMode = true;
    }

    public void SetRandomSeed()
    {
        seedStr = StaticRandomGenerator<MT19937>.Get().Next().ToString();
        seed = Cast.HashString(seedStr);
    }
}

public class GamePersistant
{
    List<BuildingType> m_unlockedBuilding = new List<BuildingType>();
    List<string> m_completedLevels = new List<string>();

    public GamePersistant()
    {
        foreach (var b in Global.instance.buildingDatas.defaultUnlockedBuildings)
            m_unlockedBuilding.Add(b);
    }

    public bool IsBuildingUnlocked(BuildingType type)
    {
        return m_unlockedBuilding.Contains(type);
    }

    public void SetBuildingUnlocked(BuildingType type, bool unlocked)
    {
        bool found = IsBuildingUnlocked(type);
        if (!found && unlocked)
            m_unlockedBuilding.Add(type);
        if (found && !unlocked)
            m_unlockedBuilding.Remove(type);
    }

    public bool IsLevelCompleted(string level)
    {
        return m_completedLevels.Contains(level);
    }

    public void SetLevelCompleted(string level)
    {
        if (IsLevelCompleted(level))
            return;

        m_completedLevels.Add(level);

        global::Save.instance.SaveCurrentSlot();
    }

    public void Load(JsonObject obj)
    {
        m_unlockedBuilding.Clear();
        m_completedLevels.Clear();

        var unlockedJson = obj.GetElement("unlockedBuilding");
        if(unlockedJson != null && unlockedJson.IsJsonArray())
        {
            var unlockedArray = unlockedJson.JsonArray();
            foreach(var uJson in unlockedArray)
            {
                if(uJson.IsJsonString())
                {
                    string buildingName = uJson.String();
                    BuildingType type;
                    if (Enum.TryParse(buildingName, out type))
                        m_unlockedBuilding.Add(type);
                }
            }
        }

        var completedJson = obj.GetElement("completedLevel");
        if(completedJson != null && completedJson.IsJsonArray())
        {
            var completedArray = completedJson.JsonArray();
            foreach(var cJson in completedArray)
            {
                if (cJson.IsJsonString())
                    m_completedLevels.Add(cJson.String());
            }
        }
    }

    public JsonObject Save()
    {
        JsonObject obj = new JsonObject();

        JsonArray unlockedArray = new JsonArray();
        obj.AddElement("unlockedBuilding", unlockedArray);
        foreach (var u in m_unlockedBuilding)
            unlockedArray.Add(u.ToString());

        JsonArray completedLevelArray = new JsonArray();
        obj.AddElement("completedLevel", completedLevelArray);
        foreach (var l in m_completedLevels)
            completedLevelArray.Add(l);

        return obj;
    }
}

