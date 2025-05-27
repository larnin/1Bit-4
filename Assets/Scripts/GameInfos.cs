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
}

public class GameParams
{
    public int seed;
    public string seedStr;
    public WorldSize worldSize;

    public GameParams()
    {
        worldSize = WorldSize.Medium;
        seedStr = StaticRandomGenerator<MT19937>.Get().Next().ToString();
        seed = Cast.HashString(seedStr);
    }
}

