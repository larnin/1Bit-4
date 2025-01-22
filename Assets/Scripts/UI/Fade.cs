
using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    const string PercentName = "_Percent";

    [SerializeField] Material m_material;
    [SerializeField] float m_transitionDuration = 0.5f;
    [SerializeField] Color m_color = Color.black;
    
    Tweener m_currentTween;

    SubscriberList m_subscriberList = new SubscriberList();

    static Fade m_instance = null;

    private void Awake()
    {
        if (m_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        m_instance = this;

        m_subscriberList.Add(new Event<ShowLoadingScreenEvent>.Subscriber(OnFade));
        m_subscriberList.Add(new Event<HideLoadingScreenEvent>.Subscriber(OnStopFade));
        m_subscriberList.Subscribe();

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void Start()
    {
        gameObject.SetActive(false);
    }
    
    void OnFade(ShowLoadingScreenEvent e)
    {
        UpdateCamera();

        if (e.start)
        {
            gameObject.SetActive(true);
            m_material.SetFloat(PercentName, 1);
            m_currentTween = m_material.DOFloat(0, PercentName, m_transitionDuration);
        }
        else
        {
            gameObject.SetActive(true);
            m_material.SetFloat(PercentName, 0);
            m_currentTween = m_material.DOFloat(1, PercentName, m_transitionDuration).OnComplete(() => 
            { 
                if(this != null && gameObject != null)
                    gameObject.SetActive(false); 
            });
        }
    }

    void OnStopFade(HideLoadingScreenEvent e)
    {
        if (m_currentTween != null)
            m_currentTween.Kill(false);

        gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateCamera();
    }

    void UpdateCamera()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            if (canvas.worldCamera != null)
                return;

            GetCameraEvent cameraEvent = new GetCameraEvent();
            Event<GetCameraEvent>.Broadcast(cameraEvent);

            canvas.worldCamera = cameraEvent.UICamera;
        }
    }
}
