using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NRand;

enum ScreenShakeSourceType
{
    Position,
    GameObject,
    Infinite,
}

public class ScreenShakeBehaviour : MonoBehaviour
{
    class ScreenShakeData
    {
        public int id;
        public ScreenShakeBase shake;
        public ScreenShakeData(int _id, ScreenShakeBase _shake)
        {
            id = _id;
            shake = _shake;
        }
    }

    List<ScreenShakeData> m_screenShakes = new List<ScreenShakeData>();
    SubscriberList m_subsctiberList = new SubscriberList();

    Camera m_camera = null;
    float m_initialCameraSize = 0;
    Vector3 m_initialCameraPosition = Vector3.zero;
    int m_nextShakeID;

    MT19937 m_rand = new MT19937();

    private void Awake()
    {
        m_camera = GetComponentInChildren<Camera>();

        m_subsctiberList.Add(new Event<AddScreenShakeEvent>.Subscriber(OnAddShake));
        m_subsctiberList.Add(new Event<StopScreenShakeEvent>.Subscriber(OnStopShake));
        m_subsctiberList.Add(new Event<StopAllScreenShakeEvent>.Subscriber(OnStopAllShake));
        m_subsctiberList.Add(new Event<IsScreenShakePlayingEvent>.Subscriber(IsPlaying));
        m_subsctiberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subsctiberList.Unsubscribe();
    }

    private void Start()
    {
        m_initialCameraSize = m_camera.orthographicSize;
        m_initialCameraPosition = transform.localPosition;
    }

    void Update()
    {
        Vector2 offset = Vector2.zero;
        float scale = 0;
        float rotation = 0;

        float t = Time.deltaTime;

        foreach (var shake in m_screenShakes)
        {
            shake.shake.Update(t, m_rand);

            offset += shake.shake.GetOffset();
            scale += shake.shake.GetScale();
            rotation += shake.shake.GetRotation();
        }

        m_screenShakes.RemoveAll(x => { return x.shake.IsEnded(); });

        transform.localPosition = m_initialCameraPosition + new Vector3(offset.x, offset.y, 0);
        transform.localRotation = Quaternion.Euler(0, 0, rotation);
        m_camera.orthographicSize = m_initialCameraSize + scale;
    }

    void OnAddShake(AddScreenShakeEvent e)
    {
        e.resultID = m_nextShakeID;
        e.screenShake.Start();
        m_screenShakes.Add(new ScreenShakeData(m_nextShakeID, e.screenShake));
        m_nextShakeID++;
    }

    void OnStopShake(StopScreenShakeEvent e)
    {
        m_screenShakes.RemoveAll(x => { return x.id == e.ID; });
    }

    void OnStopAllShake(StopAllScreenShakeEvent e)
    {
        m_screenShakes.Clear();
    }

    void IsPlaying(IsScreenShakePlayingEvent e)
    {
        e.playing = m_screenShakes.Exists(x => { return x.id == e.ID; });
    }
}
