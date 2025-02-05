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

    public Settings()
    {
        //todo load settings


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
}

public class GameParams
{
    public int seed;
    public WorldSize worldSize;

    public GameParams()
    {
        worldSize = WorldSize.Small;
        seed = (int)StaticRandomGenerator<MT19937>.Get().Next();
    }
}

