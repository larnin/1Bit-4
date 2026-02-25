using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NRand;


public class ScreenShakeBehaviour : MonoBehaviour
{
    enum ScreenShakeSourceType
    {
        Position,
        GameObject,
        Infinite,
    }

    class ScreenShakeData
    {
        public int id;
        public ScreenShakeAsset asset;
        public ScreenShakePlayData data = new ScreenShakePlayData();

        public ScreenShakeSourceType sourceType = ScreenShakeSourceType.Infinite;
        public Vector3 sourcePosition;
        public GameObject sourceObject;

        public ScreenShakeData(int _id, ScreenShakeAsset _asset, float intensity, float power, float overrideDuration)
        {
            id = _id;
            asset = _asset;

            data.seed = (int)StaticRandomGenerator<MT19937>.Get().Next();
            data.overrideDuration = overrideDuration;
            data.intensity = intensity;
            data.power = power;

            sourceType = ScreenShakeSourceType.Infinite;
        }

        public void SetPosition(Vector3 pos)
        {
            sourceType = ScreenShakeSourceType.Position;
            sourcePosition = pos;
        }

        public void SetObject(GameObject obj)
        {
            sourceType = ScreenShakeSourceType.GameObject;
            sourceObject = obj;
        }
    }

    [SerializeField] Camera m_camera = null;
    [SerializeField] float m_intensityAtMinDistance = 1;
    [SerializeField] float m_intensityAtMaxDistance = 0.1f;
    [SerializeField] float m_powerMaxEffectDistance = 0.5f;

    static ScreenShakeBehaviour m_instance;
    public static ScreenShakeBehaviour instance { get { return m_instance; } }

    List<ScreenShakeData> m_screenShakes = new List<ScreenShakeData>();

    int m_nextShakeID;

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    void Update()
    {
        float globalIntensity = GameInfos.instance.settings.GetScreenShakeIntensity();
        float sizeIntensity = 0;

        Vector2 positionOffset = Vector2.zero;
        float rotationOffset = 0;
        float sizeOffset = 0;

        var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent()).camera;
        bool isCameraValid = camera != null;
        float orthoSize = 0;
        if (isCameraValid)
        {
            isCameraValid = camera.orthographic;
            orthoSize = camera.orthographicSize;

            float sizePercent = Event<GetCurrentIsoSizePercentEvent>.Broadcast(new GetCurrentIsoSizePercentEvent()).isoSizePercent;
            sizeIntensity = sizePercent * m_intensityAtMinDistance + (1 - sizePercent) * m_intensityAtMaxDistance;
        }

        float t = Time.deltaTime;

        foreach (var shake in m_screenShakes)
        {
            shake.data.time += t;

            float distance = GetDistanceTo(shake);

            float distanceIntensity = 0;
            if (orthoSize > 0)
                distanceIntensity = 1 - (distance * shake.data.power / orthoSize);
            if (distanceIntensity < 0)
                distanceIntensity = 0;
            if (distanceIntensity > 1 - m_powerMaxEffectDistance)
                distanceIntensity = 1;
            else if (m_powerMaxEffectDistance > 0.999f)
                distanceIntensity = 0;
            else distanceIntensity /= (1 - m_powerMaxEffectDistance);
            float totalIntensity = distanceIntensity * globalIntensity * sizeIntensity;

            if (totalIntensity > 0)
            {
                positionOffset += shake.asset.GetOffset(shake.data) * totalIntensity;
                rotationOffset += shake.asset.GetRotation(shake.data) * totalIntensity;
                sizeOffset += shake.asset.GetOrthographicSizeOffset(shake.data) * totalIntensity;
            }
        }

        m_screenShakes.RemoveAll(x => { return x.asset.IsCompleted(x.data); });

        transform.localPosition = new Vector3(positionOffset.x, 0, positionOffset.y);
        transform.localRotation = Quaternion.Euler(0, 0, rotationOffset);

        Event<SetShakeIsoSizeOffsetEvent>.Broadcast(new SetShakeIsoSizeOffsetEvent(sizeOffset));
    }

    public int StartShake(ScreenShakeAsset asset, float intensity = 1, float power = 1, float overrideDuration = -1)
    {
        var result = m_nextShakeID;
        m_nextShakeID++;

        m_screenShakes.Add(new ScreenShakeData(result, asset, intensity, power, overrideDuration));

        return result;
    }

    public int StartShake(ScreenShakeAsset asset, Vector3 position, float intensity = 1, float power = 1, float overrideDuration = -1)
    {
        var result = m_nextShakeID;
        m_nextShakeID++;

        var data = new ScreenShakeData(result, asset, intensity, power, overrideDuration);
        data.SetPosition(position);

        m_screenShakes.Add(data);

        return result;
    }

    public int StartShake(ScreenShakeAsset asset, GameObject obj, float intensity = 1, float power = 1, float overrideDuration = -1)
    {
        var result = m_nextShakeID;
        m_nextShakeID++;

        var data = new ScreenShakeData(result, asset, intensity, power, overrideDuration);
        data.SetObject(obj);

        m_screenShakes.Add(data);

        return result;
    }

    public void SetIntensity(int ID, float intensity)
    {
        var shake = GetFromID(ID);
        if (shake == null)
            return;

        shake.data.intensity = intensity;
    }

    public void SetPower(int ID, float power)
    {
        var shake = GetFromID(ID);
        if (shake == null)
            return;

        shake.data.power = power;
    }

    public void SetOverrideDuration(int ID, float duration)
    {
        var shake = GetFromID(ID);
        if (shake == null)
            return;

        shake.data.overrideDuration = duration;
    }

    public bool IsPlaying(int ID)
    {
        return GetFromID(ID) != null;
    }

    public void StopShake(int ID)
    {
        m_screenShakes.RemoveAll((x) => { return x.id == ID; });
    }

    public void StopAllShakes()
    {
        m_screenShakes.Clear();
    }

    ScreenShakeData GetFromID(int id)
    {
        return m_screenShakes.Find((x) => { return x.id == id; });
    }

    float GetDistanceTo(ScreenShakeData data)
    {
        if (m_camera == null)
            return 0;

        if (data.sourceType == ScreenShakeSourceType.Infinite)
            return 0;

        Vector3 pos = data.sourcePosition;
        if (data.sourceType == ScreenShakeSourceType.GameObject)
            pos = data.sourceObject.transform.position;

        Vector3 cameraPos = m_camera.transform.position;
        Vector3 dir = m_camera.transform.forward;

        Vector3 posOnDir = cameraPos + Vector3.Project(pos - cameraPos, dir);

        return (pos - posOnDir).magnitude;
    }
}
