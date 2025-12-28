using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlaceBuildingCursor : MonoBehaviour, CursorInterface
{
    [SerializeField] LayerMask m_groundLayer;
    [SerializeField] string m_placeBuildingSound;
    [SerializeField] float m_placeBuildingSoundVolume = 1;
    [SerializeField] PlaceBuildingCursorDecal m_decal;
    [SerializeField] Color m_connexionColor = Color.black;
    [SerializeField] float m_connexionWidth = 0.1f;
    [SerializeField] Material m_connexionMaterial;

    bool m_enabled = false;
    bool m_onEditor = false;
    BuildingType m_type;
    BuildingBase m_instance;
    BuildingPlaceType m_canPlace = BuildingPlaceType.Valid;
    bool m_posValid = false;
    Vector3 m_mousePos;
    Vector3Int m_cursorPos;

    SubscriberList m_subscriberList = new SubscriberList();

    List<LineRenderer> m_connexions = new List<LineRenderer>();

    private void Awake()
    {
        m_subscriberList.Add(new Event<SetDecalsEnabledEvent>.Subscriber(OnEnableDecals));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void OnEnable()
    {
        UpdateBuilding();

        if(m_decal != null)
            m_decal.gameObject.SetActive(true);
    }

    public void SetBuildingType(BuildingType type)
    {
        m_type = type;
    }

    public bool IsCursorEnabled() 
    {
        return m_enabled;
    }

    public void SetCursorEnabled(bool enabled)
    {
        m_onEditor = EditorGridBehaviour.instance != null;

        m_enabled = enabled;
        UpdateBuilding();

        if (m_decal != null)
            m_decal.gameObject.SetActive(enabled);

        if(!enabled)
            RemoveAllConnexions();
    }

    void UpdateBuilding()
    {
        if(!m_enabled)
        {
            if (m_instance != null)
                Destroy(m_instance.gameObject);
            EnableCross(false);
            return;
        }

        if(m_instance != null)
        {
            var type = m_instance.GetBuildingType();
            if (type == m_type)
                return;

            Destroy(m_instance.gameObject);
        }

        var buildingData = Global.instance.buildingDatas.GetBuilding(m_type);
        if (buildingData == null || buildingData.prefab == null)
            return;

        var obj = Instantiate(buildingData.prefab);
        obj.transform.parent = transform;
        
        var comp = obj.GetComponent<BuildingBase>();
        if (comp != null)
            comp.SetAsCursor(true);

        m_instance = comp;
    }

    private void Update()
    {
        if (!m_enabled)
            return;

        UpdatePos();
        UpdateCanPlace();
        UpdateCross();
        UpdateConnexions();

        if (m_instance != null)
            m_instance.UpdateRotation();

        if (Input.GetMouseButtonDown(0))
            OnClick();
        else if (Input.GetMouseButtonDown(1))
            SetCursorEnabled(false);

        if (GameSystem.instance != null && !GameSystem.instance.IsBuildingAllowedToPlace(m_type))
            SetCursorEnabled(false);

        if (m_decal != null)
            m_decal.UpdateVisual();
    }

    void UpdatePos()
    {
        if (m_instance == null)
            return;

        var cam = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (cam.camera == null)
            return;

        m_posValid = false;
        var overUI = Event<IsMouseOverUIEvent>.Broadcast(new IsMouseOverUIEvent());
        if (overUI.overUI)
            return;

        var ray = cam.camera.ScreenPointToRay(Input.mousePosition);

        bool haveHit = LoopCursorRatcast(ray, m_groundLayer, out m_mousePos);
        if(!haveHit)
            return;

        m_cursorPos = new Vector3Int(Mathf.RoundToInt(m_mousePos.x), Mathf.RoundToInt(m_mousePos.y), Mathf.RoundToInt(m_mousePos.z));
        m_instance.transform.position = m_cursorPos;

        if (m_decal != null)
            m_decal.SetTarget(m_cursorPos, m_instance.GetBuildingType(), m_instance.PlacementRadius());

        m_posValid = true;
    }

    public static bool LoopCursorRatcast(Ray ray, LayerMask layer, out Vector3 pos)
    {
        pos = Vector3.zero;

        bool haveHit = false;
        Vector3 bestPos = Vector3.zero;
        float bestDistance = float.MaxValue;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());

        RaycastHit hit;

        if (grid == null)
        {
            haveHit = Physics.Raycast(ray, out hit, float.MaxValue, layer.value);
            if (!haveHit)
                return false;

            pos = hit.point;
            pos += hit.normal * 0.5f;

            return true;
        }

        var dups = Event<GetCameraDuplicationEvent>.Broadcast(new GetCameraDuplicationEvent());

        int size = GridEx.GetRealSize(grid.grid);

        foreach(var d in dups.duplications)
        {
            var tempRay = new Ray(ray.origin - new Vector3(d.x * size, 0, d.y * size), ray.direction);
            bool tempHit = Physics.Raycast(tempRay, out hit, float.MaxValue, layer.value);
            if (!tempHit)
                continue;

            haveHit = tempHit;

            if(hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                bestPos = hit.point;
                bestPos += hit.normal * 0.5f;
            }
        }

        if (haveHit)
            pos = bestPos;

        return haveHit;
    }

    void UpdateCanPlace()
    {
        m_canPlace = BuildingPlaceType.Unknow;

        var buildingData = Global.instance.buildingDatas.GetBuilding(m_type);
        if(buildingData == null)
            return;

        if (BuildingList.instance == null || ConnexionSystem.instance == null)
            return;


        var validPos = GetNearestValidPos(m_cursorPos);
        m_instance.transform.position = validPos;
        m_cursorPos = validPos;
        if (m_canPlace == BuildingPlaceType.Unknow)
            m_canPlace = m_instance.CanBePlaced(m_cursorPos);

        if (m_decal != null)
            m_decal.SetTarget(m_cursorPos, m_instance.GetBuildingType(), m_instance.PlacementRadius());

        if (m_onEditor)
            return;

        if (!buildingData.IsFree() && !buildingData.cost.HaveMoney())
        {
            m_canPlace = BuildingPlaceType.NoResources;
            return;
        }

        var validate = Event<ValidateNewBuildingPositionEvent>.Broadcast(new ValidateNewBuildingPositionEvent(m_cursorPos));
        if (validate.placeType != BuildingPlaceType.Valid)
        {
            m_canPlace = validate.placeType;
            return;
        }

        //test at range of an other pylon
        Vector3 pos = m_instance.GetGroundCenter();
        float radius = m_instance.PlacementRadius();

        List<BuildingBase> connectable = new List<BuildingBase>();
        connectable.AddRange(BuildingList.instance.GetAllBuilding(BuildingType.Tower));
        connectable.AddRange(BuildingList.instance.GetAllBuilding(BuildingType.Pylon));
        connectable.AddRange(BuildingList.instance.GetAllBuilding(BuildingType.BigPylon));

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());

        bool canPlace = false;
        if (grid.grid != null)
        {
            foreach (var b in connectable)
            {
                if (!ConnexionSystem.instance.IsConnected(b))
                    continue;

                var targetPos = b.GetGroundCenter();
                var targetRadius = Global.instance.buildingDatas.GetRealPlaceRadius(radius, b.PlacementRadius()) - 0.01f;
                if(GridEx.GetDistance(grid.grid, pos, targetPos) < targetRadius)
                {
                    canPlace = true;
                    break;
                }
            }
        }
        if (!canPlace)
        {
            m_canPlace = BuildingPlaceType.TooFar;
            return;
        }
    }

    Vector3Int GetNearestValidPos(Vector3Int pos)
    {
        if (m_instance.CanBePlaced(pos) == BuildingPlaceType.Valid)
            return pos;

        for(int i = 1; i < 4; i++)
        {
            var newPos = pos - new Vector3Int(0, i, 0);
            if (m_instance.CanBePlaced(newPos) == BuildingPlaceType.Valid)
                return newPos;
        }

        List<Vector3Int> testPos = new List<Vector3Int>();
        for(int i = -2; i <= 2; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                for(int k = -2; k <= 2; k++)
                {
                    if (i == 0 && k == 0 && j <= 0)
                        continue; //already checked

                    testPos.Add(pos + new Vector3Int(i, j, k));
                }
            }
        }

        testPos.Sort((a, b) =>
        {
            float distA = (a - m_mousePos).sqrMagnitude;
            float distB = (b - m_mousePos).sqrMagnitude;

            return distA.CompareTo(distB);
        });

        foreach(var p in testPos)
        {
            if (m_instance.CanBePlaced(p) == BuildingPlaceType.Valid)
                return p;
        }
        
        return pos;
    }

    void UpdateCross()
    {
        if (m_instance != null)
            m_instance.gameObject.SetActive(m_posValid);
            
        if(m_canPlace == BuildingPlaceType.Valid || !m_posValid)
        {
            EnableCross(false);
            return;
        }

        EnableCross(true, GetMessage(m_canPlace));
    }

    void OnClick()
    {
        if (BuildingList.instance == null)
            return;

        if (GameSystem.instance != null && !GameSystem.instance.IsBuildingAllowedToPlace(m_type))
            return;

        if (m_instance == null)
            return;
        if (!m_posValid || m_canPlace != BuildingPlaceType.Valid)
            return;

        var buildingData = Global.instance.buildingDatas.GetBuilding(m_type);
        if (buildingData == null || buildingData.prefab == null)
            return;

        if(!buildingData.IsFree())
            buildingData.cost.ConsumeCost();

        var obj = Instantiate(buildingData.prefab);
        obj.transform.parent = BuildingList.instance.transform;
        obj.transform.localPosition = m_cursorPos;
        var building = obj.GetComponent<BuildingBase>();
        if (building != null)
            building.SetRotation(m_instance.GetRotation());

        m_instance.SetRotation(RotationEx.RandomRotation());
        m_instance.UpdateRotation();

        Event<OnBuildingBuildEvent>.Broadcast(new OnBuildingBuildEvent(m_type, m_cursorPos));

        if (SoundSystem.instance != null)
            SoundSystem.instance.PlaySound(m_placeBuildingSound, obj.transform.position, m_placeBuildingSoundVolume);

        if(UndoList.instance != null)
        {
            var ID = Event<GetEntityIDEvent>.Broadcast(new GetEntityIDEvent(), obj).id;
            var undo = new UndoElementEntityChange();
            undo.SetPlace(EntityType.Building, ID, building.Save());
            UndoList.instance.AddStep(undo);
        }
    }

    void EnableCross(bool enabled, string message = "")
    {
        if (DisplayIconsV2.instance == null || m_instance == null)
            return;


        if (enabled)
        {
            float height = 2;
            var buildingData = Global.instance.buildingDatas.GetBuilding(m_type);
            if (buildingData != null)
            {
                Vector3 size = buildingData.size;
                size -= Vector3.one;
                size /= 2;
                height = size.y;
            }

            DisplayIconsV2.instance.Register(m_instance.gameObject, height, "Cross", message);
        }
        else DisplayIconsV2.instance.Unregister(m_instance.gameObject);
    }

    string GetMessage(BuildingPlaceType type)
    {
        switch(type)
        {
            case BuildingPlaceType.NoResources:
                return "No resources";
            case BuildingPlaceType.InvalidPlace:
                return "Invalid place";
            case BuildingPlaceType.TooFar:
                return "Too far";
            case BuildingPlaceType.NeedCrystal:
                return "Need crystal";
            case BuildingPlaceType.NeedOil:
                return "Need oil";
            case BuildingPlaceType.NeedTitanim:
                return "Need titanium";
            case BuildingPlaceType.NeedWater:
                return "Need water";
            case BuildingPlaceType.TooCloseSolarPannel:
                return "Too close to other Solar Pannel";
            case BuildingPlaceType.PositionLocked:
                return "Not Here";
            case BuildingPlaceType.Unknow:
            case BuildingPlaceType.Valid:
            default:
                break;
        }

        return "";
    }

    void OnEnableDecals(SetDecalsEnabledEvent e)
    {
        if (!m_enabled)
            return;

        if(m_decal != null)
            m_decal.gameObject.SetActive(e.enabled);
    }

    void UpdateConnexions()
    {
        if(ConnexionSystem.instance == null)
        {
            RemoveAllConnexions();
            return;
        }

        if(m_canPlace != BuildingPlaceType.Valid)
        {
            RemoveAllConnexions();
            return;
        }

        int nbConnexions = ConnexionSystem.instance.GetConnectedBuildingNb();
        bool isCurrentNode = BuildingTypeEx.IsNode(m_type);

        int connexionIndex = 0;

        Vector3 pos = m_instance.GetGroundCenter();
        float radius = m_instance.PlacementRadius();

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());

        if (grid.grid != null)
        {
            for (int i = 0; i < nbConnexions; i++)
            {
                var b = ConnexionSystem.instance.GetConnectedBuildingFromIndex(i);
                if (!isCurrentNode && !BuildingTypeEx.IsNode(b.GetBuildingType()))
                    continue;

                var targetPos = b.GetGroundCenter();
                var targetRadius = Global.instance.buildingDatas.GetRealPlaceRadius(radius, b.PlacementRadius()) - 0.01f;

                if (VectorEx.SqrMagnitudeXZ(targetPos - pos) < targetRadius * targetRadius)
                {
                    if (connexionIndex >= m_connexions.Count)
                        m_connexions.Add(CreateConnexion(m_instance, b));
                    else
                    {
                        var c = m_connexions[connexionIndex];
                        c.SetPosition(0, m_instance.GetRayPoint());
                        c.SetPosition(1, GridEx.GetNearestPoint(grid.grid, b.GetRayPoint(), m_instance.GetRayPoint()));
                    }

                    connexionIndex++;
                }
            }
        }

        while(connexionIndex < m_connexions.Count)
        {
            Destroy(m_connexions[connexionIndex].gameObject);
            m_connexions.RemoveAt(connexionIndex);
        }
    }

    LineRenderer CreateConnexion(BuildingBase building1, BuildingBase building2)
    {
        var obj = new GameObject("Connexion");
        obj.transform.parent = transform;

        var line = obj.AddComponent<LineRenderer>();
        line.startWidth = m_connexionWidth;
        line.endWidth = m_connexionWidth;
        line.startColor = m_connexionColor;
        line.endColor = m_connexionColor;

        Vector3 pos1 = building1.GetRayPoint();
        Vector3 pos2 = building2.GetRayPoint();

        line.positionCount = 2;
        Vector3[] points = new Vector3[2] { pos1, pos2 };
        line.SetPositions(points);
        line.material = m_connexionMaterial;
        obj.transform.position = pos1;

        return line;
    }

    void RemoveAllConnexions()
    {
        foreach(var c in m_connexions)
            Destroy(c.gameObject);

        m_connexions.Clear();
    }


}
