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

    WorldSize m_worldSize = WorldSize.Medium;
    string m_seed = "";

    bool m_selected = false;

    private void Start()
    {
        RandomSeed();

        HideQuitButton();
    }

    public void SetWorldSize(int sizeType)
    {
        m_worldSize = (WorldSize)sizeType;
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
        GameInfos.instance.gameParams.seed = Cast.HashString(m_seed);

        var scene = new ChangeSceneParams(m_gameSceneName);
        scene.skipFadeOut = true;

        SceneSystem.changeScene(scene);
    }

    public void Options()
    {
        if (m_selected)
            return;

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
}
