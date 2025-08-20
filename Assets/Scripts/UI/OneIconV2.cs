using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OneIconV2 : MonoBehaviour
{
    Transform m_arrow;
    Image m_icon;
    TMP_Text m_text;

    RectTransform m_current;
    RectTransform m_parent;

    private void Awake()
    {
        m_arrow = transform.Find("Arrow");
        var iconTransform = transform.Find("Icon");
        if (iconTransform != null)
            m_icon = iconTransform.GetComponent<Image>();
        var textTransform = transform.Find("Text");
        if (textTransform != null)
            m_text = textTransform.GetComponent<TMP_Text>();

        m_text.raycastTarget = false;

        m_current = GetComponent<RectTransform>();
    }

    public void SetData(Vector2 pos, Sprite icon, bool arrow, Color color, string text, Camera camera)
    {
        if (m_parent == null)
        {
            var parent = transform.parent;
            if (parent != null)
                m_parent = parent.GetComponent<RectTransform>();
        } 

        if (m_icon != null)
        {
            m_icon.enabled = icon != null;

            if(icon != null)
                m_icon.sprite = icon;
            m_icon.color = color;
        }

        if(m_text != null)
            m_text.text = text;

        if(m_arrow != null)
        {
            m_arrow.gameObject.SetActive(arrow);

            if(arrow)
            {
                float angle = Vector2.SignedAngle(new Vector2(0, 1), pos - new Vector2(Screen.width / 2, Screen.height / 2));
                Quaternion rot = Quaternion.Euler(0, 0, angle);
                m_arrow.localRotation = rot;
            }
        }

        var canvas = m_parent.GetComponentInParent<Canvas>();

        Vector2 transformPoint;
        if(canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parent, pos, null, out transformPoint);
        else RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parent, pos, camera, out transformPoint);

        m_current.anchoredPosition = transformPoint;
    }
}
