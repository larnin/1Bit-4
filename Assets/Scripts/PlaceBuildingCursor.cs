using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlaceBuildingCursor : MonoBehaviour
{
    [SerializeField] GameObject m_crossSprite;
    [SerializeField] LayerMask m_groundLayer;

    bool m_enabled = false;
    BuildingType m_type;
    BuildingBase m_instance;
    bool m_canPlace = false;
    bool m_posValid = false;

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
            m_crossSprite.SetActive(false);
            return;
        }

        if(m_instance != null)
        {
            var type = m_instance.GetBuildingType();
            if (type == m_type)
                return;

            Destroy(m_instance);
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

        var ray = cam.camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        m_posValid = false;
        bool haveHit = Physics.Raycast(ray, out hit, float.MaxValue, m_groundLayer.value);
        if(!haveHit)
            return;

        Vector3 target = hit.point;
        target += hit.normal * 0.5f;

        Vector3Int targetInt = new Vector3Int(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y), Mathf.RoundToInt(target.z));
        m_instance.transform.position = targetInt;

        m_posValid = true;
    }

    void UpdateCanPlace()
    {
        m_canPlace = false;

        var buildingData = Global.instance.buildingDatas.GetBuilding(m_type);
        if(buildingData == null)
            return;

        if (BuildingList.instance == null)
            return;

        //test at range of an other pylon
        Vector3 pos = m_instance.GetGroundCenter();
        float radius = m_instance.PlacementRadius();

        List<BuildingBase> connectable = new List<BuildingBase>();
        connectable.AddRange(BuildingList.instance.GetAllBuilding(BuildingType.Tower));
        connectable.AddRange(BuildingList.instance.GetAllBuilding(BuildingType.Pylon));
        connectable.AddRange(BuildingList.instance.GetAllBuilding(BuildingType.BigPylon));

        bool canPlace = false;
        foreach(var b in connectable)
        {
            var targetPos = b.GetGroundCenter();
            var targetRadius = radius + b.PlacementRadius();
            if((targetPos - pos).sqrMagnitude < targetRadius * targetRadius)
            {
                canPlace = true;
                break;
            }
        }
        if (!canPlace)
            return;

        //test if the ground can support this building
        var grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);
        var bounds = m_instance.GetBounds();
        if (grid.grid != null)
        {
            Vector3Int min = bounds.min;
            Vector3Int max = bounds.max;

            for(int i = min.x; i < max.x; i++)
            {
                for(int k = min.z; k < max.z; k++)
                {
                    var ground = GridEx.GetBlock(grid.grid, new Vector3Int(i, min.y - 1, k));
                    if (ground != BlockType.ground)
                        return;

                    for(int j = min.y; j < max.y; j++)
                    {
                        var block = GridEx.GetBlock(grid.grid, new Vector3Int(i, j, k));
                        if (block != BlockType.air)
                            return;
                    }
                }
            }
        }

        //test if an other building already here
        int nbBuilding = BuildingList.instance.GetBuildingNb();
        for(int i = 0; i < nbBuilding; i++)
        {
            var b = BuildingList.instance.GetBuildingFromIndex(i);
            var otherBounds = b.GetBounds();

            if (Utility.Intersects(otherBounds, bounds))
                return;
        }

        m_canPlace = true;
    }

    void UpdateCross()
    {
        if (m_instance != null)
            m_instance.gameObject.SetActive(m_posValid);
            
        if(m_canPlace || !m_posValid)
        {
            m_crossSprite.SetActive(false);
            return;
        }

        m_crossSprite.SetActive(true);

        var cam = new GetCameraEvent();
        Event<GetCameraEvent>.Broadcast(cam);
        if (cam.camera == null)
            return;

        m_crossSprite.transform.forward = -cam.camera.transform.forward;

        Vector3 pos = Vector3.zero;
        if (m_instance != null)
            pos = m_instance.transform.position;

        var buildingData = Global.instance.buildingDatas.GetBuilding(m_type);
        if(buildingData != null)
        {
            Vector3 size = buildingData.size;
            size -= Vector3.one;
            size /= 2;
            pos += size;
        }

        pos += 10 * m_crossSprite.transform.forward;
        m_crossSprite.transform.position = pos;
    }

    void OnClick()
    {
        if (BuildingList.instance == null)
            return;

        if (m_instance == null)
            return;
        if (!m_posValid || !m_canPlace)
            return;

        Vector3Int pos = m_instance.GetPos();

        var buildingData = Global.instance.buildingDatas.GetBuilding(m_type);
        if (buildingData == null || buildingData.prefab == null)
            return;

        var obj = Instantiate(buildingData.prefab);
        obj.transform.parent = BuildingList.instance.transform;
        obj.transform.localPosition = pos;
    }
}
