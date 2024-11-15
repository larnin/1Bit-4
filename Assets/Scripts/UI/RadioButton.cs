using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadioButton : Selectable, IPointerClickHandler, IEventSystemHandler, ISubmitHandler
{
    [SerializeField] Image m_checkedImage;
    [SerializeField] bool m_checked = false;

    public RadioButtonCheckedEvent onCheck { get; set; }

    public class RadioButtonCheckedEvent : UnityEvent
    {
        public RadioButtonCheckedEvent() { }
    }

    protected override void Awake()
    {
        UpdateSprite();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSelect();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        OnSelect();
    }


    void UpdateSprite()
    {
        m_checkedImage.gameObject.SetActive(m_checked);
    }

    void OnSelect()
    {
        m_checked = true;
        UpdateSprite();
        UpdateNeighbors();
    }

    void UpdateNeighbors()
    {
        var parent = transform.parent;
        if (parent == null)
            return;

        for(int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            var button = child.GetComponent<RadioButton>();
            if (button == null)
                continue;
            if (button == this)
                continue;
            button.SetChecked(false);
        }
    }

    public bool IsChecked()
    {
        return m_checked;
    }

    public void SetChecked(bool value)
    {
        if(!m_checked && value)
        {
            if(onCheck != null)
                onCheck.Invoke();
        }

        m_checked = value;
        UpdateSprite();
    }
}

