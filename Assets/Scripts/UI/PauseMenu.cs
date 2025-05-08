using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] string m_menuName;
    [SerializeField] string m_pauseSound;
    [SerializeField] float m_pauseVolume = 1;

    bool m_selected = false;
    bool m_firstFrame = false;

    private void Awake()
    {
        var canvas = GetComponent<Canvas>();

        var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());

        if(camera.UICamera != null && canvas != null)
        {
            canvas.worldCamera = camera.UICamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = 1;
        }

        SetTip();

        if (SoundSystem.instance != null)
            SoundSystem.instance.PlaySoundUI(m_pauseSound, m_pauseVolume);
    }

    public void OnContinue()
    {
        if (m_selected)
            return;

        Destroy(gameObject);
    }

    public void OnOptions()
    {
        if (m_selected)
            return;

        if (MenuSystem.instance == null)
            return;

        MenuSystem.instance.OpenMenu<OptionsMenu>("Options");
    }

    public void OnQuit()
    {
        if (m_selected)
            return;

        m_selected = true;

        SceneSystem.changeScene(new ChangeSceneParams(m_menuName));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && m_firstFrame)
            OnContinue();

        m_firstFrame = true;
    }

    void SetTip()
    {
        var tipTr = transform.Find("Tips");
        if (tipTr == null)
            return;

        var tipText = tipTr.GetComponentInChildren<TMP_Text>();
        if (tipText == null)
            return;

        int nextIndex = Global.instance.tipsDatas.GetRandomTipIndex(GameInfos.instance.lastTip);

        if (nextIndex >= 0)
            tipText.text = Global.instance.tipsDatas.tips[nextIndex].tip;
        else tipTr.gameObject.SetActive(false);

        GameInfos.instance.lastTip = nextIndex;
    }
}

