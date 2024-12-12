using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SelectCursor : MonoBehaviour
{
    [SerializeField] LayerMask m_selectionLayer;
    [SerializeField] LayerMask m_hoverLayer;
    [SerializeField] float m_durationToDisplayHover = 0.5f;
    [SerializeField] GameObject m_selectionCornerPrefab;

    bool m_enabled = false;

    GameObject m_hoveredObject = null;
    float m_hoveredDuration = 0;

    bool m_selectionStarted = false;
    Vector3 m_selectionStart;
    Vector3 m_selectionEnd;

    List<OneSelection> m_selectedObjects = new List<OneSelection>();

    class OneSelection
    {
        public GameObject obj;

        Collider m_collider;
        List<GameObject> m_corners = new List<GameObject>();

        public void Init(GameObject owner, GameObject prefab)
        {
            m_collider = obj.GetComponent<Collider>();
            if (m_collider == null || prefab == null)
                return;

            for(int i = 0; i < 8; i++)
            {
                var c = Instantiate(prefab);
                c.transform.parent = owner.transform;
                m_corners.Add(c);
            }
        }

        public void OnDestroy()
        {
            foreach (var c in m_corners)
                Destroy(c);
        }

        public void Update()
        {
            if (m_collider == null)
                return;

            var bounds = m_collider.bounds;

            var min = bounds.min;
            var max = bounds.max;

            if (m_corners.Count < 8)
                return;

            UpdateOneCorner(m_corners[0], min, Rotation.rot_270, false);
            UpdateOneCorner(m_corners[1], new Vector3(min.x, min.y, max.z), Rotation.rot_180, false);
            UpdateOneCorner(m_corners[2], new Vector3(max.x, min.y, max.z), Rotation.rot_90, false);
            UpdateOneCorner(m_corners[3], new Vector3(max.x, min.y, min.z), Rotation.rot_0, false);

            UpdateOneCorner(m_corners[4], new Vector3(min.x, max.y, min.z), Rotation.rot_180, true);
            UpdateOneCorner(m_corners[5], new Vector3(min.x, max.y, max.z), Rotation.rot_90, true);
            UpdateOneCorner(m_corners[6], max, Rotation.rot_0, true);
            UpdateOneCorner(m_corners[7], new Vector3(max.x, max.y, min.z), Rotation.rot_270, true);
        }

        void UpdateOneCorner(GameObject corner, Vector3 pos, Rotation rot, bool rotateVertical)
        {
            if (corner == null)
                return;

            corner.transform.position = pos;

            var rotation = RotationEx.ToQuaternion(rot);
            if (rotateVertical)
                rotation *= Quaternion.Euler(180, 0, 0);
            corner.transform.rotation = rotation;
        }
    }

    public void SetCursorEnabled(bool enabled)
    {
        m_enabled = enabled;

        if(!m_enabled)
        {
            m_hoveredObject = null;
            m_hoveredDuration = 0;

            foreach (var s in m_selectedObjects)
                s.OnDestroy();
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

        foreach (var s in m_selectedObjects)
            s.Update();
    }

    void UpdateHovered()
    {
        var camera = new GetCameraEvent();
        Event<GetCameraEvent>.Broadcast(camera);
        if (camera.camera == null)
            return;

        var ray = camera.camera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        bool haveHit = Physics.Raycast(ray, out hit, float.MaxValue, m_hoverLayer.value);

        GameObject newTarget = null;
        if(haveHit)
            newTarget = hit.collider.gameObject;

        if(CustomLightsManager.instance != null && newTarget != null)
        {
            GetTeamEvent team = new GetTeamEvent();
            Event<GetTeamEvent>.Broadcast(team, newTarget);

            if (team.team != Team.Player)
            {
                if (!CustomLightsManager.instance.IsPosVisible(newTarget.transform.position))
                    newTarget = null;
            }
        }

        if(newTarget != m_hoveredObject)
        {
            m_hoveredDuration = 0;
            m_hoveredObject = newTarget;
            Event<SetHoveredObjectEvent>.Broadcast(new SetHoveredObjectEvent(null));
        }

        if (m_hoveredObject != null)
        {
            float nextTime = m_hoveredDuration + Time.deltaTime;

            if (nextTime >= m_durationToDisplayHover && m_hoveredDuration < m_durationToDisplayHover)
            {
                Event<SetHoveredObjectEvent>.Broadcast(new SetHoveredObjectEvent(m_hoveredObject));
            }
            m_hoveredDuration = nextTime;
        }
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
                m_selectionStarted = false;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Event<HideSelectionBoxEvent>.Broadcast(new HideSelectionBoxEvent());

            foreach (var s in m_selectedObjects)
                s.OnDestroy();
            m_selectedObjects.Clear();
            m_selectionStarted = false;
        }

        if (Input.GetKeyDown(KeyCode.Delete))
            DestroySelection();
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
            bool haveHit = Physics.Raycast(ray, out hit, 1000, m_selectionLayer.value);

            GameObject newTarget = null;
            if (haveHit && IsSelectionValid(hit.collider.gameObject))
                newTarget = hit.collider.gameObject;

            if(newTarget != null)
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

        for(int i = 0; i < newSelection.Count; i++)
        {
            if (!m_selectedObjects.Exists(x => { return x.obj == newSelection[i]; }))
            {
                var s = new OneSelection();
                s.obj = newSelection[i];
                s.Init(gameObject, m_selectionCornerPrefab);
                m_selectedObjects.Add(s);
            }
        }

        for(int i = 0; i < m_selectedObjects.Count; i++)
        {
            if(!newSelection.Contains(m_selectedObjects[i].obj))
            {
                m_selectedObjects[i].OnDestroy();
                m_selectedObjects.RemoveAt(i);
                i--;
            }
        }
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
        rect = rect.Encapsulate(cam.WorldToScreenPoint(boundsMax));
        rect = rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMin.x, boundsMin.y, boundsMax.z)));
        rect = rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMax.x, boundsMin.y, boundsMax.z)));
        rect = rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMin.x, boundsMax.y, boundsMax.z)));
        rect = rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMin.x, boundsMax.y, boundsMin.z)));
        rect = rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMax.x, boundsMin.y, boundsMin.z)));
        rect = rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMax.x, boundsMax.y, boundsMin.z)));

        return rect.Overlaps(selRect);
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

    void DestroySelection()
    {
        foreach(var s in m_selectedObjects)
        {
            if (s.obj != null)
            {
                bool canDestroy = false;
                var type = GameSystem.GetEntityType(s.obj);
                switch(type)
                {
                    case EntityType.Building:
                        var b = s.obj.GetComponent<BuildingBase>();
                        if (b == null || b.GetBuildingType() != BuildingType.Tower)
                            canDestroy = true;
                        break;
                    default:
                        break;
                }

                if (canDestroy)
                    Destroy(s.obj);
            }

            s.OnDestroy();
        }

        m_selectedObjects.Clear();
    }
}

