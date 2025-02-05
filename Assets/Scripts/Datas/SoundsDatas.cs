using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;
using UnityEngine.Audio;

[Serializable]
public class OneSoundData
{
    public string name;
    public List<AudioClip> clips;
}

[Serializable]
public class OneMusicData
{
    public string name;
    public AudioClip clip;
}

[Serializable]
public class SoundsDatas
{
    [SerializeField] List<OneSoundData> m_sounds;
    [SerializeField] List<OneMusicData> m_musics;
    public AudioMixer audioMixer;

    public AudioClip GetRandomSound(string name)
    {
        foreach(var s in m_sounds)
        {
            if(s.name == name)
            {
                if (s.clips.Count == 0)
                    return null;

                int index = Rand.UniformIntDistribution(0, s.clips.Count, StaticRandomGenerator<MT19937>.Get());
                return s.clips[index];
            }
        }

        return null;
    }

    public AudioClip GetMusic(string name)
    {
        foreach (var s in m_musics)
        {
            if (s.name == name)
                return s.clip;
        }

        return null;
    }

}

