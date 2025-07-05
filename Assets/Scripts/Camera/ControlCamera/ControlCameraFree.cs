using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class FreeCameraParams
{
    public float fov = 90;
    public float placeDistance = 20;

    public float arrowSpeed = 1;
    public float arrowAccelerationDuration = 0.2f;

    public float rotationSpeed = 1;
}

public class ControlCameraFree : ControlCameraBase
{
    FreeCameraParams m_params;

    Grid m_grid;

    Vector3 m_position;
    Vector3 m_seeDir;

    Vector3 m_initialSeeDir;

    Vector3 m_oldMousePos;
    bool m_wasFocused = false;

    float m_forward;
    float m_backward;
    float m_left;
    float m_right;
    float m_up;
    float m_down;

    public ControlCameraFree(FreeCameraParams camParams)
    {
        m_params = camParams;
    }

    public override void SetParent(GameCamera camera)
    {
        base.SetParent(camera);
        m_position = camera.GetMainCamera().transform.position;
        m_initialSeeDir = camera.GetMainCamera().transform.forward;
        m_seeDir = m_initialSeeDir;
    }

    public override void Enable()
    {
        base.Enable();

        MoveCamera(m_position);

        var camera = m_gameCamera.GetMainCamera();
        camera.orthographic = false;
        camera.fieldOfView = m_params.fov;
        camera.transform.forward = m_seeDir;
    }

    public override void Disable()
    {
        base.Disable();

    }

    public override void Update()
    {
        if (m_grid == null)
            m_grid = Event<GetGridEvent>.Broadcast(new GetGridEvent()).grid;

        var camera = m_gameCamera.GetMainCamera();

        if (GameInfos.instance.paused)
        {
            m_oldMousePos = Input.mousePosition;
            return;
        }

        bool addRight = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
        bool addLeft = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A);
        bool addForward = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W);
        bool addBackward = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
        bool addUp = Input.GetKey(KeyCode.Space);
        bool addDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        m_right += Time.deltaTime / m_params.arrowAccelerationDuration * (addRight ? 1 : -1);
        m_left += Time.deltaTime / m_params.arrowAccelerationDuration * (addLeft ? 1 : -1);
        m_forward += Time.deltaTime / m_params.arrowAccelerationDuration * (addForward ? 1 : -1);
        m_backward += Time.deltaTime / m_params.arrowAccelerationDuration * (addBackward ? 1 : -1);
        m_up += Time.deltaTime / m_params.arrowAccelerationDuration * (addUp ? 1 : -1);
        m_down += Time.deltaTime / m_params.arrowAccelerationDuration * (addDown ? 1 : -1);

        m_right = Mathf.Clamp01(m_right);
        m_left = Mathf.Clamp01(m_left);
        m_forward = Mathf.Clamp01(m_forward);
        m_backward = Mathf.Clamp01(m_backward);
        m_up = Mathf.Clamp01(m_up);
        m_down = Mathf.Clamp01(m_down);

        Vector3 inputDir = new Vector3(m_left - m_right, m_up - m_down, m_forward - m_backward);

        if (inputDir != Vector3.zero)
        {
            var forward = camera.transform.forward;
            var side = Vector3.Cross(forward, Vector3.up).normalized;

            Vector3 offset = (forward * inputDir.z + side * inputDir.x + Vector3.up * inputDir.y) * Time.deltaTime * m_params.arrowSpeed;

            Vector3 newPos = m_position + offset;
            MoveCamera(newPos);

            m_gameCamera.OnMove();
        }

        if (Input.GetMouseButton(2) && m_wasFocused && Application.isFocused)
        {
            Vector3 offset = Input.mousePosition - m_oldMousePos;

            float angleXZ = Mathf.Atan2(m_seeDir.z, m_seeDir.x);

            float horizontalMagnitude = new Vector2(m_seeDir.x, m_seeDir.z).magnitude;
            float angleY = Mathf.Atan2(m_seeDir.y, horizontalMagnitude);

            angleXZ -= offset.x * Time.deltaTime * m_params.rotationSpeed;
            angleY += offset.y * Time.deltaTime * m_params.rotationSpeed;
            angleY = Mathf.Clamp(angleY, -Mathf.PI / 2.01f, Mathf.PI / 2.01f);

            m_seeDir.y = Mathf.Sin(angleY);
            float xz = Mathf.Cos(angleY);
            m_seeDir.x = Mathf.Cos(angleXZ) * xz;
            m_seeDir.z = Mathf.Sin(angleXZ) * xz;

            camera.transform.forward = m_seeDir;
        }

        m_oldMousePos = Input.mousePosition;
        m_wasFocused = Application.isFocused;
    }

    void MoveCamera(Vector3 newPos)
    {
        if (m_isEnabled)
            m_gameCamera.transform.position = Vector3.zero;

        var camera = m_gameCamera.GetMainCamera();

        if (m_grid == null)
        {
            m_position = newPos;
            if(m_isEnabled)
                camera.transform.position = newPos;
            return;
        }

        newPos = GridEx.ClampPos(m_grid, newPos);

        m_position = newPos;
        
        if(m_isEnabled)
            camera.transform.position = newPos;
    }

    public override void MoveTo(Vector3 pos)
    {
        Vector3 targetPos = pos - m_params.placeDistance * m_initialSeeDir;
        MoveCamera(targetPos);

        m_gameCamera.OnMove();
    }

}
