using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] Slider m_musicSlider;
    [SerializeField] Slider m_soundSlider;
    [SerializeField] TMP_Dropdown m_colorDropdown;
    [SerializeField] Toggle m_colorFlipToggle;
    [SerializeField] Toggle m_zoomToggle;
    [SerializeField] Toggle m_fullScreenToggle;

    bool m_init = false;

    private void Awake()
    {
        if (m_musicSlider != null)
            m_musicSlider.value = GameInfos.instance.settings.GetMusicVolume();

        if (m_soundSlider != null)
            m_soundSlider.value = GameInfos.instance.settings.GetSoundVolume();

        if(m_colorDropdown != null)
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach(var p in Global.instance.colorsDatas.colors)
            {
                options.Add(new TMP_Dropdown.OptionData(p.name));
            }

            m_colorDropdown.options = options;

            m_colorDropdown.value = GetCurrentColorIndex();
        }

        if(m_colorFlipToggle != null)
        {
            m_colorFlipToggle.isOn = GameInfos.instance.settings.GetColorFlip();
        }

        if(m_zoomToggle != null)
        {
            m_zoomToggle.isOn = GameInfos.instance.settings.IsInverseZoom();
        }

        if(m_fullScreenToggle != null)
        {
            m_fullScreenToggle.isOn = GameInfos.instance.settings.GetFullScreen();
        }

        m_init = true;
    }

    public void OnMusicChanged(float value)
    {
        if (!m_init)
            return;

        GameInfos.instance.settings.SetMusicVolume(value);
    }

    public void OnSoundChanged(float value)
    {
        if (!m_init)
            return;

        GameInfos.instance.settings.SetSoundVolume(value);
    }

    public void OnColorChange(int colorIndex)
    {
        if (!m_init)
            return;

        if (Global.instance.colorsDatas.colors.Count == 0)
            return;

        if (colorIndex < 0 || colorIndex >= Global.instance.colorsDatas.colors.Count)
            colorIndex = 0;

        GameInfos.instance.settings.SetColorName(Global.instance.colorsDatas.colors[colorIndex].name);
    }

    public void OnColorFlipChange(bool flipped)
    {
        if (!m_init)
            return;

        GameInfos.instance.settings.SetColorFlip(flipped);
    }

    public void OnZoomDirectionChange(bool direction)
    {
        if (!m_init)
            return;

        GameInfos.instance.settings.SetInverseZoom(direction);
    }

    public void OnFullScreenChange(bool fullScreen)
    {
        if (!m_init)
            return;

        GameInfos.instance.settings.SetFullScreen(fullScreen);
    }

    public void CloseMenu()
    {
        Destroy(gameObject);
    }

    int GetCurrentColorIndex()
    {
        string color = GameInfos.instance.settings.GetColorName();

        for(int i = 0; i < Global.instance.colorsDatas.colors.Count; i++)
        {
            if (Global.instance.colorsDatas.colors[i].name == color)
                return i;
        }

        return 0;
    }
}

