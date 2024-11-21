
using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    [SerializeField] float m_transitionDuration = 0.5f;
    [SerializeField] Color m_color = Color.black;

    Image m_render;
    Tweener m_currentTween;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<ShowLoadingScreenEvent>.Subscriber(OnFade));
        m_subscriberList.Add(new Event<HideLoadingScreenEvent>.Subscriber(OnStopFade));
        m_subscriberList.Subscribe();

        m_render = GetComponentInChildren<Image>(true);

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
        if (e.start)
        {
            Color initialColor = m_color;
            initialColor.a = 0;
            m_render.color = initialColor;
            gameObject.SetActive(true);
            m_currentTween = m_render.DOColor(m_color, m_transitionDuration);
        }
        else
        {
            gameObject.SetActive(true);
            Color targetColor = m_color;
            targetColor.a = 0;
            m_currentTween = m_render.DOColor(targetColor, m_transitionDuration).OnComplete(() => 
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
}
