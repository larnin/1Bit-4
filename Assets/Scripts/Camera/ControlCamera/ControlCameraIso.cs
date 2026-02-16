using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class IsoCameraParams
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

public class ControlCameraIso : ControlCameraBase
{
    IsoCameraParams m_params;

    float m_initialAngle;
    float m_size;

    Vector3 m_oldMousePos;
    bool m_wasFocused = false;

    float m_left;
    float m_right;
    float m_up;
    float m_down;

    Vector3 m_position;
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

    Vector3 m_cameraLocation;
    Quaternion m_cameraRotation;

    public ControlCameraIso(IsoCameraParams camParams)
    {
        m_params = camParams;
    }

    public override void SetParent(GameCamera camera)
    {
        base.SetParent(camera);

        m_initialAngle = m_gameCamera.transform.rotation.eulerAngles.y;
        m_position = m_gameCamera.transform.position;
        m_startAngle = m_initialAngle;
        m_endAngle = m_initialAngle;
        m_currentAngle = m_initialAngle;
        m_rotationNormDuration = 0;
        m_resetPressTime = 0;
        m_size = m_params.initialSize;
        m_nextRotation = false;
        m_nextRotationPositive = false;
        m_resetTime = 0;

        var cam = m_gameCamera.GetMainCamera();
        m_cameraLocation = cam.transform.localPosition;
        m_cameraRotation = cam.transform.localRotation;
    }

    public override void Enable()
    {
        base.Enable();

        MoveCamera(m_position);

        var camera = m_gameCamera.GetMainCamera();
        camera.orthographic = true;
        camera.orthographicSize = m_size + m_gameCamera.GetOrthographicOffset();
        camera.transform.localPosition = m_cameraLocation;
        camera.transform.localRotation = m_cameraRotation;
    }

    public override void Disable()
    {
        base.Disable();

        if (m_resetTime != 0 || m_nextRotation)
        {
            m_resetTime = 0;
            m_position = m_resetEndPos;
            m_size = m_params.initialSize;
        }

        if(m_nextRotation)
        {
            m_nextRotation = false;
            m_rotationNormDuration = 0;
            m_currentAngle = m_endAngle;
        }

        m_resetPressTime = 0;
    }

    public override void Update()
    {
        var camera = m_gameCamera.GetMainCamera();

        if (GameInfos.instance.paused)
        {
            m_oldMousePos = Input.mousePosition;
            return;
        }

        float scrollY = Input.mouseScrollDelta.y;
        if (!GameInfos.instance.settings.IsInverseZoom())
            scrollY *= -1;

        var overUI = Event<IsMouseOverUIEvent>.Broadcast(new IsMouseOverUIEvent());
        if (overUI.overUI)
            scrollY = 0;
        if (Event<IsScrollLockedEvent>.Broadcast(new IsScrollLockedEvent()).scrollLocked)
            scrollY = 0;

        if (scrollY != 0 && m_resetTime <= 0)
        {
            float multiplier = MathF.Pow(m_params.stepZoom, MathF.Abs(scrollY));
            if (MathF.Sign(scrollY) < 0)
                multiplier = 1 / multiplier;

            m_size = m_size * multiplier;
            if (m_size < Mathf.Min(m_params.maxSize, m_params.minSize))
                m_size = Mathf.Min(m_params.maxSize, m_params.minSize);
            if (m_size > Mathf.Max(m_params.maxSize, m_params.minSize))
                m_size = MathF.Max(m_params.maxSize, m_params.minSize);

            m_gameCamera.OnMove();

            if (EditorLogs.instance != null)
                EditorLogs.instance.AddLog("Camera", "Set camera size to " + m_size.ToString("0.0"));
        }

        if (Input.GetMouseButton(2) && m_resetTime <= 0 && m_wasFocused && Application.isFocused)
        {
            var oldRay = camera.ScreenPointToRay(m_oldMousePos);
            var newRay = camera.ScreenPointToRay(Input.mousePosition);

            Plane p = new Plane(Vector3.up, Vector3.zero);

            float enter;
            p.Raycast(oldRay, out enter);
            Vector3 oldPos = oldRay.GetPoint(enter);

            p.Raycast(newRay, out enter);
            Vector3 newPos = newRay.GetPoint(enter);

            Vector3 delta = newPos - oldPos;

            Vector3 currentPos = m_position;
            currentPos -= delta;
            MoveCamera(currentPos);

            m_gameCamera.OnMove();
        }

        if (m_resetTime <= 0)
        {
            bool ctrlDown = Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl);
            bool addRight = Input.GetKey(KeyCode.RightArrow) || (Input.GetKey(KeyCode.D) && !ctrlDown);
            bool addLeft = Input.GetKey(KeyCode.LeftArrow) || ((Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A)) && !ctrlDown);
            bool addUp = Input.GetKey(KeyCode.UpArrow) || ((Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W)) && !ctrlDown);
            bool addDown = Input.GetKey(KeyCode.DownArrow) || (Input.GetKey(KeyCode.S) && !ctrlDown);

            m_right += Time.deltaTime / m_params.arrowAccelerationDuration * (addRight ? 1 : -1);
            m_left += Time.deltaTime / m_params.arrowAccelerationDuration * (addLeft ? 1 : -1);
            m_up += Time.deltaTime / m_params.arrowAccelerationDuration * (addUp ? 1 : -1);
            m_down += Time.deltaTime / m_params.arrowAccelerationDuration * (addDown ? 1 : -1);
        }

        m_right = Mathf.Clamp01(m_right);
        m_left = Mathf.Clamp01(m_left);
        m_up = Mathf.Clamp01(m_up);
        m_down = Mathf.Clamp01(m_down);

        Vector2 inputDir = new Vector2(m_right - m_left, m_up - m_down);

        if (inputDir != Vector2.zero && m_resetTime <= 0)
        {
            var forward = camera.transform.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 ortho = new Vector3(forward.z, 0, -forward.x);

            float speed = m_size / m_params.initialSize;
            Vector3 offset = (forward * inputDir.y + ortho * inputDir.x) * Time.deltaTime * m_params.arrowSpeed * speed;

            Vector3 newPos = m_position + offset;
            MoveCamera(newPos);

            m_gameCamera.OnMove();
        }

        m_oldMousePos = Input.mousePosition;
        m_wasFocused = Application.isFocused;

        //if (m_resetPressTime > 0)
        {
            if (Input.GetKey(KeyCode.R) && m_resetTime <= 0)
            {
                float newResetTime = m_resetPressTime + Time.deltaTime;
                if (m_resetPressTime < m_params.cameraResetPressTime && newResetTime >= m_params.cameraResetPressTime)
                {
                    m_resetTime = Time.deltaTime / m_params.cameraResetTime;

                    m_resetStartPos = m_position;
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

                    if (EditorLogs.instance != null)
                        EditorLogs.instance.AddLog("Camera", "Reset camera position");
                }
                m_resetPressTime = newResetTime;
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                if (m_resetPressTime < m_params.cameraResetPressTime && m_resetTime <= 0)
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

                    if (EditorLogs.instance != null)
                        EditorLogs.instance.AddLog("Camera", "Rotate camera");
                }
                m_resetPressTime = 0;
            }
        }

        if (m_currentAngle != m_endAngle)
        {
            m_rotationNormDuration += Time.deltaTime / m_params.rotationDuration;
            if (m_rotationNormDuration >= 1)
            {
                m_rotationNormDuration = 1;
                m_currentAngle = m_endAngle;
            }
            else m_currentAngle = DOVirtual.EasedValue(m_startAngle, m_endAngle, m_rotationNormDuration, Ease.InOutQuad);

            m_gameCamera.transform.rotation = Quaternion.Euler(0, m_currentAngle, 0);

            m_gameCamera.OnMove();
        }
        else if (m_nextRotation)
        {
            float rotDir = m_nextRotationPositive ? 1 : -1;
            m_rotationNormDuration = 0;
            m_startAngle = m_currentAngle;
            m_endAngle += 90 * rotDir; ;

            m_nextRotation = false;
        }

        if (m_resetTime > 0)
        {
            if (m_resetTime > 1)
                m_resetTime = 1;

            m_size = m_resetStartSize * (1 - m_resetTime) + m_params.initialSize * m_resetTime;
            MoveCamera(m_resetStartPos * (1 - m_resetTime) + m_resetEndPos * m_resetTime);

            m_gameCamera.OnMove();

            if (m_resetTime >= 1)
                m_resetTime = 0;
            else m_resetTime += Time.deltaTime / m_params.cameraResetTime;
        }

        float size = m_size + m_gameCamera.GetOrthographicOffset();

        bool changeSize = false;
        if (camera.orthographicSize != size)
            changeSize = true;

        camera.orthographicSize = size;

        if (changeSize)
            m_gameCamera.OnMove();
    }

    void MoveCamera(Vector3 newPos)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());

        if (grid.grid == null)
        {
            m_position = newPos;
            if(m_isEnabled)
                m_gameCamera.transform.position = newPos;
            return;
        }

        newPos = GridEx.ClampPos(grid.grid, newPos);

        m_position = newPos;
        if(m_isEnabled)
            m_gameCamera.transform.position = newPos;
    }

    public override void MoveTo(Vector3 pos)
    {
        MoveCamera(pos);

        m_gameCamera.OnMove();
    }

    public float GetSize()
    {
        return m_size;
    }

    public float GetRotation()
    {
        return m_currentAngle;
    }

    public float GetSizePercent()
    {
        return (m_size - m_params.minSize) / (m_params.maxSize - m_params.minSize);
    }
}
