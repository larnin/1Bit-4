using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] float m_loadingDisplayTimer = 1;
    [SerializeField] float m_fadeDuration = 1;

    Image m_image;
    TMP_Text m_label;
    TMP_Text m_text;

    SubscriberList m_subscriberList = new SubscriberList();

    float m_loadingTimer = 0;
    int m_loadingCounter = 0;
    bool m_fadeStarted = false;
    float m_fadeTimer = 0;

    private void Awake()
    {
        var labelTr = transform.Find("Label");
        if (labelTr != null)
            m_label = labelTr.GetComponent<TMP_Text>();
        var textTr = transform.Find("Text");
        if (textTr != null)
            m_text = textTr.GetComponent<TMP_Text>();

        m_image = GetComponent<Image>();

        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(OnLoadEnd));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Update()
    {
        m_loadingTimer += Time.deltaTime;
        if(m_loadingTimer >= m_loadingDisplayTimer)
        {
            m_loadingTimer -= m_loadingDisplayTimer;
            m_loadingCounter++;
            if (m_loadingCounter > 3)
                m_loadingCounter = 0;
        }

        string labelText = "Loading ";
        for (int i = 0; i < m_loadingCounter; i++)
            labelText += '.';
        m_label.text = labelText;

        if (GameSystem.instance != null)
        {
            if (m_fadeStarted)
            {
                m_label.text = GameSystem.instance.GetStatus();
                m_label.alignment = TextAlignmentOptions.Center;
                m_text.gameObject.SetActive(false);
            }
            else
            {
                m_text.gameObject.SetActive(true);
                m_text.text = GameSystem.instance.GetStatus();
            }
        }
        else m_text.gameObject.SetActive(false);

        if(m_fadeStarted)
        {
            m_fadeTimer += Time.deltaTime;

            if (m_fadeTimer >= m_fadeDuration)
            {
                Event<ShowLoadingScreenEvent>.Broadcast(new ShowLoadingScreenEvent(false));
                Destroy(gameObject);
            }
        }
    }

    void OnLoadEnd(GenerationFinishedEvent e)
    {
        m_fadeStarted = true;
    }
}

