using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlaceWallCursor : MonoBehaviour, CursorInterface
{

    [SerializeField] LayerMask m_groundLayer;
    [SerializeField] PlaceBuildingCursorDecal m_decal;
    [SerializeField] string m_placeBuildingSound;
    [SerializeField] float m_placeBuildingSoundVolume = 1;

    bool m_enabled = false;
    bool m_onEditor = false;
    BuildingType m_type;
    bool m_cursorPressed = false;
    BuildingPlaceType m_canPlace = BuildingPlaceType.Unknow;
    Vector3 m_mousePos;
    Vector3Int m_cursorPos;
    Vector3Int m_startPos;
    List<BuildingBase> m_buildings = new List<BuildingBase>();

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<SetDecalsEnabledEvent>.Subscriber(OnEnableDecals));
        m_subscriberList.Add(new Event<GetPlaceWallCursorEvent>.Subscriber(GetPlaceWall));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void OnEnable()
    {
        UpdateBuildings();

        if (m_decal != null)
            m_decal.gameObject.SetActive(true);
    }

    public void SetBuildingType(BuildingType type)
    {
        m_type = type;

        if(m_buildings.Count > 0)
        {
            foreach (var b in m_buildings)
                Destroy(b.gameObject);
            m_buildings.Clear();
        }
    }

    public bool IsCursorEnabled()
    {
        return m_enabled;
    }

    public void SetCursorEnabled(bool enabled)
    {
        m_onEditor = EditorGridBehaviour.instance != null;

        m_enabled = enabled;
        UpdateBuildings();

        if (m_decal != null)
            m_decal.gameObject.SetActive(enabled);
    }

    void Update()
    {
        if (!m_enabled)
            return;

        if (GameSystem.instance != null && !GameSystem.instance.IsBuildingAllowedToPlace(m_type))
            SetCursorEnabled(false);

        UpdatePos();
        UpdateBuildings();

        if (m_decal != null)
            m_decal.UpdateVisual();
    }

    void UpdatePos()
    {
        var cam = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (cam.camera == null)
            return;

        var ray = cam.camera.ScreenPointToRay(Input.mousePosition);

        bool haveHit = PlaceBuildingCursor.LoopCursorRatcast(ray, m_groundLayer, out m_mousePos);

        m_cursorPos = new Vector3Int(Mathf.RoundToInt(m_mousePos.x), Mathf.RoundToInt(m_mousePos.y), Mathf.RoundToInt(m_mousePos.z));

        if (m_decal != null)
            m_decal.SetTarget(m_cursorPos, GetPlacementRadius());

        if (Input.GetMouseButtonDown(1))
            SetCursorEnabled(false);

        if (m_cursorPressed && !Input.GetMouseButton(0))
        {
            OnClick();
            m_cursorPressed = false;
        }

        if (!m_cursorPressed && Input.GetMouseButtonDown(0))
        {
            m_cursorPressed = true;
            m_startPos = m_cursorPos;
        }
    }

    void UpdateBuildings()
    {
        if(!m_enabled)
        {
            foreach(var b in m_buildings)
                Destroy(b.gameObject);
            m_buildings.Clear();
            return;
        }

        var buildingData = Global.instance.buildingDatas.GetBuilding(m_type);
        if (buildingData == null || buildingData.prefab == null)
                return;
        
        var pos = GetBuildingsPos();
        while(pos.Count > m_buildings.Count)
        {
            var obj = Instantiate(buildingData.prefab);
            obj.transform.parent = transform;

            var comp = obj.GetComponent<BuildingBase>();
            if (comp != null)
                comp.SetAsCursor(true);

            m_buildings.Add(comp);
        }

        while(pos.Count < m_buildings.Count)
        {
            DisplayIconsV2.instance.Unregister(m_buildings[m_buildings.Count - 1].gameObject);
            Destroy(m_buildings[m_buildings.Count - 1].gameObject);
            m_buildings.RemoveAt(m_buildings.Count - 1);
        }

        Grid grid = GridEx.GetCurrentGrid();

        m_canPlace = BuildingPlaceType.Unknow;
        float height = (buildingData.size.y - 1) / 2;

        List<GameObject> toDisplayCross = new List<GameObject>();

        for (int i = 0; i < pos.Count; i++)
        {
            m_buildings[i].transform.position = pos[i];
            var localCanPlace = GetPlaceType(pos[i], grid, i);
            if (localCanPlace == BuildingPlaceType.Valid)
                DisplayIconsV2.instance.Unregister(m_buildings[i].gameObject);
            else toDisplayCross.Add(m_buildings[i].gameObject);
            
            if (GetPriority(m_canPlace) < GetPriority(localCanPlace))
                m_canPlace = localCanPlace;
        }

        for(int i = 0; i < toDisplayCross.Count; i++)
        {
            if(i == 0)
                DisplayIconsV2.instance.Register(toDisplayCross[i].gameObject, height, "Cross", PlaceBuildingCursor.GetMessage(m_canPlace));
            else DisplayIconsV2.instance.Register(toDisplayCross[i].gameObject, height, "Cross");
        }

        foreach(var b in m_buildings)
        {
            var wall = b as BuildingWall;
            if (wall != null)
                wall.AskUpdateMesh();
        }
    }

    void OnClick()
    {
        if (BuildingList.instance == null)
            return;

        if (GameSystem.instance != null && !GameSystem.instance.IsBuildingAllowedToPlace(m_type))
            return;

        Grid grid = GridEx.GetCurrentGrid();
        if (grid == null)
            return;

        var buildingData = Global.instance.buildingDatas.GetBuilding(m_type);
        if (buildingData == null || buildingData.prefab == null)
            return;

        var pos = GetBuildingsPos();
        for (int i = 0; i < pos.Count; i++)
        {
            if (GetPlaceType(pos[i], grid, 0) != BuildingPlaceType.Valid)
                continue;

            if (!buildingData.IsFree())
            {
                if (!buildingData.cost.HaveMoney())
                    continue;

                buildingData.cost.ConsumeCost();
            }

            var obj = Instantiate(buildingData.prefab);
            obj.transform.parent = BuildingList.instance.transform;
            obj.transform.localPosition = pos[i];
            var building = obj.GetComponent<BuildingBase>();

            Event<OnBuildingBuildEvent>.Broadcast(new OnBuildingBuildEvent(m_type, m_cursorPos));

            if (SoundSystem.instance != null)
                SoundSystem.instance.PlaySound(m_placeBuildingSound, obj.transform.position, m_placeBuildingSoundVolume);

            if (UndoList.instance != null)
            {
                var ID = Event<GetEntityIDEvent>.Broadcast(new GetEntityIDEvent(), obj).id;
                var undo = new UndoElementEntityChange();
                undo.SetPlace(EntityType.Building, ID, building.Save());
                UndoList.instance.AddStep(undo);
            }
        }
    }

    int GetPriority(BuildingPlaceType placeType)
    {
        if (placeType == BuildingPlaceType.PositionLocked)
            return 100;
        if (placeType == BuildingPlaceType.TooFar)
            return 5;
        if (placeType == BuildingPlaceType.NoResources)
            return 4;
        if (placeType == BuildingPlaceType.InvalidPlace)
            return 3;
        if (placeType == BuildingPlaceType.Valid)
            return 0;
        if(placeType == BuildingPlaceType.Unknow)
            return -1;
        return 1;
    }

    public BuildingBase GetBuildingAt(Vector3Int pos, int index = 0)
    {
        var grid = GridEx.GetCurrentGrid();
        if (grid == null)
            return null;

        foreach(var building in m_buildings)
        {
            var point = building.GetPos();
            var loopPos = GridEx.GetNearestPoint(grid, pos, point);
            if (!grid.LoopX())
                loopPos.x = pos.x;
            if (!grid.LoopZ())
                loopPos.z = pos.z;
            var bounds = building.GetBounds();
            if (bounds.Contains(loopPos))
                return building;
        }

        return null;
    }

    void OnEnableDecals(SetDecalsEnabledEvent e)
    {
        if (!m_enabled)
            return;

        if (m_decal != null)
            m_decal.gameObject.SetActive(e.enabled);
    }

    void GetPlaceWall(GetPlaceWallCursorEvent e)
    {
        if (e.cursor == null)
            e.cursor = this;
    }

    float GetPlacementRadius()
    {
        var buildingInfo = Global.instance.buildingDatas.GetBuilding(m_type);
        if (buildingInfo == null || buildingInfo.prefab == null)
            return 0;

        var building = buildingInfo.prefab.GetComponent<BuildingBase>();
        if (building == null)
            return 0;

        return building.PlacementRadius();
    }

    List<Vector3Int> GetBuildingsPos()
    {
        List<Vector3Int> list = new List<Vector3Int>();

        Grid grid = GridEx.GetCurrentGrid();
        if (grid == null)
            return list;

        if (!m_cursorPressed)
            list.Add(m_cursorPos);
        else
        {
            Vector3Int startPos = GridEx.GetNearestPoint(grid, m_startPos, m_cursorPos);
            Vector3Int distance = m_cursorPos - startPos;

            Rotation rot = RotationEx.FromVector(distance);
            Vector3Int dir = RotationEx.ToVector3Int(rot);

            int nbStep = dir.x * distance.x + dir.z * distance.z;
            for(int i = 0; i < nbStep + 1; i++)
            {
                Vector3Int pos = startPos + dir * i;
                pos.y = GridEx.GetHeight(grid, new Vector2Int(pos.x, pos.z)) + 1;
                if (!IsPosValid(pos, grid))
                    continue;
                list.Add(pos);
            }
        }

        return list;
    }

    bool IsPosValid(Vector3Int pos, Grid grid)
    {
        Vector3Int loopPos = GridEx.GetRealPosFromLoop(grid, pos);
        if (loopPos.x != pos.x && !grid.LoopX())
            return false;
        if (loopPos.z != pos.z && !grid.LoopZ())
            return false;

        var block = GridEx.GetBlock(grid, loopPos - new Vector3Int(0, 1, 0));
        if (block.type != BlockType.ground)
            return false;

        if (BuildingList.instance != null)
        {
            var building = BuildingList.instance.GetBuildingAt(pos);
            if (building != null)
                return false;
        }

        return true;
    }

    BuildingPlaceType GetPlaceType(Vector3Int pos, Grid grid, int index)
    {
        if (grid == null)
            return BuildingPlaceType.InvalidPlace;

        if (!IsPosValid(pos, grid))
            return BuildingPlaceType.InvalidPlace;

        var buildingInfo = Global.instance.buildingDatas.GetBuilding(m_type);
        if (buildingInfo == null || buildingInfo.prefab == null)
            return BuildingPlaceType.Unknow;

        int count = index + 1;
        if (buildingInfo.IsFree())
            count--;
        var cost = ResourceCost.Mult(buildingInfo.cost, count);
        if (!cost.HaveMoney())
            return BuildingPlaceType.NoResources;

        if (!PlaceBuildingCursor.CanBePlaced(grid, pos, GetPlacementRadius()))
            return BuildingPlaceType.TooFar;

        return BuildingPlaceType.Valid;
    }
}
