using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SoundButton : MonoBehaviour
{
    [SerializeField] SoundType m_soundType;
    [SerializeField] Sprite m_enabledSprite;
    [SerializeField] Sprite m_disabledSprite;
    [SerializeField] Image m_image;

    public enum SoundType
    {
        Music,
        Sound,
    }

    private void Awake()
    {
        var trigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => { OnClick(); });
        trigger.triggers.Add(clickEntry);

        UpdateIcon();

    }
    
    void OnClick()
    {
        float newVolume = IsEnabled() ? 0 : 1;

        if (m_soundType == SoundType.Music)
            GameInfos.instance.settings.SetMusicVolume(newVolume);
        else GameInfos.instance.settings.SetSoundVolume(newVolume);

        UpdateIcon();
    }

    bool IsEnabled()
    {
        float volume = 0;
        if (m_soundType == SoundType.Music)
            volume = GameInfos.instance.settings.GetMusicVolume();
        else volume = GameInfos.instance.settings.GetSoundVolume();

        return volume > 0.01f;
    }

    void UpdateIcon()
    {
        bool enabled = IsEnabled();

        if(m_image != null)
        {
            if (enabled)
                m_image.sprite = m_enabledSprite;
            else m_image.sprite = m_disabledSprite;
        }
    }
}
