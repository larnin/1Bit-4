using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SelectCursor : MonoBehaviour
{
    [SerializeField] LayerMask m_selectionLayer;
    [SerializeField] float m_durationToDisplayHover = 0.5f;

    bool m_enabled = false;

    GameObject m_hoveredObject = null;
    float m_hoveredDuration = 0;

    bool m_selectionStarted = false;
    Vector3 m_selectionStart;
    Vector3 m_selectionEnd;

    List<GameObject> m_selectedObjects = new List<GameObject>();

    public void SetCursorEnabled(bool enabled)
    {
        m_enabled = enabled;

        if(!m_enabled)
        {
            m_hoveredObject = null;
            m_hoveredDuration = 0;
            m_selectedObjects.Clear();

            Event<SetHoveredObjectEvent>.Broadcast(new SetHoveredObjectEvent(null));
        }
    }

    public bool IsCursorEnabled()
    {
        return m_enabled;
    }

    private void Update()
    {
        if (!m_enabled)
            return;

        UpdateHovered();
        UpdateSelection();
    }

    void UpdateHovered()
    {
        var camera = new GetCameraEvent();
        Event<GetCameraEvent>.Broadcast(camera);
        if (camera.camera == null)
            return;

        var ray = camera.camera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        bool haveHit = Physics.Raycast(ray, out hit, float.MaxValue, m_selectionLayer.value);

        GameObject newTarget = null;
        if(haveHit && IsHoverValid(hit.collider.gameObject))
            newTarget = hit.collider.gameObject;

        if(newTarget != m_hoveredObject)
        {
            m_hoveredDuration = 0;
            Event<SetHoveredObjectEvent>.Broadcast(new SetHoveredObjectEvent(null));
        }

        float nextTime = m_hoveredDuration + Time.deltaTime;

        if(nextTime >= m_durationToDisplayHover && m_hoveredDuration < m_durationToDisplayHover)
        {
            Event<SetHoveredObjectEvent>.Broadcast(new SetHoveredObjectEvent(m_hoveredObject));
        }
        m_hoveredDuration = nextTime;
    }

    void UpdateSelection()
    {
        if(Input.GetMouseButtonDown(0))
        {
            IsMouseOverUIEvent overUI = new IsMouseOverUIEvent();
            Event<IsMouseOverUIEvent>.Broadcast(overUI);
            if (!overUI.overUI)
            {
                m_selectionStart = Input.mousePosition;
                m_selectionEnd = Input.mousePosition;
                m_selectionStarted = true;
            }
        }

        if (m_selectionStarted)
        {
            if (Input.GetMouseButton(0))
            {
                m_selectionEnd = Input.mousePosition;
                Event<DisplaySelectionBoxEvent>.Broadcast(new DisplaySelectionBoxEvent(m_selectionStart, m_selectionEnd));
                UpdateSelectedObjects();
            }

            if (Input.GetMouseButtonUp(0))
            {
                Event<HideSelectionBoxEvent>.Broadcast(new HideSelectionBoxEvent());

                m_selectedObjects.Clear();
                m_selectionStarted = false;
            }
        }
    }

    void UpdateSelectedObjects()
    {
        List<GameObject> newSelection = new List<GameObject>();

        var camera = new GetCameraEvent();
        Event<GetCameraEvent>.Broadcast(camera);
        if (camera.camera == null)
            return;

        float dist = (m_selectionStart - m_selectionEnd).sqrMagnitude;
        if(dist < 2)
        {
            var pos = (m_selectionStart + m_selectionEnd) / 2;

            var ray = camera.camera.ScreenPointToRay(pos);
            RaycastHit hit;
            bool haveHit = Physics.Raycast(ray, out hit, float.MaxValue, m_selectionLayer.value);

            GameObject newTarget = null;
            if (haveHit && IsSelectionValid(hit.collider.gameObject))
                newTarget = hit.collider.gameObject;

            newSelection.Add(newTarget);
        }
        else
        {
            if(BuildingList.instance != null)
            {
                int nbBuilding = BuildingList.instance.GetBuildingNb();
                for(int i = 0; i < nbBuilding; i++)
                {
                    var b = BuildingList.instance.GetBuildingFromIndex(i);
                    if (IsOnSelection(b.gameObject, camera.camera))
                        newSelection.Add(b.gameObject);
                }
            }
        }

        m_selectedObjects = newSelection;
    }

    bool IsOnSelection(GameObject obj, Camera cam)
    {
        var collider = obj.GetComponent<Collider>();
        if (collider == null)
            return false;

        var selMin = new Vector3(Mathf.Min(m_selectionStart.x, m_selectionEnd.x), Mathf.Min(m_selectionStart.y, m_selectionEnd.y), Mathf.Min(m_selectionStart.z, m_selectionEnd.z));
        var selMax = new Vector3(Mathf.Max(m_selectionStart.x, m_selectionEnd.x), Mathf.Max(m_selectionStart.y, m_selectionEnd.y), Mathf.Max(m_selectionStart.z, m_selectionEnd.z));
        var selRect = new Rect(selMin, selMax - selMin);

        var bounds = collider.bounds;
        var boundsMin = bounds.min;
        var boundsMax = bounds.max;

        var pos1 = cam.WorldToScreenPoint(boundsMin);
        var rect = new Rect(pos1, Vector2.zero);
        rect.Encapsulate(cam.WorldToScreenPoint(boundsMax));
        rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMin.x, boundsMin.y, boundsMax.z)));
        rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMax.x, boundsMin.y, boundsMax.z)));
        rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMin.x, boundsMax.y, boundsMax.z)));
        rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMin.x, boundsMax.y, boundsMin.z)));
        rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMax.x, boundsMin.y, boundsMin.z)));
        rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMax.x, boundsMax.y, boundsMin.z)));

        return rect.Overlaps(selRect);
    }

    bool IsHoverValid(GameObject obj)
    {
        var type = GameSystem.GetEntityType(obj);

        switch(type)
        {
            case EntityType.Building:
            case EntityType.Ennemy:
            case EntityType.Spawner:
                return true;
            default:
                return false;
        }
    }

    bool IsSelectionValid(GameObject obj)
    {
        var type = GameSystem.GetEntityType(obj);

        switch(type)
        {
            case EntityType.Building:
                return true;
            default:
                return false;
        }
    }
}

