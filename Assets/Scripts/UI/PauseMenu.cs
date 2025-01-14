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

        //todo option menu

    }

    public void OnQuit()
    {
        if (m_selected)
            return;

        m_selected = true;

        SceneSystem.changeScene(m_menuName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && m_firstFrame)
            OnContinue();

        m_firstFrame = true;
    }
}

