﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameInterface : MonoBehaviour
{
    class BuildingButton
    {
        public BuildingType buildingType;
        public RectTransform button;
    }

    [SerializeField] GameObject m_buildingsButtonPrefab;
    [SerializeField] float m_oneBuildingSize;
    [SerializeField] float m_buildingStartOffset;
    [SerializeField] float m_buildingListMoreSpace;
    [SerializeField] PlaceBuildingCursor m_buildingCursor;
    [SerializeField] SelectCursor m_selectCursor;
    [SerializeField] BuildingDetailDisplay m_detail;

    RectTransform m_buildingsBackground;
    List<BuildingButton> m_buildingButtons = new List<BuildingButton>();

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<IsMouseOverUIEvent>.Subscriber(IsMouseOverUI));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        var backgroundTransform = transform.Find("BuildingsBackground");
        if (backgroundTransform != null)
            m_buildingsBackground = backgroundTransform.GetComponent<RectTransform>();
    }

    public void OnClickBuilding(BuildingType type)
    {
        if (m_buildingCursor == null)
            return;

        m_buildingCursor.SetBuildingType(type);
        m_selectCursor.SetCursorEnabled(false);
    }

    public void OnHoverBuilding(BuildingType type)
    {
        m_detail.SetBuilding(type);
    }

    public void OnHoverEnd()
    {
        m_detail.SetDisabled();
    }

    public void OnClickOptions()
    {
        if (GameSystem.instance != null && !GameSystem.instance.IsLoaded())
            return;

        m_selectCursor.SetCursorEnabled(false);
        m_buildingCursor.SetCursorDisabled();

        if (MenuSystem.instance == null)
            return;

        MenuSystem.instance.OpenMenu<PauseMenu>("Pause");
    }

    private void Update()
    {
        UpdateBuildingsButtons();

        if (!GameInfos.instance.paused)
        {
            if (m_buildingCursor != null && m_selectCursor != null)
            {
                if (!m_buildingCursor.IsCursorEnabled() && !m_selectCursor.IsCursorEnabled())
                    m_selectCursor.SetCursorEnabled(true);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
                OnClickOptions();
        }
    }

    void UpdateBuildingsButtons()
    {
        if (m_buildingsBackground == null)
            return;

        if (GameSystem.instance == null)
            return;

        var buildingList = GameSystem.instance.GetUnlockedBuildings();

        //add new buttons
        foreach(var b in buildingList)
        {
            if (m_buildingButtons.Exists(x => x.buildingType == b))
                continue;

            var button = CreateBuildingButton(b);
            if (button == null)
                continue;

            BuildingButton buildingButton = new BuildingButton();
            buildingButton.button = button.GetComponent<RectTransform>();
            buildingButton.buildingType = b;

            m_buildingButtons.Add(buildingButton);
        }

        //remove old buttons
        for (int i = 0; i < m_buildingButtons.Count; i++)
        {
            if(!buildingList.Contains(m_buildingButtons[i].buildingType))
            {
                Destroy(m_buildingButtons[i].button.gameObject);
                m_buildingButtons.RemoveAt(i);
                i--;
            }
        }

        //sort buttons
        List<BuildingButton> newButtons = new List<BuildingButton>();
        foreach(var b in buildingList)
        {
            var button = m_buildingButtons.Find(x => x.buildingType == b);
            if (button != null)
                newButtons.Add(button);
        }
        m_buildingButtons = newButtons;

        //update display
        float width = m_buildingButtons.Count * m_oneBuildingSize + m_buildingListMoreSpace;
        m_buildingsBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        for(int i = 0; i < m_buildingButtons.Count; i++)
        {
            float posX = i * m_oneBuildingSize + m_buildingStartOffset;

            m_buildingButtons[i].button.anchoredPosition = new Vector2(posX, 0);
        }
    }

    GameObject CreateBuildingButton(BuildingType type)
    {
        if (m_buildingsButtonPrefab == null)
            return null;

        var obj = Instantiate(m_buildingsButtonPrefab);
        obj.transform.SetParent(m_buildingsBackground, false);

        var button = obj.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(() => { OnClickBuilding(type); });

        var trigger = obj.AddComponent<EventTrigger>();
        
        EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
        hoverEntry.eventID = EventTriggerType.PointerEnter;
        hoverEntry.callback.AddListener((data) => { OnHoverBuilding(type); });
        trigger.triggers.Add(hoverEntry);

        EventTrigger.Entry endEntry = new EventTrigger.Entry();
        endEntry.eventID = EventTriggerType.PointerExit;
        endEntry.callback.AddListener((data) => { OnHoverEnd(); });
        trigger.triggers.Add(endEntry);

        var imgTransform = obj.transform.Find("Image");
        if(imgTransform != null)
        {
            var img = imgTransform.GetComponent<Image>();
            if(img != null)
            {
                var buildingData = Global.instance.buildingDatas.GetBuilding(type);
                if (buildingData != null)
                    img.sprite = buildingData.sprite;
            }
        }

        return obj;
    }

    void IsMouseOverUI(IsMouseOverUIEvent e)
    {
        e.overUI = false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);

        for (int index = 0; index < raysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = raysastResults[index];

            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                e.overUI = true;
                break;
            }
        }
    }
}
