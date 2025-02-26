using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapButton : MonoBehaviour
{
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
        GameInfos.instance.settings.SetDisplayMap(!GameInfos.instance.settings.GetDisplayMap());

        UpdateIcon();
    }

    void UpdateIcon()
    {
        bool enabled = GameInfos.instance.settings.GetDisplayMap();

        if (m_image != null)
        {
            if (enabled)
                m_image.sprite = m_enabledSprite;
            else m_image.sprite = m_disabledSprite;
        }
    }
}
