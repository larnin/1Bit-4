using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DuplicationCamera : MonoBehaviour
{
    [SerializeField] GameObject m_cameraPrefab;
    [SerializeField] Camera m_mainCamera;

    class CameraInstance
    {
        public GameObject obj;
        public Camera camera;
        public Camera FxCamera;
        public Vector2Int offset;
    }

    SubscriberList m_subscriberList = new SubscriberList();

    List<CameraInstance> m_duplicatedCameras = new List<CameraInstance>();

    Grid m_grid;

    private void Awake()
    {
        m_subscriberList.Add(new Event<CameraMoveEvent>.Subscriber(OnCameraMove));
        m_subscriberList.Add(new Event<CameraZoomEvent>.Subscriber(OnCameraZoom));
        m_subscriberList.Add(new Event<SetGridEvent>.Subscriber(SetGrid));
        m_subscriberList.Add(new Event<GetAllMainCameraEvent>.Subscriber(GetAllCameras));

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

    void OnCameraZoom(CameraZoomEvent e)
    {
        UpdateCameras();
    }

    void UpdateCameras()
    {
        if (m_grid == null)
            return;

        var rect = GetFrustrumRect(m_mainCamera);

        int size = GridEx.GetRealSize(m_grid);

        var rectMin = rect.min;
        var rectMax = rect.max;

        var min = new Vector2Int(PosToSizeIndex(Mathf.RoundToInt(rectMin.x), size), PosToSizeIndex(Mathf.RoundToInt(rectMin.y), size));
        var max = new Vector2Int(PosToSizeIndex(Mathf.RoundToInt(rectMax.x), size), PosToSizeIndex(Mathf.RoundToInt(rectMax.y), size));

        //Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
        var planes = GeometryUtility.CalculateFrustumPlanes(m_mainCamera);
        Vector4[] planeVects = new Vector4[4];
        for (int i = 0; i < 4; i++)
        {
            var normal = planes[i].normal;
            planeVects[i] = new Vector4(normal.x, normal.y, normal.z, planes[i].distance);
        }

        int height = GridEx.GetRealHeight(m_grid) + Global.instance.blockDatas.renderMoreHeight;

        int nbCamera = 0;
        for(int i = min.x; i <= max.x; i++)
        {
            for(int j = min.y; j <= max.y; j++)
            {
                Vector3Int box = new Vector3Int(i, 0, j);

                //if (IsChunkOnFrustrum(planeVects, box, size, height))
                {
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
            }
        }

        while(m_duplicatedCameras.Count > nbCamera)
        {
            Destroy(m_duplicatedCameras[nbCamera].obj);
            m_duplicatedCameras.RemoveAt(nbCamera);
        }

        float camSize = m_mainCamera.orthographicSize;
        foreach (var c in m_duplicatedCameras)
        {
            c.camera.orthographicSize = camSize;
            c.FxCamera.orthographicSize = camSize;

            c.obj.transform.localPosition = Vector3.zero;
            c.obj.transform.localRotation = Quaternion.identity;
            c.camera.transform.rotation = m_mainCamera.transform.rotation;
            c.camera.transform.position = m_mainCamera.transform.position - new Vector3(c.offset.x, 0, c.offset.y) * size;
        }
    }

    Rect GetFrustrumRect(Camera c)
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

        int height = GridEx.GetRealHeight(m_grid) + Global.instance.blockDatas.renderMoreHeight;

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

        var min = new Vector3(box.x * size, box.y = height, box.z * size);
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
