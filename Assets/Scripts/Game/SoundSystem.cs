using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SoundSystem : MonoBehaviour
{
    [SerializeField] GameObject m_soundPrefab;
    [SerializeField] GameObject m_musicPrefab;
    [SerializeField] int m_soundTrackNb = 10;
    [SerializeField] int m_maxSoundsOfSameType = 2;
    [SerializeField] float m_musicTransitionDuration = 2;

    class PlayingSound
    {
        public string name;
        public int ID;
        public bool isLoop;
        public float startTime;
        public float volume;
        public AudioSource source;
    }

    List<AudioSource> m_freeSoundSource = new List<AudioSource>();
    List<PlayingSound> m_playingSounds = new List<PlayingSound>();
    int m_nextID = 0;

    List<PlayingSound> m_musicSources = new List<PlayingSound>();
    string m_musicName;
    int m_musicSourceIndex = -1;
    int m_musicLastSourceIndex = -1;
    float m_musicTransitionTime = -1;

    private void Awake()
    {
        CreateSources();
    }

    void CreateSources()
    {
        if(m_soundPrefab != null)
        {
            for(int i = 0; i < m_soundTrackNb; i++)
            {
                var obj = Instantiate(m_soundPrefab);
                obj.transform.parent = transform;
                obj.transform.localPosition = Vector3.zero;

                var source = obj.GetComponent<AudioSource>();
                if(source == null)
                {
                    Destroy(obj);
                    break;
                }

                source.playOnAwake = false;
                m_freeSoundSource.Add(source);
            }
        }

        if(m_musicPrefab != null)
        {
            for(int i = 0; i < 2; i++)
            {
                var obj = Instantiate(m_musicPrefab);
                obj.transform.parent = transform;
                obj.transform.localPosition = Vector3.zero;

                var source = obj.GetComponent<AudioSource>();
                if(source == null)
                {
                    Destroy(obj);
                    break;
                }

                source.playOnAwake = false;
                PlayingSound music = new PlayingSound();
                music.source = source;
                m_musicSources.Add(music);
            }
        }
    }

    private void Update()
    {
        for(int i = 0; i < m_playingSounds.Count; i++)
        {
            var s = m_playingSounds[i];
            if (!s.isLoop && !s.source.isPlaying)
            {
                StopSoundInternal(i);
                i--;
            }
        }

        UpdateMusicTransition();
    }

    void UpdateMusicTransition()
    {
        if(m_musicTransitionDuration >= 0)
        {
            m_musicTransitionTime += Time.deltaTime;

            float normTime = m_musicTransitionTime / m_musicTransitionDuration;
            if(normTime >= 1)
            {
                normTime = 1;
                m_musicTransitionTime = -1;

                if(m_musicLastSourceIndex >= 0 && m_musicSources.Count > m_musicLastSourceIndex)
                {
                    m_musicSources[m_musicLastSourceIndex].source.Stop();
                    m_musicSources[m_musicLastSourceIndex].source.clip = null;
                }
                m_musicLastSourceIndex = -1;
            }

            if(m_musicSourceIndex >= 0 && m_musicSources.Count > m_musicSourceIndex)
            {
                float v = m_musicSources[m_musicSourceIndex].volume * normTime;
                m_musicSources[m_musicSourceIndex].source.volume = v;
            }

            if(m_musicLastSourceIndex >= 0 && m_musicSources.Count > m_musicLastSourceIndex)
            {
                float v = m_musicSources[m_musicLastSourceIndex].volume * (1 - normTime);
                m_musicSources[m_musicLastSourceIndex].source.volume = v;
            }
        }
    }

    public int PlaySound(string name, Vector3 pos, float volume = 1, bool loop = false, bool spatialize = true)
    {
        return PlaySoundInternal(name, false, pos, volume, loop, spatialize);
    }

    public int PlaySoundUI(string name, float volume = 1, bool loop = false)
    {
        return PlaySoundInternal(name, true, Vector3.zero, volume, loop, false);
    }

    int PlaySoundInternal(string name, bool fromUI, Vector3 pos, float volume, bool loop, bool spatialize)
    {
        if (m_freeSoundSource.Count == 0)
            return -1;

        var clip = Global.instance.soundsDatas.GetRandomSound(name);
        if (clip == null)
            return -1;

        int nb = GetSoundNb(name);
        if (nb >= m_maxSoundsOfSameType)
            StopOldestSound(name);

        PlayingSound s = new PlayingSound();
        s.ID = m_nextID;
        m_nextID++;
        s.name = name;
        s.startTime = Time.time;
        s.volume = volume;
        s.source = m_freeSoundSource[m_freeSoundSource.Count - 1];
        m_freeSoundSource.RemoveAt(m_freeSoundSource.Count - 1);
        s.source.clip = clip;
        s.source.loop = loop;
        if (fromUI)
            s.source.transform.localPosition = Vector3.zero;
        else s.source.transform.position = pos;
        s.source.volume = volume;
        s.source.spatialize = spatialize;
        s.source.time = 0;
        s.source.Play();

        m_playingSounds.Add(s);

        return s.ID;
    }

    public int GetSoundNb(string name)
    {
        int nb = 0;
        foreach(var s in m_playingSounds)
        {
            if (s.name == name)
                nb++;
        }

        return nb;
    }

    public bool IsSoundPlaying(int ID)
    {
        foreach(var s in m_playingSounds)
        {
            if (s.ID == ID)
                return true;
        }

        return false;
    }

    public void StopSound(int ID)
    {
        for(int i = 0; i < m_playingSounds.Count; i++)
        {
            if(m_playingSounds[i].ID == ID)
            {
                StopSoundInternal(i);
                return;
            }
        }
    }

    public void StopAllSound(string name)
    {
        for (int i = 0; i < m_playingSounds.Count; i++)
        {
            if (m_playingSounds[i].name == name)
            {
                StopSoundInternal(i);
                i--;
            }
        }
    }

    public void StopOldestSound(string name)
    {
        int oldestIndex = -1;
        for (int i = 0; i < m_playingSounds.Count; i++)
        {
            if (m_playingSounds[i].name == name)
            {
                if (oldestIndex < 0 || m_playingSounds[oldestIndex].startTime > m_playingSounds[i].startTime)
                    oldestIndex = i;
            }
        }

        if (oldestIndex >= 0)
            StopSoundInternal(oldestIndex);
    }

    void StopSoundInternal(int index)
    {
        if (index < 0 || index >= m_playingSounds.Count)
            return;

        var s = m_playingSounds[index];

        m_freeSoundSource.Add(s.source);
        s.source.Stop();
        s.source.clip = null;
        s.source.transform.localPosition = Vector3.zero;

        m_playingSounds.RemoveAt(index);
    }

    public void PlayMusic(string name, float volume = 1, bool instant = false, bool forceRestart = false)
    {
        if (m_musicName == name && !forceRestart)
            return;

        StopMusic(instant);

        var clip = Global.instance.soundsDatas.GetMusic(name);
        if (clip == null)
            return;

        m_musicSourceIndex = 0;
        if (m_musicLastSourceIndex == 0)
            m_musicSourceIndex = 1;

        if (m_musicSourceIndex >= m_musicSources.Count)
            return;

        var s = m_musicSources[m_musicSourceIndex];
        s.name = name;
        s.startTime = Time.time;
        s.volume = volume;
        s.source.clip = clip;
        s.source.loop = true;

        if (instant)
            s.source.volume = volume;
        else s.source.volume = 0;

        if (instant)
            m_musicTransitionTime = -1;
        else m_musicTransitionTime = 0;
    }

    public string GetPlayingMusic()
    {
        return m_musicName;
    }

    public void StopMusic(bool instant = false)
    {
        if (m_musicSourceIndex < 0)
            return;

        if(instant)
        {
            m_musicSourceIndex = -1;
            m_musicLastSourceIndex = -1;
            m_musicTransitionTime = -1;
            
            foreach(var s in m_musicSources)
            {
                s.source.Stop();
                s.source.clip = null;
            }
        }
        else
        {
            m_musicLastSourceIndex = m_musicSourceIndex;
            m_musicTransitionTime = 0;
            int otherSource = 0;
            if (m_musicLastSourceIndex == 0)
                otherSource = 1;

            if (otherSource < m_musicSources.Count)
            {
                m_musicSources[otherSource].source.Stop();
                m_musicSources[otherSource].source.clip = null;
            }
        }
    }
}