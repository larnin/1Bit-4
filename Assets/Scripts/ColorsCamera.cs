using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ColorsCamera : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<SettingsColorChangedEvent>.Subscriber(OnColorChange));
        m_subscriberList.Subscribe();

        OnColorChange();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnColorChange(SettingsColorChangedEvent e)
    {
        OnColorChange();
    }

    void OnColorChange()
    {
        var colorName = GameInfos.instance.settings.GetColorName();
        bool colorFlip = GameInfos.instance.settings.GetColorFlip();

        Color lightColor = Global.instance.colorsDatas.GetLightColor(colorName);
        Color darkColor = Global.instance.colorsDatas.GetDarkColor(colorName);

        if(colorFlip)
        {
            var temp = lightColor;
            lightColor = darkColor;
            darkColor = temp;
        }

        var effects = GetComponentsInChildren<BlackAndWhitePostEffect>();

        foreach(var e in effects)
        {
            e.SetColors(lightColor, darkColor);
        }
    }
}
