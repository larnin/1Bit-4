using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelSelectionElement : MonoBehaviour
{
    int m_levelIndex = -1;
    bool m_infiniteMode = false;
    bool m_locked = true;

    public void SetLevelIndex(int levelIndex)
    {
        m_levelIndex = levelIndex;
        m_infiniteMode = false;

        SetDatas();
    }

    public void SetInfiniteMode()
    {
        m_levelIndex = -1;
        m_infiniteMode = true;

        SetDatas();
    }

    void SetDatas()
    {
        var info = Global.instance.levelsData.GetLevelInfo(m_levelIndex, m_infiniteMode);
        if (info == null)
            return;

        var titleTr = transform.Find("Name");
        if(titleTr != null)
        {
            var titleTxt = titleTr.GetComponent<TMP_Text>();
            if (titleTxt != null)
                titleTxt.SetText(info.name);
        }

        var descTr = transform.Find("Description");
        if(descTr != null)
        {
            var descTxt = descTr.GetComponent<TMP_Text>();
            if (descTxt != null)
                descTxt.SetText(info.description);
        }

        var iconBackTr = transform.Find("IconBack");
        if(iconBackTr != null)
        {
            var iconTr = iconBackTr.Find("Icon");
            if(iconTr != null)
            {
                var iconImg = iconTr.GetComponent<Image>();
                if (iconImg != null)
                    iconImg.sprite = info.icon;
            }
        }


        m_locked = false;
        if (info.unlockCondition == LevelUnlockCondition.WhenPreviousCompleted && !m_infiniteMode)
        {
            if (m_levelIndex > 0)
            {
                var previous = Global.instance.levelsData.GetLevelInfo(m_levelIndex - 1, false);
                if (previous != null)
                    m_locked = !GameInfos.instance.persistant.IsLevelCompleted(previous.name);
            }
        }

        var lockTr = transform.Find("Lock");
        if (lockTr != null)
            lockTr.gameObject.SetActive(m_locked);
    }

    public int GetLevelIndex()
    {
        return m_levelIndex;
    }

    public bool IsInfiniteMode()
    {
        return m_infiniteMode;
    }

    public bool IsLocked()
    {
        return m_locked;
    }
}
