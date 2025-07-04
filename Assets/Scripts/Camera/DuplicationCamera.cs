﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DuplicationCamera : MonoBehaviour
{
    [SerializeField] GameObject m_cameraPrefab;
    [SerializeField] GameObject m_additionnalCameraPrefab;
    [SerializeField] Camera m_mainCamera;

    class CameraInstance
    {
        public GameObject obj;
        public Camera camera;
        public Camera FxCamera;
        public Camera AdditionnalCamera;
        public Vector2Int offset;
    }

    SubscriberList m_subscriberList = new SubscriberList();

    List<CameraInstance> m_duplicatedCameras = new List<CameraInstance>();

    Grid m_grid;

    private void Awake()
    {
        m_subscriberList.Add(new Event<CameraMoveEvent>.Subscriber(OnCameraMove));
        m_subscriberList.Add(new Event<SetGridEvent>.Subscriber(SetGrid));
        m_subscriberList.Add(new Event<GetAllMainCameraEvent>.Subscriber(GetAllCameras));
        m_subscriberList.Add(new Event<GetCameraDuplicationEvent>.Subscriber(GetDuplications));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void SetGrid(SetGridEvent e)
    {
        m_grid = e.grid;
    }

    void OnCameraMove(CameraMoveEvent e)
    {
        UpdateCameras();
    }

    void GetDuplications(GetCameraDuplicationEvent e)
    {
        e.duplications.Clear();

        foreach(var c in m_duplicatedCameras)
        {
            e.duplications.Add(c.offset);
        }
    }

    private void Update()
    {
        UpdateCameras();
    }

    void UpdateCameras()
    {
        if (m_grid == null)
            return;

        int height = GridEx.GetRealHeight(m_grid) + Global.instance.blockDatas.renderMoreHeight;

        var rect = GetFrustrumRect(m_mainCamera, height);

        //DebugDraw.Rectangle(new Vector3(rect.position.x, 0, rect.position.y), rect.size, Color.red);

        int size = GridEx.GetRealSize(m_grid);

        var rectMin = rect.min;
        var rectMax = rect.max;

        var min = new Vector2Int(PosToSizeIndex(Mathf.RoundToInt(rectMin.x), size), PosToSizeIndex(Mathf.RoundToInt(rectMin.y), size));
        var max = new Vector2Int(PosToSizeIndex(Mathf.RoundToInt(rectMax.x), size), PosToSizeIndex(Mathf.RoundToInt(rectMax.y), size));

        if(!m_grid.LoopX())
        {
            min.x = 0;
            max.x = 0;
        }

        if(!m_grid.LoopZ())
        {
            min.y = 0;
            max.y = 0;
        }

        min.x = Mathf.Max(min.x, -5);
        min.y = Mathf.Max(min.y, -5);
        max.x = Mathf.Clamp(max.x, min.x, 5);
        max.y = Mathf.Clamp(max.y, min.y, 5);

        //Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
        var planes = GeometryUtility.CalculateFrustumPlanes(m_mainCamera);
        Vector4[] planeVects = new Vector4[4];
        for (int i = 0; i < 4; i++)
        {
            var normal = planes[i].normal;
            planeVects[i] = new Vector4(normal.x, normal.y, normal.z, planes[i].distance);
        }

        int nbCamera = 0;
        for(int i = min.x; i <= max.x; i++)
        {
            for(int j = min.y; j <= max.y; j++)
            {
                Vector3Int box = new Vector3Int(i, 0, j);

                Color color = Color.green;
                if (IsChunkOnFrustrum(planeVects, box, size, height))
                {
                    color = Color.cyan;
                    CameraInstance c = null;

                    if (nbCamera >= m_duplicatedCameras.Count)
                    {
                        c = MakeNewCamera();
                        m_duplicatedCameras.Add(c);
                    }
                    else c = m_duplicatedCameras[nbCamera];

                    c.offset = new Vector2Int(i, j);

                    nbCamera++;
                }

                //var minBox = new Vector3(box.x * size, box.y * height, box.z * size) + Vector3.one;
                //var maxBox = new Vector3((box.x + 1) * size, (box.y + 1) * height, (box.z + 1) * size) - Vector3.one;
                //DebugDraw.Rectangle(minBox, new Vector2((maxBox - minBox).x, (maxBox - minBox).z), color);
            }
        }

        while(m_duplicatedCameras.Count > nbCamera)
        {
            Destroy(m_duplicatedCameras[nbCamera].obj);
            m_duplicatedCameras.RemoveAt(nbCamera);
        }

        bool ortho = m_mainCamera.orthographic;
        float fov = m_mainCamera.fieldOfView;
        float camSize = m_mainCamera.orthographicSize;
        foreach (var c in m_duplicatedCameras)
        {
            c.obj.transform.localPosition = Vector3.zero;
            c.obj.transform.localRotation = Quaternion.identity;

            c.camera.transform.rotation = m_mainCamera.transform.rotation;
            c.camera.transform.position = m_mainCamera.transform.position - new Vector3(c.offset.x, 0, c.offset.y) * size;
            c.camera.orthographic = ortho;
            c.camera.orthographicSize = camSize;
            c.camera.fieldOfView = fov;

            c.FxCamera.transform.rotation = c.camera.transform.rotation;
            c.FxCamera.transform.position = c.camera.transform.position;
            c.FxCamera.orthographic = ortho;
            c.FxCamera.orthographicSize = camSize;
            c.FxCamera.fieldOfView = fov;

            if(c.AdditionnalCamera != null)
            { 
                c.AdditionnalCamera.transform.rotation = c.camera.transform.rotation;
                c.AdditionnalCamera.transform.position = c.camera.transform.position;
                c.AdditionnalCamera.orthographic = ortho;
                c.AdditionnalCamera.orthographicSize = camSize;
                c.AdditionnalCamera.fieldOfView = fov;
            }
        }
    }

    Rect GetFrustrumRect(Camera c, int height)
    {
        float screenWidth = Screen.width - 1;
        float screenHeight = Screen.height - 1;
        var rays = new Ray[] {
            c.ScreenPointToRay(Vector3.zero),
            c.ScreenPointToRay(new Vector3(screenWidth, 0, 0)),
            c.ScreenPointToRay(new Vector3(0, screenHeight, 0)),
            c.ScreenPointToRay(new Vector3(screenWidth, screenHeight, 0))};

        Rect b = new Rect();
        bool boundsSet = false;

        var planes = new Plane[] { new Plane(Vector3.up, new Vector3(0, -0.5f, 0)), new Plane(Vector3.up, new Vector3(0, height - 0.5f, 0)) };

        foreach (var ray in rays)
        {
            foreach (var p in planes)
            {
                float enter;
                if (p.Raycast(ray, out enter))
                {
                    var pos = ray.GetPoint(enter);
                    var pos2 = new Vector2(pos.x, pos.z);
                    if (!boundsSet)
                        b = new Rect(pos2, Vector3.zero);
                    else b = b.Encapsulate(pos2);
                    boundsSet = true;
                }
            }
        }

        return b;
    }

    static int PosToSizeIndex(int pos, int size)
    {
        int index = pos / size;
        if (pos < 0)
            index--;

        return index;
    }

    static bool IsChunkOnFrustrum(Vector4[] planes, Vector3Int box, int size, int height)
    {
        //https://iquilezles.org/articles/frustumcorrect/

        var min = new Vector3(box.x * size, box.y * height, box.z * size);
        var max = new Vector3((box.x + 1) * size, (box.y + 1) * height, (box.z + 1) * size);

        for (int i = 0; i < 4; i++)
        {
            int v = 0;
            v += (Vector4.Dot(planes[i], new Vector4(min.x, min.y, min.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(max.x, min.y, min.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(min.x, max.y, min.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(max.x, max.y, min.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(min.x, min.y, max.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(max.x, min.y, max.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(min.x, max.y, max.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(max.x, max.y, max.z, 1.0f)) < 0) ? 1 : 0;
            if (v == 8)
                return false;
        }

        return true;
    }


    CameraInstance MakeNewCamera()
    {
        var instance = new CameraInstance();
        instance.obj = Instantiate(m_cameraPrefab);
        instance.obj.transform.parent = transform;

        if(m_additionnalCameraPrefab != null)
        {
            var additionnalObj = Instantiate(m_additionnalCameraPrefab);
            additionnalObj.transform.parent = instance.obj.transform;
            additionnalObj.transform.localPosition = Vector3.zero;
            additionnalObj.transform.localRotation = Quaternion.identity;
            instance.AdditionnalCamera = additionnalObj.GetComponentInChildren<Camera>();
        }

        var cameraObj = instance.obj.transform.Find("Camera");
        if (cameraObj != null)
            instance.camera = cameraObj.GetComponent<Camera>();

        var fxCameraObj = instance.obj.transform.Find("FxCamera");
        if (fxCameraObj != null)
            instance.FxCamera = fxCameraObj.GetComponent<Camera>();

        return instance;
    }

    void GetAllCameras(GetAllMainCameraEvent e)
    {
        e.cameras = new List<Camera>();

        foreach(var c in m_duplicatedCameras)
        {
            e.cameras.Add(c.camera);
        }
    }
}
