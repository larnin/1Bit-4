using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SaveSelectMenu : MonoBehaviour
{
    [SerializeField] GameObject m_oneSavePrefab;
    [SerializeField] float m_distanceBetweenSaves;
    [SerializeField] Transform m_slotsPivot;

    MainMenu m_menu;

    List<SaveSelectElement> m_elements = new List<SaveSelectElement>();

    public void SetMenu(MainMenu menu)
    {
        m_menu = menu;
    }

    private void Awake()
    {
        int nbSave = Save.maxSaveSlots;

        for(int i = 0; i < nbSave; i++)
        {
            float pos = (-(nbSave - 1) * 0.5f + i) * m_distanceBetweenSaves;

            var obj = Instantiate(m_oneSavePrefab);
            obj.transform.SetParent(m_slotsPivot, false);
            obj.transform.localPosition = new Vector3(pos, 0, 0);
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            var comp = obj.GetComponent<SaveSelectElement>();
            if(comp != null)
            {
                comp.SetData(i, this);
                m_elements.Add(comp);
            }
        }
    }

    public void OnPlay(int index)
    {
        Save.instance.SelectSaveSlot(index);
        Save.instance.SaveCurrentSlot();
        Save.instance.GetGlobal().lastPlayedSlot = index;
        Save.instance.SaveGlobal();
        if (m_menu != null)
            m_menu.Play();
    }
}