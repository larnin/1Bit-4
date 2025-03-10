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
    [SerializeField] TMP_InputField m_seedField;
    [SerializeField] Button m_quitButton;
    [SerializeField] string m_gameSceneName;
    [SerializeField] GameObject m_gargantuanWarning;
    [SerializeField] GameObject m_gargantuanButton;

    WorldSize m_worldSize = WorldSize.Medium;
    string m_seed = "";

    bool m_selected = false;

    private void Start()
    {
        RandomSeed();

        if (m_gargantuanWarning != null)
            m_gargantuanWarning.SetActive(false);

        HideQuitButton();
    }

    public void SetWorldSize(int sizeType)
    {
        m_worldSize = (WorldSize)sizeType;

        #if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (m_gargantuanWarning != null)
            m_gargantuanWarning.SetActive(m_worldSize == WorldSize.Gargantuan);
        #endif
    }

    public void UpdateSeed()
    {
        m_seed = m_seedField.text;
    }

    public void RandomSeed()
    {
        string newSeed = StaticRandomGenerator<MT19937>.Get().Next().ToString();
        m_seedField.text = newSeed;
        m_seed = newSeed;
    }

    public void Play()
    {
        if (m_selected)
            return;

        m_selected = true;
        GameInfos.instance.gameParams.worldSize = m_worldSize;
        GameInfos.instance.gameParams.seedStr = m_seed;
        GameInfos.instance.gameParams.seed = Cast.HashString(m_seed);

        var scene = new ChangeSceneParams(m_gameSceneName);
        scene.skipFadeOut = true;

        SceneSystem.changeScene(scene);
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
