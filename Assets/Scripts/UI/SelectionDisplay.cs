using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SelectionDisplay : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();
    Image m_image;
    RectTransform m_transform;

    private void Awake()
    {
        m_transform = GetComponent<RectTransform>();
        m_image = GetComponentInChildren<Image>();
        m_image.gameObject.SetActive(false);

        m_subscriberList.Add(new Event<DisplaySelectionBoxEvent>.Subscriber(DisplaySelection));
        m_subscriberList.Add(new Event<HideSelectionBoxEvent>.Subscriber(HideSelection));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void DisplaySelection(DisplaySelectionBoxEvent e)
    {
        var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (camera.camera == null)
            return;

        m_image.gameObject.SetActive(true);

        Vector2 pos1, pos2;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_transform, e.pos1, camera.camera, out pos1);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_transform, e.pos2, camera.camera, out pos2);

        Vector3 center = (pos1 + pos2) / 2;

        m_image.rectTransform.localPosition = center;
        
        float width = Mathf.Abs(pos1.x - pos2.x);
        float height = Mathf.Abs(pos1.y - pos2.y);
        m_image.rectTransform.sizeDelta = new Vector2(width, height);
    }

    void HideSelection(HideSelectionBoxEvent e)
    {
        m_image.gameObject.SetActive(false);
    }
}
