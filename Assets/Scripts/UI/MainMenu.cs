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
    [SerializeField] Button m_quitButton;
    [SerializeField] string m_gameSceneName;

    bool m_selected = false;

    private void Start()
    {
        HideQuitButton();
    }

    public void Play()
    {
        if (m_selected)
            return;

        MenuSystem.instance.OpenMenu<LevelSelectionMenu>("LevelSelect");
    }

    public void Options()
    {
        if (m_selected)
            return;

        MenuSystem.instance.OpenMenu<OptionsMenu>("Options");
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

        if(m_gargantuanButton != null)
            m_gargantuanButton.gameObject.SetActive(false);
        if(m_gargantuanWarning != null)
        {
            var text = m_gargantuanWarning.GetComponent<TMP_Text>();
            if(text != null)
            {
                text.text = "Gargantian size is not available in the web version";
            }
            m_gargantuanWarning.SetActive(true);
        }

#endif
    }
}
