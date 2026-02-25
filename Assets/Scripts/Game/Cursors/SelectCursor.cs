using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SelectCursor : MonoBehaviour, CursorInterface
{
    [SerializeField] LayerMask m_selectionLayer;
    [SerializeField] LayerMask m_hoverLayer;
    [SerializeField] float m_durationToDisplayHover = 0.5f;
    [SerializeField] GameObject m_selectionCornerPrefab;
    [SerializeField] string m_removeBuildingSound;
    [SerializeField] float m_removeBuildingSoundVolume = 1;

    bool m_enabled = false;

    GameObject m_hoveredObject = null;
    float m_hoveredDuration = 0;

    bool m_selectionStarted = false;
    Vector3 m_selectionStart;
    Vector3 m_selectionEnd;

    List<OneSelection> m_selectedObjects = new List<OneSelection>();

    GameObject m_popup;

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

            HidePopup();
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

        if (m_popup != null)
            UpdatePopup();
    }

    void UpdateHovered()
    {
        var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (camera.camera == null)
            return;

        var ray = camera.camera.ScreenPointToRay(Input.mousePosition);

        GameObject newTarget = LoopHoverRatcast(ray, m_hoverLayer);

        if(CustomLightsManager.instance != null && newTarget != null)
        {
            var team = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), newTarget);

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

    public static GameObject LoopHoverRatcast(Ray ray, LayerMask layer)
    {
        bool haveHit = false;
        GameObject bestTarget = null;
        float bestDistance = float.MaxValue;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());

        RaycastHit hit;

        if (grid.grid == null)
        {
            haveHit = Physics.Raycast(ray, out hit, float.MaxValue, layer.value);
            if (!haveHit)
                return null;
            return hit.collider.gameObject;
        }

        var dups = Event<GetCameraDuplicationEvent>.Broadcast(new GetCameraDuplicationEvent());

        int size = GridEx.GetRealSize(grid.grid);

        foreach (var d in dups.duplications)
        {
            var tempRay = new Ray(ray.origin - new Vector3(d.x * size, 0, d.y * size), ray.direction);
            bool tempHit = Physics.Raycast(tempRay, out hit, float.MaxValue, layer.value);
            if (!tempHit)
                continue;

            haveHit = tempHit;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                bestTarget = hit.collider.gameObject;
            }
        }

        return bestTarget;
    }

    void UpdateSelection()
    {
        if(Input.GetMouseButtonDown(0))
        {
            IsMouseOverUIEvent overUI = Event<IsMouseOverUIEvent>.Broadcast(new IsMouseOverUIEvent());
            if (!overUI.overUI)
            {
                m_selectionStart = Input.mousePosition;
                m_selectionEnd = Input.mousePosition;
                m_selectionStarted = true;
                HidePopup();
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

            if (Input.GetMouseButtonUp(0) || !Application.isFocused)
            {
                Event<HideSelectionBoxEvent>.Broadcast(new HideSelectionBoxEvent());
                m_selectionStarted = false;
                DisplayPopup();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Event<HideSelectionBoxEvent>.Broadcast(new HideSelectionBoxEvent());

            foreach (var s in m_selectedObjects)
                s.OnDestroy();
            m_selectedObjects.Clear();
            m_selectionStarted = false;
            HidePopup();
        }

        if (Input.GetKeyDown(KeyCode.Delete))
            DestroySelection();
    }

    void UpdateSelectedObjects()
    {
        List<GameObject> newSelection = new List<GameObject>();

        var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (camera.camera == null)
            return;

        float dist = (m_selectionStart - m_selectionEnd).sqrMagnitude;
        if(dist < 2)
        {
            var pos = (m_selectionStart + m_selectionEnd) / 2;

            var ray = camera.camera.ScreenPointToRay(pos);
            GameObject newTarget = LoopHoverRatcast(ray, m_hoverLayer);

            if (newTarget != null && !IsSelectionValid(newTarget))
                newTarget = null;

            if(newTarget != null)
                newSelection.Add(newTarget);
        }
        else
        {
            if(BuildingList.instance != null)
            {
                var dups = Event<GetCameraDuplicationEvent>.Broadcast(new GetCameraDuplicationEvent());

                var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());

                if (grid.grid != null)
                {
                    float size = GridEx.GetRealSize(grid.grid);

                    int nbBuilding = BuildingList.instance.GetBuildingNb();
                    for (int i = 0; i < nbBuilding; i++)
                    {
                        var b = BuildingList.instance.GetBuildingFromIndex(i);

                        var team = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), b.gameObject);
                        if (team.team != Team.Player)
                            continue;

                        if (IsOnSelection(b.gameObject, camera.camera, dups.duplications, size))
                            newSelection.Add(b.gameObject);
                    }
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

    bool IsOnSelection(GameObject obj, Camera cam, List<Vector2Int> dups, float size)
    {
        var collider = obj.GetComponent<Collider>();
        if (collider == null)
            return false;

        var selMin = new Vector3(Mathf.Min(m_selectionStart.x, m_selectionEnd.x), Mathf.Min(m_selectionStart.y, m_selectionEnd.y), Mathf.Min(m_selectionStart.z, m_selectionEnd.z));
        var selMax = new Vector3(Mathf.Max(m_selectionStart.x, m_selectionEnd.x), Mathf.Max(m_selectionStart.y, m_selectionEnd.y), Mathf.Max(m_selectionStart.z, m_selectionEnd.z));
        var selRect = new Rect(selMin, selMax - selMin);

        var bounds = collider.bounds;
        foreach (var d in dups)
        {
            var boundsMin = bounds.min + new Vector3(d.x, 0, d.y) * size;
            var boundsMax = bounds.max + new Vector3(d.x, 0, d.y) * size;

            var pos1 = cam.WorldToScreenPoint(boundsMin);
            var rect = new Rect(pos1, Vector2.zero);
            rect = rect.Encapsulate(cam.WorldToScreenPoint(boundsMax));
            rect = rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMin.x, boundsMin.y, boundsMax.z)));
            rect = rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMax.x, boundsMin.y, boundsMax.z)));
            rect = rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMin.x, boundsMax.y, boundsMax.z)));
            rect = rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMin.x, boundsMax.y, boundsMin.z)));
            rect = rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMax.x, boundsMin.y, boundsMin.z)));
            rect = rect.Encapsulate(cam.WorldToScreenPoint(new Vector3(boundsMax.x, boundsMax.y, boundsMin.z)));

            if (rect.Overlaps(selRect))
                return true;
        }

        return false;
    }

    bool IsSelectionValid(GameObject obj)
    {
        var type = GameSystem.GetEntityType(obj);

        var team = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), obj);
        if (team.team != Team.Player)
            return false;

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
                        if (b == null || (b.GetBuildingType() != BuildingType.Tower && b.GetTeam() == Team.Player))
                        {
                            canDestroy = true;
                            Event<OnBuildingRemovedEvent>.Broadcast(new OnBuildingRemovedEvent(b.GetBuildingType()));
                        }
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

        if(SoundSystem.instance != null)
        {
            SoundSystem.instance.PlaySoundUI(m_removeBuildingSound, m_removeBuildingSoundVolume);
        }

        HidePopup();
    }

    void DisplayPopup()
    {
        HidePopup();

        if(m_selectedObjects.Count != 1)
            return;

        if(m_selectedObjects[0].obj == null)
            return;

        var building = m_selectedObjects[0].obj.GetComponent<BuildingBase>();
        if(building == null)
            return;

        var prefab = building.GetSelectionPopupPrefab();
        if(prefab == null)
            return;

        var canvas = Event<GetCanvasEvent>.Broadcast(new GetCanvasEvent()).canvas;
        if (canvas == null)
            return;

        var obj = Instantiate(prefab);
        var popup = obj.GetComponent<EntityPopupBase>();
        if (popup != null)
            popup.SetEntity(m_selectedObjects[0].obj);
        obj.transform.SetParent(canvas.transform, false);
        m_popup = obj;
        UpdatePopup();
    }

    void HidePopup()
    {
        if(m_popup != null)
        {
            Destroy(m_popup);
            m_popup = null;
        }
    }

    void UpdatePopup()
    {
        if (m_selectedObjects.Count == 0 || m_selectedObjects[0].obj == null)
        {
            HidePopup();
            return;
        }

        var building = m_selectedObjects[0].obj.GetComponent<BuildingBase>();
        if(building == null)
        {
            HidePopup();
            return;
        }

        var canvas = Event<GetCanvasEvent>.Broadcast(new GetCanvasEvent()).canvas;
        if (canvas == null)
            return;
        var canvasRect = canvas.GetComponent<RectTransform>();

        var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (camera.UICamera == null)
            return;

        var popup = m_popup.GetComponent<EntityPopupBase>();
        if (m_popup == null)
            return;

        float width = Screen.width;
        float height = Screen.height;

        var pos = building.GetGroundCenter();
        var bounds = building.GetBounds();
        pos.y += bounds.size.y;

        Vector2 screenPos = camera.UICamera.WorldToScreenPoint(pos);
        var rectTransform = m_popup.GetComponent<RectTransform>();
        if (rectTransform == null)
            return;

        var cam = camera.UICamera;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            cam = null;

        Vector2 screenMin, screenMax;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Vector2.zero, cam, out screenMin);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, new Vector2(width, height), cam, out screenMax);

        Vector2 popupPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out popupPos);

        popupPos.y += building.GetSelectionPopupOffset();

        var popupBounds = popup.GetBounds();
        Vector2 popupMin = popupPos + popupBounds.min;
        Vector2 popupMax = popupPos + popupBounds.max;

        Vector2 offset = Vector2.zero;
        if (popupMin.x < screenMin.x)
            offset.x += screenMin.x - popupMin.x;
        if (popupMin.y < screenMin.y)
            offset.y += screenMin.y - popupMin.y;
        if (popupMax.x > screenMax.x)
            offset.x += screenMax.x - popupMax.x;
        if (popupMax.y > screenMax.y)
            offset.y += screenMax.y - popupMax.y;

        rectTransform.anchoredPosition = popupPos + offset;
    }
}

