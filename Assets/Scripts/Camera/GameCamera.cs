using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class GameCamera : MonoBehaviour
{
    [Serializable]
    class IsoCameraParams
    {
        public float minSize = 30;
        public float maxSize = 10;
        public float initialSize = 15;
        public float stepZoom = 1.1f;
        public float arrowSpeed = 1;
        public float arrowAccelerationDuration = 0.2f;
        public float rotationDuration = 0.5f;
        public float cameraResetPressTime = 1.0f;
        public float cameraResetTime = 0.5f;
    }

    [Serializable]
    class FreeCameraParams
    {
        public float fov;
    }

    [SerializeField] IsoCameraParams m_isoCamParams = new IsoCameraParams();
    [SerializeField] FreeCameraParams m_freeCamParams = new FreeCameraParams();
    [SerializeField] Camera m_UICamera;
    [SerializeField] Camera m_clearCamera;
    [SerializeField] Camera m_lastCamera;

    float m_initialAngle;
    float m_size;

    Vector3 m_oldMousePos;
    bool m_wasFocused = false;

    float m_left;
    float m_right;
    float m_up;
    float m_down;

    float m_currentAngle;
    float m_startAngle;
    float m_endAngle;
    float m_rotationNormDuration;

    bool m_nextRotation;
    bool m_nextRotationPositive;

    float m_resetPressTime;

    float m_resetTime;
    Vector3 m_resetStartPos;
    Vector3 m_resetEndPos;
    float m_resetStartSize;

    Grid m_grid;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(OnGenerationEnd));
        m_subscriberList.Add(new Event<GetCameraEvent>.Subscriber(GetCamera));
        m_subscriberList.Add(new Event<GetCameraScaleEvent>.Subscriber(GetCameraScale));
        m_subscriberList.Add(new Event<GetCameraRotationEvent>.Subscriber(GetCameraRotation));
        m_subscriberList.Add(new Event<SetGridEvent>.Subscriber(SetGrid));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        m_initialAngle = transform.rotation.eulerAngles.y;
        m_startAngle = m_initialAngle;
        m_endAngle = m_initialAngle;
        m_currentAngle = m_initialAngle;
        m_rotationNormDuration = 0;
        m_resetPressTime = 0;
        m_size = m_isoCamParams.initialSize;
        m_nextRotation = false;
        m_nextRotationPositive = false;
        m_resetTime = 0;
    }

    private void Update()
    {
        if (GameInfos.instance.paused)
        {
            m_oldMousePos = Input.mousePosition;
            return;
        }

        float scrollY = Input.mouseScrollDelta.y;
        if (!GameInfos.instance.settings.IsInverseZoom())
            scrollY *= -1;

        if (scrollY != 0 && m_resetTime <= 0)
        {
            float multiplier = MathF.Pow(m_isoCamParams.stepZoom, MathF.Abs(scrollY));
            if (MathF.Sign(scrollY) < 0)
                multiplier = 1 / multiplier;

            m_size = m_size * multiplier;
            if (m_size < Mathf.Min(m_isoCamParams.maxSize, m_isoCamParams.minSize))
                m_size = Mathf.Min(m_isoCamParams.maxSize, m_isoCamParams.minSize);
            if (m_size > Mathf.Max(m_isoCamParams.maxSize, m_isoCamParams.minSize))
                m_size = MathF.Max(m_isoCamParams.maxSize, m_isoCamParams.minSize);

            Event<CameraMoveEvent>.Broadcast(new CameraMoveEvent(m_clearCamera, m_UICamera));
        }

        if(Input.GetMouseButton(2) && m_resetTime <= 0 && m_wasFocused && Application.isFocused)
        {
            var oldRay = m_clearCamera.ScreenPointToRay(m_oldMousePos);
            var newRay = m_clearCamera.ScreenPointToRay(Input.mousePosition);

            Plane p = new Plane(Vector3.up, Vector3.zero);

            float enter;
            p.Raycast(oldRay, out enter);
            Vector3 oldPos = oldRay.GetPoint(enter);

            p.Raycast(newRay, out enter);
            Vector3 newPos = newRay.GetPoint(enter);

            Vector3 delta = newPos - oldPos;

            Vector3 currentPos = transform.position;
            currentPos -= delta;
            MoveCamera(currentPos);

            Event<CameraMoveEvent>.Broadcast(new CameraMoveEvent(m_clearCamera, m_UICamera));
        }

        if (m_resetTime <= 0)
        {
            bool addRight = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
            bool addLeft = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A);
            bool addUp = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W);
            bool addDown = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);

            m_right += Time.deltaTime / m_isoCamParams.arrowAccelerationDuration * (addRight ? 1 : -1);
            m_left += Time.deltaTime / m_isoCamParams.arrowAccelerationDuration * (addLeft ? 1 : -1);
            m_up += Time.deltaTime / m_isoCamParams.arrowAccelerationDuration * (addUp ? 1 : -1);
            m_down += Time.deltaTime / m_isoCamParams.arrowAccelerationDuration * (addDown ? 1 : -1);
        }

        m_right = Mathf.Clamp01(m_right);
        m_left = Mathf.Clamp01(m_left);
        m_up = Mathf.Clamp01(m_up);
        m_down = Mathf.Clamp01(m_down);

        Vector2 inputDir = new Vector2(m_right - m_left, m_up - m_down);

        if(inputDir != Vector2.zero && m_resetTime <= 0)
        {
            var forward = m_clearCamera.transform.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 ortho = new Vector3(forward.z, 0, -forward.x);

            float speed = m_size / m_isoCamParams.initialSize;
            Vector3 offset = (forward * inputDir.y + ortho * inputDir.x) * Time.deltaTime * m_isoCamParams.arrowSpeed * speed;

            Vector3 newPos = transform.position + offset;
            MoveCamera(newPos);

            Event<CameraMoveEvent>.Broadcast(new CameraMoveEvent(m_clearCamera, m_UICamera));
        }

        m_oldMousePos = Input.mousePosition;
        m_wasFocused = Application.isFocused;

        //if (m_resetPressTime > 0)
        {
            if (Input.GetKey(KeyCode.R) && m_resetTime <= 0)
            {
                float newResetTime = m_resetPressTime + Time.deltaTime;
                if (m_resetPressTime < m_isoCamParams.cameraResetPressTime && newResetTime >= m_isoCamParams.cameraResetPressTime)
                {
                    m_resetTime = Time.deltaTime / m_isoCamParams.cameraResetTime;

                    m_resetStartPos = transform.position;
                    m_resetEndPos = m_resetStartPos;

                    if (BuildingList.instance != null)
                    {
                        var tower = BuildingList.instance.GetFirstBuilding(BuildingType.Tower);
                        if (tower != null)
                            m_resetEndPos = tower.GetGroundCenter();
                    }

                    m_resetStartSize = m_size;

                    m_startAngle = Utility.ReduceAngle(m_currentAngle);
                    m_endAngle = m_initialAngle;
                    m_rotationNormDuration = 0;
                }
                m_resetPressTime = newResetTime;
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                if (m_resetPressTime < m_isoCamParams.cameraResetPressTime && m_resetTime <= 0)
                {
                    float rotDir = 1;
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        rotDir = -1;

                    if (m_currentAngle != m_endAngle)
                    {
                        m_nextRotation = true;
                        m_nextRotationPositive = rotDir > 0;
                    }
                    else
                    {
                        m_rotationNormDuration = 0;
                        m_startAngle = m_currentAngle;
                        m_endAngle += 90 * rotDir; ;
                    }
                }
                m_resetPressTime = 0;
            }
        }

        if (m_currentAngle != m_endAngle)
        {
            m_rotationNormDuration += Time.deltaTime / m_isoCamParams.rotationDuration;
            if(m_rotationNormDuration >= 1)
            {
                m_rotationNormDuration = 1;
                m_currentAngle = m_endAngle;
            }
            else m_currentAngle = DOVirtual.EasedValue(m_startAngle, m_endAngle, m_rotationNormDuration, Ease.InOutQuad);

            transform.rotation = Quaternion.Euler(0, m_currentAngle, 0);

            Event<CameraMoveEvent>.Broadcast(new CameraMoveEvent(m_clearCamera, m_UICamera));
        }
        else if (m_nextRotation)
        {
            float rotDir = m_nextRotationPositive ? 1 : -1;
            m_rotationNormDuration = 0;
            m_startAngle = m_currentAngle;
            m_endAngle += 90 * rotDir; ;

            m_nextRotation = false;
        }

        if(m_resetTime > 0)
        {
            if (m_resetTime > 1)
                m_resetTime = 1;

            m_size = m_resetStartSize * (1 - m_resetTime) + m_isoCamParams.initialSize * m_resetTime;
            MoveCamera(m_resetStartPos * (1 - m_resetTime) + m_resetEndPos * m_resetTime);

            Event<CameraMoveEvent>.Broadcast(new CameraMoveEvent(m_clearCamera, m_UICamera));

            if (m_resetTime >= 1)
                m_resetTime = 0;
            else m_resetTime += Time.deltaTime / m_isoCamParams.cameraResetTime;
        }

        UpdateCameraMatrix();
    }

    void SetGrid(SetGridEvent e)
    {
        m_grid = e.grid;
    }

    void OnGenerationEnd(GenerationFinishedEvent e)
    {
        if(BuildingList.instance != null)
        {
            var tower = BuildingList.instance.GetFirstBuilding(BuildingType.Tower);
            if(tower != null)
            {
                var center = tower.GetGroundCenter();
                MoveCamera(center);

                Event<CameraMoveEvent>.Broadcast(new CameraMoveEvent(m_clearCamera, m_UICamera));
                return;
            }
        }

        if (m_grid == null)
            return;

        var size = GridEx.GetRealSize(m_grid);
        var height = GridEx.GetRealHeight(m_grid);

        Vector3 pos = new Vector3(size, height, size) / 2;
        MoveCamera(pos);

        Event<CameraMoveEvent>.Broadcast(new CameraMoveEvent(m_clearCamera, m_UICamera));
    }

    void GetCamera(GetCameraEvent e)
    {
        e.camera = m_clearCamera;
        e.UICamera = m_UICamera;
    }

    void GetCameraScale(GetCameraScaleEvent e)
    {
        e.scale = m_size;
    }

    void GetCameraRotation(GetCameraRotationEvent e)
    {
        e.rotation = m_currentAngle;
    }

    void UpdateCameraMatrix()
    {
        bool changeSize = false;
        if (m_clearCamera.orthographicSize != m_size)
            changeSize = true;

        m_UICamera.orthographicSize = m_size;
        m_clearCamera.orthographicSize = m_size;
        m_lastCamera.orthographicSize = m_size;

        if(changeSize)
            Event<CameraZoomEvent>.Broadcast(new CameraZoomEvent(m_size));
    }

    void MoveCamera(Vector3 newPos)
    {
        if (m_grid == null)
        {
            transform.position = newPos;
            return;
        }

        float size = GridEx.GetRealSize(m_grid);

        if(newPos.x < 0 || newPos.x > size)
        {
            if (m_grid.LoopX())
                newPos.x = PosLoop(newPos.x, size);
            else if (newPos.x < 0)
                newPos.x = 0;
            else newPos.x = size;
        }

        if(newPos.z < 0 || newPos.z > size)
        {
            if (m_grid.LoopZ())
                newPos.z = PosLoop(newPos.z, size);
            else if (newPos.z < 0)
                newPos.z = 0;
            else newPos.z = size;
        }

        transform.position = newPos;
    }

    static float PosLoop(float pos, float size)
    {
        if (pos >= 0)
            return pos % size;
        return size - ((-pos - 1) % size) - 1;
    }
}
