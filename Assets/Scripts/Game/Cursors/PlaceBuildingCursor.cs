using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlaceBuildingCursor : MonoBehaviour
{
    [SerializeField] LayerMask m_groundLayer;
    [SerializeField] string m_placeBuildingSound;
    [SerializeField] float m_placeBuildingSoundVolume = 1;

    bool m_enabled = false;
    BuildingType m_type;
    BuildingBase m_instance;
    BuildingPlaceType m_canPlace = BuildingPlaceType.Valid;
    bool m_posValid = false;
    Vector3 m_mousePos;
    Vector3Int m_cursorPos;

    private void OnEnable()
    {
        UpdateBuilding();
    }

    public void SetBuildingType(BuildingType type)
    {
        m_type = type;
        m_enabled = true;
        UpdateBuilding();
    }

    public void SetCursorDisabled()
    {
        m_enabled = false;
        UpdateBuilding();
    }

    public bool IsCursorEnabled()
    {
        return m_enabled;
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

        if (Input.GetMouseButtonDown(0))
            OnClick();
        else if (Input.GetMouseButtonDown(1))
            SetCursorDisabled();
    }

    void UpdatePos()
    {
        if (m_instance == null)
            return;

        var cam = new GetCameraEvent();
        Event<GetCameraEvent>.Broadcast(cam);
        if (cam.camera == null)
            return;

        m_posValid = false;
        IsMouseOverUIEvent overUI = new IsMouseOverUIEvent();
        Event<IsMouseOverUIEvent>.Broadcast(overUI);
        if (overUI.overUI)
            return;

        var ray = cam.camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool haveHit = Physics.Raycast(ray, out hit, float.MaxValue, m_groundLayer.value);
        if(!haveHit)
            return;

        m_mousePos = hit.point;
        m_mousePos += hit.normal * 0.5f;

        m_cursorPos = new Vector3Int(Mathf.RoundToInt(m_mousePos.x), Mathf.RoundToInt(m_mousePos.y), Mathf.RoundToInt(m_mousePos.z));
        m_instance.transform.position = m_cursorPos;

        m_posValid = true;
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
            m_canPlace = m_instance.CanBePlaced(validPos);

        if (!buildingData.IsFree() && !buildingData.cost.HaveMoney())
        {
            m_canPlace = BuildingPlaceType.NoResources;
            return;
        }

        //test at range of an other pylon
        Vector3 pos = m_instance.GetGroundCenter();
        float radius = m_instance.PlacementRadius();

        List<BuildingBase> connectable = new List<BuildingBase>();
        connectable.AddRange(BuildingList.instance.GetAllBuilding(BuildingType.Tower));
        connectable.AddRange(BuildingList.instance.GetAllBuilding(BuildingType.Pylon));
        connectable.AddRange(BuildingList.instance.GetAllBuilding(BuildingType.BigPylon));

        bool canPlace = false;
        foreach (var b in connectable)
        {
            if (!ConnexionSystem.instance.IsConnected(b))
                continue;

            var targetPos = b.GetGroundCenter();
            var targetRadius = radius + b.PlacementRadius() - 0.01f;
            if (VectorEx.SqrMagnitudeXZ(targetPos - pos) < targetRadius * targetRadius)
            {
                canPlace = true;
                break;
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

        Event<OnBuildingBuildEvent>.Broadcast(new OnBuildingBuildEvent(m_type));

        if (SoundSystem.instance != null)
            SoundSystem.instance.PlaySound(m_placeBuildingSound, obj.transform.position, m_placeBuildingSoundVolume);
    }

    void EnableCross(bool enabled, string message = "")
    {
        if (DisplayIcons.instance == null || m_instance == null)
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

            DisplayIcons.instance.Register(m_instance.gameObject, height, "Cross", message);
        }
        else DisplayIcons.instance.Unregister(m_instance.gameObject);
    }

    string GetMessage(BuildingPlaceType type)
    {
        switch(type)
        {
            case BuildingPlaceType.NoResources:
                return "No resources";
            case BuildingPlaceType.InvalidPlace:
                return "Invalid Place";
            case BuildingPlaceType.TooFar:
                return "Too far";
            case BuildingPlaceType.NeedCrystal:
                return "Need Crystal";
            case BuildingPlaceType.NeedOil:
                return "Need Oil";
            case BuildingPlaceType.NeedTitanim:
                return "Need Titanium";
            case BuildingPlaceType.NeedWater:
                return "Need Water";
            case BuildingPlaceType.TooCloseSolarPannel:
                return "Too close to other Solar Pannel";
            case BuildingPlaceType.Unknow:
            case BuildingPlaceType.Valid:
            default:
                break;
        }

        return "";
    }
}
