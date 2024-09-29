using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum BuildingType
{
    Tower,
    Pylon,
}

public enum EnergyUptakePriority
{
    consumption,
    storage,
}

public abstract class BuildingBase : MonoBehaviour
{
    [SerializeField] float m_life;

    bool m_added = false;
    bool m_asCursor = false;

    public void SetAsCursor(bool asCursor)
    {
        m_asCursor = asCursor;
        if (m_asCursor && m_added)
            Remove();
        else if (!m_asCursor && !m_added)
            Add();
    }

    public bool IsCursor()
    {
        return m_asCursor;
    }

    public Vector3Int GetSize()
    {
        var building = Global.instance.buildingDatas.GetBuilding(GetBuildingType());
        if (building == null)
            return Vector3Int.one;

        return building.size;
    }

    public Vector3Int GetPos()
    {
        var pos = transform.localPosition;

        return new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
    }

    public BoundsInt GetBounds()
    {
        return new BoundsInt(GetPos(), GetSize());
    }

    public abstract BuildingType GetBuildingType();

    public virtual float EnergyGeneration() { return 0; }
    public virtual float EnergyUptakeWanted() { return 0; }
    public virtual void EnergyUptake(float value) { }
    public virtual EnergyUptakePriority EnergyPriority() { return EnergyUptakePriority.consumption; }
    public virtual float EnergyStorageValue() { return 0; }
    public virtual float EnergyStorageMax() { return 0; }
    public virtual float PlacementRadius() { return 0; }

    public virtual void OnEnable()
    {
        Add();
    }

    public virtual void OnDisable()
    {
        Remove();
    }

    public virtual void Update()
    {
        if (!m_added)
            Add();
    }

    void Add()
    {
        var manager = BuildingList.instance;
        if (manager != null)
        {
            m_added = true;
            manager.Register(this);
            SetComponentsEnabled(m_added);
        }

    }

    void Remove()
    {
        var manager = BuildingList.instance;
        if (manager != null)
            manager.UnRegister(this);

        m_added = false;
        SetComponentsEnabled(m_added);
    }

    void SetComponentsEnabled(bool enabled)
    {
        var light = GetComponent<CustomLight>();
        if (light != null)
            light.enabled = enabled;
    }
}

