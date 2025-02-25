using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;

public class ConnexionSystem : MonoBehaviour
{
    static readonly ProfilerMarker ms_profilerMarkerCreateConnections = new ProfilerMarker(ProfilerCategory.Scripts, "ConnexionSystem.UpdateState");
    static readonly ProfilerMarker ms_profilerMarkerConnectedBuildings = new ProfilerMarker(ProfilerCategory.Scripts, "ConnexionSystem.GetConnectedBuilding");
    static readonly ProfilerMarker ms_profilerMarkerIsConnected = new ProfilerMarker(ProfilerCategory.Scripts, "ConnexionSystem.IsConnected");

    [SerializeField] Color m_connexionColor = Color.black;
    [SerializeField] float m_connexionWidth = 0.1f;
    [SerializeField] Material m_connexionMaterial;

    class OneConnexion
    {
        public BuildingBase building1;
        public BuildingBase building2;
        public GameObject connexion;
    }

    class BuildingInfo
    {
        public BuildingBase building;
        public List<BuildingBase> connectedBuildings = new List<BuildingBase>();
    }

    static ConnexionSystem m_instance = null;
    public static ConnexionSystem instance { get { return m_instance; } }

    List<BuildingInfo> m_connectedBuildings = new List<BuildingInfo>();
    List<OneConnexion> m_connexions = new List<OneConnexion>();
    bool m_needUpdate = false;

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    private void Update()
    {
        if(m_needUpdate)
        {
            m_needUpdate = false;
            UpdateState();
        }
    }

    public void OnBuildingChange()
    {
        m_needUpdate = true;
    }

    void UpdateState()
    {
        using (ms_profilerMarkerCreateConnections.Auto())
        {
            m_connectedBuildings.Clear();
            List<OneConnexion> connexions = new List<OneConnexion>();

            if (BuildingList.instance == null)
                return;

            List<BuildingInfo> openList = new List<BuildingInfo>();

            var tower = BuildingList.instance.GetFirstBuilding(BuildingType.Tower);
            if (tower == null)
                return;

            BuildingInfo info = new BuildingInfo();
            info.building = tower;
            m_connectedBuildings.Add(info);
            openList.Add(info);

            int nbBuilding = BuildingList.instance.GetBuildingNb();

            while (openList.Count > 0)
            {
                var current = openList[0];
                openList.RemoveAt(0);

                Vector3 currentPos = current.building.GetGroundCenter();
                float currentRadius = current.building.PlacementRadius();

                for (int i = 0; i < nbBuilding; i++)
                {
                    var building = BuildingList.instance.GetBuildingFromIndex(i);
                    if (building == current.building)
                        continue;

                    if (building.GetTeam() != Team.Player)
                        continue;

                    if (Utility.IsDead(building.gameObject))
                        continue;

                    Vector3 pos = building.GetGroundCenter();

                    float dist = VectorEx.SqrMagnitudeXZ(pos - currentPos);

                    float radius = Global.instance.buildingDatas.GetRealPlaceRadius(currentRadius, building.PlacementRadius());

                    if (dist <= radius * radius)
                    {
                        if (!ConnexionExist(connexions, current.building, building))
                        {
                            var connexion = new OneConnexion();
                            connexion.building1 = current.building;
                            connexion.building2 = building;
                            current.connectedBuildings.Add(building);

                            connexions.Add(connexion);
                        }

                        if (m_connectedBuildings.Exists(x => { return x.building == building; }))
                            continue;

                        BuildingInfo newBuilding = new BuildingInfo();
                        newBuilding.building = building;
                        newBuilding.connectedBuildings.Add(current.building);
                        m_connectedBuildings.Add(newBuilding);

                        var type = building.GetBuildingType();

                        if (BuildingTypeEx.IsNode(type))
                            openList.Add(newBuilding);
                    }
                }
            }

            foreach (var c in connexions)
            {
                bool found = false;
                foreach (var old in m_connexions)
                {
                    if ((old.building1 == c.building1 && old.building2 == c.building2)
                        || (old.building1 == c.building2 && old.building2 == c.building1))
                    {
                        found = true;
                        c.connexion = old.connexion;
                        break;
                    }
                }
                if (found)
                    continue;

                c.connexion = CreateLine(c.building1, c.building2);
            }

            foreach (var old in m_connexions)
            {
                bool found = false;
                foreach (var c in connexions)
                {
                    if ((old.building1 == c.building1 && old.building2 == c.building2)
                        || (old.building1 == c.building2 && old.building2 == c.building1))
                    {
                        found = true;
                        c.connexion = old.connexion;
                        break;
                    }
                }
                if (found)
                    continue;

                Destroy(old.connexion);
            }

            m_connexions = connexions;

            Event<ConnexionsUpdatedEvent>.Broadcast(new ConnexionsUpdatedEvent());
        }
    }

    bool ConnexionExist(List<OneConnexion> connexions, BuildingBase building1, BuildingBase building2)
    {
        foreach (var c in connexions)
        {
            if ((building1 == c.building1 && building2 == c.building2)
                   || (building1 == c.building2 && building2 == c.building1))
            {
                return true;
            }
        }

        return false;
    }

    GameObject CreateLine(BuildingBase building1, BuildingBase building2)
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

        return obj;
    }

    public int GetConnectedBuildingNb()
    {
        return m_connectedBuildings.Count;
    }

    public BuildingBase GetConnectedBuildingFromIndex(int index)
    {
        if (index < 0 || index >= m_connectedBuildings.Count)
            return null;

        return m_connectedBuildings[index].building;
    }

    public bool IsConnected(BuildingBase building)
    {
        using (ms_profilerMarkerIsConnected.Auto())
        {
            return m_connectedBuildings.Exists(x => { return x.building == building; });
        }
    }

    public List<BuildingBase> GetConnectedBuilding(BuildingBase building)
    {
        using (ms_profilerMarkerConnectedBuildings.Auto())
        {
            var info = m_connectedBuildings.Find(x => { return x.building == building; });
            if (info != null)
                return info.connectedBuildings;

            return new List<BuildingBase>();
        }
            
    }
}
