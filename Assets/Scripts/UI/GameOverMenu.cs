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
    [SerializeField] float m_appearOffset;
    [SerializeField] float m_appearDuration;
    [SerializeField] Ease m_appearCurve;
    [SerializeField] TMP_Text m_scoreValue;
    [SerializeField] TMP_Text m_killsValue;
    [SerializeField] TMP_Text m_spawnersValue;
    [SerializeField] TMP_Text m_buildingsConstructedValue;
    [SerializeField] TMP_Text m_buildingsLostValue;
    [SerializeField] TMP_Text m_seedText;
    [SerializeField] TMP_Text m_mapText;
    [SerializeField] string m_gameSceneName;
    [SerializeField] string m_gameOverSound;
    [SerializeField] float m_gameOverVolume = 1;

    bool m_selected = false;

    public void OnContinue()
    {
        if (m_selected)
            return;

        m_selected = true;

        SceneSystem.changeScene(new ChangeSceneParams(Global.instance.editorDatas.lobbySceneName));
    }

    private void Awake()
    {
        DrawScores();

        if (SoundSystem.instance != null)
            SoundSystem.instance.PlaySoundUI(m_gameOverSound, m_gameOverVolume);
    }

    private void Start()
    {
        var target = transform.localPosition;
        var start = transform.localPosition + new Vector3(0, m_appearOffset, 0);

        transform.localPosition = start;

        transform.DOLocalMove(target, m_appearDuration).SetEase(m_appearCurve);
    }

    public void SetScore(float Difficulty)
    {
        int score = Mathf.RoundToInt(Difficulty * Difficulty * 10);

        if (m_scoreValue != null)
            m_scoreValue.text = score.ToString();
    }

    void DrawScores()
    {
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

        if (m_mapText != null)
            m_mapText.text = GameInfos.instance.gameParams.worldSize.ToString();
    }
}
