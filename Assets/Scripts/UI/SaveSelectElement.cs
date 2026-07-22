using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class SaveSelectElement : MonoBehaviour
{
    [SerializeField] TMP_Text m_playButtonText;
    [SerializeField] TMP_Text m_slotText;
    [SerializeField] TMP_Text m_dataText;
    [SerializeField] GameObject m_deleteButton;

    int m_index = -1;
    SaveSelectMenu m_menu;

    public void SetData(int index, SaveSelectMenu menu)
    {
        m_index = index;
        m_menu = menu;

        UpdateInfos();
    }

    public void OnPlay()
    {
        Save.instance.SelectSaveSlot(m_index);

        var header = Save.instance.GetHeader(m_index);
        if (header.empty)
            Save.instance.SaveCurrentSlot();

        if (m_menu != null)
            m_menu.OnPlay(m_index);
    }

    public void OnErase()
    {
        Save.instance.DeleteSave(m_index);
        UpdateInfos();
    }

    void UpdateInfos()
    {
        if(m_slotText != null)
            m_slotText.text = "Slot " + m_index;

        var header = Save.instance.GetHeader(m_index);
        if (header == null)
            return;

        if(header.empty)
        {
            m_dataText.text = "Empty";
            m_playButtonText.text = "New";
            m_deleteButton.SetActive(false);
        }
        else
        {
            m_dataText.text = "Playtime\n" +header.playTime.ToString("");
            m_playButtonText.text = "Contine";
            m_deleteButton.SetActive(true);
        }
    }
}
