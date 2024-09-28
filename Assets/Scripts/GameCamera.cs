using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class GameCamera : MonoBehaviour
{
    [SerializeField] float m_minZoom = 25;
    [SerializeField] float m_maxZoom = 1;
    [SerializeField] float m_stepZoom = 1.1f;
    [SerializeField] float m_arrowsSpeed = 1;
    [SerializeField] float m_arrowsAccelerationDuration = 0.2f;
    [SerializeField] float m_rotationDuration = 0.5f;
    [SerializeField] float m_cameraResetPressTime = 1.0f;
    [SerializeField] float m_cameraResetTime = 0.5f;

    Camera m_camera;
    float m_initialZoom;
    Vector3 m_initialPosition;
    float m_initialAngle;
    float m_zoom;

    Vector3 m_oldMousePos;

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
    float m_resetStartCameraZoom;
    float m_resetStartZoom;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(OnGenerationEnd));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        m_camera = Camera.main;
        m_initialPosition = transform.position;
        m_initialZoom = m_camera.orthographicSize;
        m_initialAngle = transform.rotation.eulerAngles.y;
        m_startAngle = m_initialAngle;
        m_endAngle = m_initialAngle;
        m_currentAngle = m_initialAngle;
        m_rotationNormDuration = 0;
        m_resetPressTime = 0;
        m_zoom = 1;
        m_nextRotation = false;
        m_nextRotationPositive = false;
        m_resetTime = 0;
    }

    private void Update()
    {
        float scrollY = Input.mouseScrollDelta.y;
        if (scrollY != 0 && m_resetTime <= 0)
        {
            float multiplier = MathF.Pow(m_stepZoom, MathF.Abs(scrollY));
            if (MathF.Sign(scrollY) < 0)
                multiplier = 1 / multiplier;

            float newSize = m_camera.orthographicSize * multiplier;
            if (newSize > m_maxZoom && newSize < m_minZoom)
            {
                m_camera.orthographicSize = newSize;
                m_zoom *= multiplier;
            }
        }

        if(Input.GetMouseButton(2) && m_resetTime <= 0)
        {
            var oldRay = m_camera.ScreenPointToRay(m_oldMousePos);
            var newRay = m_camera.ScreenPointToRay(Input.mousePosition);

            Plane p = new Plane(Vector3.up, Vector3.zero);

            float enter;
            p.Raycast(oldRay, out enter);
            Vector3 oldPos = oldRay.GetPoint(enter);

            p.Raycast(newRay, out enter);
            Vector3 newPos = newRay.GetPoint(enter);

            Vector3 delta = newPos - oldPos;

            Vector3 currentPos = transform.position;
            currentPos -= delta;
            transform.position = currentPos;
        }

        bool addRight = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
        bool addLeft = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A);
        bool addUp = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W);
        bool addDown = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);

        m_right += Time.deltaTime / m_arrowsAccelerationDuration * (addRight ? 1 : -1);
        m_left += Time.deltaTime / m_arrowsAccelerationDuration * (addLeft ? 1 : -1);
        m_up += Time.deltaTime / m_arrowsAccelerationDuration * (addUp ? 1 : -1);
        m_down += Time.deltaTime / m_arrowsAccelerationDuration * (addDown ? 1 : -1);

        m_right = Mathf.Clamp01(m_right);
        m_left = Mathf.Clamp01(m_left);
        m_up = Mathf.Clamp01(m_up);
        m_down = Mathf.Clamp01(m_down);

        Vector2 inputDir = new Vector2(m_right - m_left, m_up - m_down);

        if(inputDir != Vector2.zero && m_resetTime <= 0)
        {
            var forward = m_camera.transform.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 ortho = new Vector3(forward.z, 0, -forward.x);

            Vector3 offset = (forward * inputDir.y + ortho * inputDir.x) * Time.deltaTime * m_arrowsSpeed * m_zoom;

            Vector3 newPos = transform.position + offset;
            transform.position = newPos;
        }

        m_oldMousePos = Input.mousePosition;

        //if (m_resetPressTime > 0)
        {
            if (Input.GetKey(KeyCode.R) && m_resetTime <= 0)
            {
                float newResetTime = m_resetPressTime + Time.deltaTime;
                if (m_resetPressTime < m_cameraResetPressTime && newResetTime >= m_cameraResetPressTime)
                {
                    m_resetTime = Time.deltaTime / m_cameraResetTime;

                    m_resetStartPos = transform.position;
                    m_resetStartCameraZoom = m_camera.orthographicSize;
                    m_resetStartZoom = m_zoom;

                    m_startAngle = Utility.ReduceAngle(m_currentAngle);
                    m_endAngle = m_initialAngle;
                    m_rotationNormDuration = 0;
                }
                m_resetPressTime = newResetTime;
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                if (m_resetPressTime < m_cameraResetPressTime && m_resetTime <= 0)
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
            m_rotationNormDuration += Time.deltaTime / m_rotationDuration;
            if(m_rotationNormDuration >= 1)
            {
                m_rotationNormDuration = 1;
                m_currentAngle = m_endAngle;
            }
            else m_currentAngle = DOVirtual.EasedValue(m_startAngle, m_endAngle, m_rotationNormDuration, Ease.InOutQuad);

            transform.rotation = Quaternion.Euler(0, m_currentAngle, 0);
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

            transform.position = m_resetStartPos * (1 - m_resetTime) + m_initialPosition * m_resetTime;
            m_camera.orthographicSize = m_resetStartCameraZoom * (1 - m_resetTime) + m_initialZoom * m_resetTime;
            m_zoom = m_resetStartZoom * (1 - m_resetTime) + m_resetTime;

            if (m_resetTime >= 1)
                m_resetTime = 0;
            else m_resetTime += Time.deltaTime / m_cameraResetTime;
        }
    }

    void OnGenerationEnd(GenerationFinishedEvent e)
    {
        var grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);
        if (grid.grid == null)
            return;

        var size = GridEx.GetRealSize(grid.grid);
        var height = GridEx.GetRealHeight(grid.grid);

        Vector3 pos = new Vector3(size, height, size) / 2;
        transform.position = pos;
    }
}
