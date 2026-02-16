using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class GameCamera : MonoBehaviour
{
    [SerializeField] IsoCameraParams m_isoCamParams = new IsoCameraParams();
    [SerializeField] FreeCameraParams m_freeCamParams = new FreeCameraParams();
    [SerializeField] Camera m_UICamera;
    [SerializeField] Camera m_clearCamera;
    [SerializeField] Camera m_lastCamera;
    [SerializeField] bool m_allowFreeCamera;

    SubscriberList m_subscriberList = new SubscriberList();

    ControlCameraIso m_controlCameraIso;
    ControlCameraFree m_controlCameraFree;

    ControlCameraBase m_currentControlCamera;

    float m_orthographicOffset = 0;

    private void Awake()
    {
        m_controlCameraIso = new ControlCameraIso(m_isoCamParams);
        m_controlCameraIso.SetParent(this);
        m_controlCameraFree = new ControlCameraFree(m_freeCamParams);
        m_controlCameraFree.SetParent(this);
        SelectControlCamera(m_controlCameraIso);

        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(OnGenerationEnd));
        m_subscriberList.Add(new Event<GetCameraEvent>.Subscriber(GetCamera));
        m_subscriberList.Add(new Event<GetCameraScaleEvent>.Subscriber(GetCameraScale));
        m_subscriberList.Add(new Event<GetCameraRotationEvent>.Subscriber(GetCameraRotation));
        m_subscriberList.Add(new Event<IsIsoCameraEvent>.Subscriber(IsIsoCamera));
        m_subscriberList.Add(new Event<GetCurrentIsoSizePercentEvent>.Subscriber(GetIsoPercent));
        m_subscriberList.Add(new Event<SetShakeIsoSizeOffsetEvent>.Subscriber(SetShakeIsoOffset));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Update()
    {
        m_currentControlCamera.Update();

        if(m_allowFreeCamera && Input.GetKeyDown(KeyCode.Tab))
        {
            if (m_currentControlCamera == m_controlCameraIso)
            {
                SelectControlCamera(m_controlCameraFree);
                if (EditorLogs.instance != null)
                    EditorLogs.instance.AddLog("Camera", "Switch to Free camera");
            }
            else
            {
                SelectControlCamera(m_controlCameraIso);
                if (EditorLogs.instance != null)
                    EditorLogs.instance.AddLog("Camera", "Switch to Isometric camera");
            }
        }

        CopyCameraInfos(m_clearCamera, m_UICamera);
        CopyCameraInfos(m_clearCamera, m_lastCamera);
    }

    void CopyCameraInfos(Camera source, Camera target)
    {
        if (target == null)
            return;

        target.fieldOfView = source.fieldOfView;
        target.orthographicSize = source.orthographicSize;
        target.orthographic = source.orthographic;
        target.transform.position = source.transform.position;
        target.transform.rotation = source.transform.rotation;
    }

    void OnGenerationEnd(GenerationFinishedEvent e)
    {
        if(BuildingList.instance != null)
        {
            var tower = BuildingList.instance.GetFirstBuilding(BuildingType.Tower);
            if(tower != null)
            {
                var center = tower.GetGroundCenter();
                m_controlCameraIso.MoveTo(center);
                m_controlCameraFree.MoveTo(center);

                Event<CameraMoveEvent>.Broadcast(new CameraMoveEvent(m_clearCamera, m_UICamera));
                return;
            }
        }

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());

        if (grid.grid == null)
            return;

        var size = GridEx.GetRealSize(grid.grid);
        var height = GridEx.GetRealHeight(grid.grid);

        Vector3 pos = new Vector3(size, height, size) / 2;

        m_controlCameraIso.MoveTo(pos);
        m_controlCameraFree.MoveTo(pos);

        Event<CameraMoveEvent>.Broadcast(new CameraMoveEvent(m_clearCamera, m_UICamera));
    }

    void GetCamera(GetCameraEvent e)
    {
        e.camera = m_clearCamera;
        e.UICamera = m_UICamera;
    }

    void GetCameraScale(GetCameraScaleEvent e)
    {
        e.scale = m_controlCameraIso.GetSize(); ;
    }

    void GetCameraRotation(GetCameraRotationEvent e)
    {
        e.rotation = m_controlCameraIso.GetRotation();
    }

    void IsIsoCamera(IsIsoCameraEvent e)
    {
        e.isoCamera = m_currentControlCamera == m_controlCameraIso;
    }

    void GetIsoPercent(GetCurrentIsoSizePercentEvent e)
    {
        e.isoSizePercent = m_controlCameraIso.GetSizePercent();
    }

    static public float PosLoop(float pos, float size)
    {
        if (pos >= 0)
            return pos % size;
        return size - ((-pos - 1) % size) - 1;
    }

    void SelectControlCamera(ControlCameraBase cam)
    {
        if (m_currentControlCamera != null)
            m_currentControlCamera.Disable();

        m_currentControlCamera = cam;

        if (cam == null)
            return;

        m_currentControlCamera.Enable();
    }

    public void OnMove()
    {
        Event<CameraMoveEvent>.Broadcast(new CameraMoveEvent(m_clearCamera, m_UICamera));
    }

    public Camera GetMainCamera()
    {
        return m_clearCamera;
    }

    public float GetOrthographicOffset()
    {
        return m_orthographicOffset;
    }

    void SetShakeIsoOffset(SetShakeIsoSizeOffsetEvent e)
    {
        m_orthographicOffset = e.offset;
    }
}
