using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NRand;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button m_continueButton;
    [SerializeField] Button m_quitButton;
    [SerializeField] Transform m_submenuPivot;

    string m_currentMenu;

    bool m_selected = false;

    private void Start()
    {
        HideContinueButton();
        HideQuitButton();
    }

    public void Play()
    {
        if (m_selected)
            return;

        m_selected = true;

        var scene = new ChangeSceneParams(Global.instance.editorDatas.lobbySceneName);
        SceneSystem.changeScene(scene);

        int currentSave = Save.instance.GetGlobal().lastPlayedSlot;
        if (currentSave < 0)
            currentSave = 0;
        Save.instance.SelectSaveSlot(currentSave);
        Save.instance.LoadCurrentSlot();
    }

    public void LoadNew()
    {
        if (m_selected)
            return;

        CloseCurrentMenu();
        var menu = MenuSystem.instance.OpenMenu<SaveSelectMenu>("SaveSelect", false, false, false);
        menu.SetMenu(this);

        SetCurrentMenu(menu.gameObject, "SaveSelect");
    }

    public void Options()
    {
        if (m_selected)
            return;

        CloseCurrentMenu();
        var menu = MenuSystem.instance.OpenMenu<OptionsMenu>("Options", false, false, false);

        SetCurrentMenu(menu.gameObject, "Options");
    }

    public void Quit()
    {
        if (m_selected)
            return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void HideQuitButton()
    {
#if !UNITY_STANDALONE_WIN && !UNITY_EDITOR
        m_quitButton.gameObject.SetActive(false);

#endif
    }

    void HideContinueButton()
    {
        if (m_continueButton == null)
            return;

        if (Save.instance.GetGlobal().lastPlayedSlot < 0)
            m_continueButton.gameObject.SetActive(false);
    }

    void SetCurrentMenu(GameObject obj, string name)
    {
        m_currentMenu = name;

        if(m_submenuPivot != null)
        {
            var tr = obj.GetComponent<RectTransform>();
            if(tr != null)
            {
                tr.SetParent(m_submenuPivot.transform, false);
                tr.localPosition = Vector3.zero;
                tr.localScale = Vector3.one;
                tr.localRotation = Quaternion.identity;
                tr.anchorMin = Vector2.zero;
                tr.anchorMax = Vector2.one;
            }
        }
    }

    void CloseCurrentMenu()
    {
        MenuSystem.instance.CloseMenu(m_currentMenu);
    }
}
