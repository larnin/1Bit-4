using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] string m_menuName;

    bool m_selected = false;
    bool m_firstFrame = false;

    private void Awake()
    {
        var canvas = GetComponent<Canvas>();

        GetCameraEvent camera = new GetCameraEvent();
        Event<GetCameraEvent>.Broadcast(camera);

        if(camera.UICamera != null && canvas != null)
        {
            canvas.worldCamera = camera.UICamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = 1;
        }

        GameInfos.instance.paused = true;
    }

    public void OnContinue()
    {
        if (m_selected)
            return;

        GameInfos.instance.paused = false;

        Destroy(gameObject);
    }

    public void OnOptions()
    {
        if (m_selected)
            return;

    }

    public void OnQuit()
    {
        if (m_selected)
            return;

        m_selected = true;

        SceneSystem.changeScene(m_menuName, false, () => { GameInfos.instance.paused = false; });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && m_firstFrame)
            OnContinue();

        m_firstFrame = true;
    }
}

