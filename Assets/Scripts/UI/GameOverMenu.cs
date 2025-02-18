using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] string m_menuName;
    [SerializeField] float m_appearOffset;
    [SerializeField] float m_appearDuration;
    [SerializeField] Ease m_appearCurve;
    [SerializeField] TMP_Text m_scoreValue;
    [SerializeField] TMP_Text m_killsValue;
    [SerializeField] TMP_Text m_spawnersValue;
    [SerializeField] TMP_Text m_buildingsConstructedValue;
    [SerializeField] TMP_Text m_buildingsLostValue;
    [SerializeField] TMP_Text m_seedText;
    [SerializeField] string m_gameSceneName;
    [SerializeField] string m_gameOverSound;
    [SerializeField] float m_gameOverVolume = 1;

    bool m_selected = false;

    public void OnContinue()
    {
        if (m_selected)
            return;

        m_selected = true;

        SceneSystem.changeScene(new ChangeSceneParams(m_menuName));
    }

    public void OnRestart()
    {
        if (m_selected)
            return;

        m_selected = true;

        var scene = new ChangeSceneParams(m_gameSceneName);
        scene.skipFadeOut = true;

        SceneSystem.changeScene(scene);
    }

    private void Awake()
    {
        var obj = transform.Find("Pivot");
        if(obj != null)
        {
            var target = obj.localPosition;
            var start = obj.localPosition + new Vector3(0, m_appearOffset, 0);

            obj.localPosition = start;

            obj.DOLocalMove(target, m_appearDuration).SetEase(m_appearCurve);
        }

        DrawScores();

        if (SoundSystem.instance != null)
            SoundSystem.instance.PlaySoundUI(m_gameOverSound, m_gameOverVolume);
    }

    void DrawScores()
    {
        if (DifficultySystem.instance != null)
        {
            int score = Mathf.RoundToInt(DifficultySystem.instance.GetDifficulty() * DifficultySystem.instance.GetDifficulty() * 10);

            if (m_scoreValue != null)
                m_scoreValue.text = score.ToString();
        }

        if (StatsSystem.instance != null)
        {
            var stats = StatsSystem.instance.GetStats();

            if (m_killsValue != null)
                m_killsValue.text = stats.kills.ToString();

            if (m_spawnersValue != null)
                m_spawnersValue.text = stats.spawnersDestroyed.ToString();

            if (m_buildingsConstructedValue != null)
                m_buildingsConstructedValue.text = stats.buildingsBuild.ToString();

            if (m_buildingsLostValue != null)
                m_buildingsLostValue.text = stats.buildingsLost.ToString();
        }

        if (m_seedText != null)
            m_seedText.text = GameInfos.instance.gameParams.seedStr;
    }
}
