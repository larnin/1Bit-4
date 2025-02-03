using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour
{
    [SerializeField] string m_hoverSound;
    [SerializeField] float m_hoverSoundVolume = 1;
    [SerializeField] string m_clickSound;
    [SerializeField] float m_clickSoundVolume = 1;

    private void Awake()
    {
        
        var trigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
        hoverEntry.eventID = EventTriggerType.PointerEnter;
        hoverEntry.callback.AddListener((data) => { OnHover(); });
        trigger.triggers.Add(hoverEntry);

        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => { OnClick(); });
        trigger.triggers.Add(clickEntry);

    }

    void OnHover()
    {
        if (SoundSystem.instance == null)
            return;

        SoundSystem.instance.PlaySoundUI(m_hoverSound, m_hoverSoundVolume);
    }

    void OnClick()
    {
        if (SoundSystem.instance == null)
            return;

        SoundSystem.instance.PlaySoundUI(m_clickSound, m_clickSoundVolume);
    }
}
