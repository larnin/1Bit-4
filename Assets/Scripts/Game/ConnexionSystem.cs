using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;

public class ConnexionSystem : MonoBehaviour
{
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
        public bool connectedToTower = false;
    }

    static ConnexionSystem m_instance = null;
    public static ConnexionSystem instance { get { return m_instance; } }

    List<BuildingInfo> m_connectedBuildings = new List<BuildingInfo>();
    List<BuildingInfo> m_allBuildings = new List<BuildingInfo>();
    List<OneConnexion> m_connexions = new List<OneConnexion>();
    Dictionary<BuildingBase, BuildingInfo> m_connexionFinder = new Dictionary<BuildingBase, BuildingInfo>();

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    public void OnBuildingAdd(BuildingBase b)
    {
        BuildingInfo newBuilding = new BuildingInfo();
        newBuilding.building = b;

        if (b.GetBuildingType() == BuildingType.Tower)
            newBuilding.connectedToTower = true;

        foreach(var building in m_allBuildings)
        {
            if(AreConnectable(b, building.building))
            {
                if (building.connectedToTower)
                {
                    if(!newBuilding.connectedToTower)
                    {
                        foreach(var connectedBuilding in newBuilding.connectedBuildings)
                            CreateConnection(b, connectedBuilding);
                    }
                    newBuilding.connectedToTower = true;
                }

                if (newBuilding.connectedToTower)
                    CreateConnection(b, building.building);

                newBuilding.connectedBuildings.Add(building.building);
                building.connectedBuildings.Add(b);
            }
        }

        m_allBuildings.Add(newBuilding);
        if (newBuilding.connectedToTower)
            m_connectedBuildings.Add(newBuilding);
        m_connexionFinder.Add(b, newBuilding);

        AddConnectedToTowerState(newBuilding);

        Event<ConnexionsUpdatedEvent>.Broadcast(new ConnexionsUpdatedEvent());
    }

    public void OnBuildingRemove(BuildingBase b)
    {
        BuildingInfo node;
        if (!m_connexionFinder.TryGetValue(b, out node))
            return;

        m_allBuildings.Remove(node);
        m_connectedBuildings.Remove(node);
        m_connexionFinder.Remove(b);

        List<BuildingInfo> testNodes = new List<BuildingInfo>();

        foreach(var otherBuilding in node.connectedBuildings)
        {
            var c = m_connexions.Find(x => { return IsConnexion(x, b, otherBuilding); });
            if(c != null)
            {
                Destroy(c.connexion);
                m_connexions.Remove(c);
            }

            BuildingInfo otherNode;
            if (!m_connexionFinder.TryGetValue(otherBuilding, out otherNode))
                continue;

            otherNode.connectedBuildings.Remove(b);

            testNodes.Add(otherNode);
        }

        foreach(var otherNode in testNodes)
        {
            if (!IsConnectedToTower(otherNode.building))
                RemoveConnectedToTowerState(otherNode);
        }

        Event<ConnexionsUpdatedEvent>.Broadcast(new ConnexionsUpdatedEvent());
    }

    void AddConnectedToTowerState(BuildingInfo b)
    {
        if (!b.connectedToTower)
            return;

        foreach(var c in b.connectedBuildings)
        {
            BuildingInfo info;
            if (!m_connexionFinder.TryGetValue(c, out info))
                continue;
            
            if(!info.connectedToTower)
            {
                info.connectedToTower = true;
                m_connectedBuildings.Add(info);

                if(!m_connexions.Exists(x => { return IsConnexion(x, b.building, c); }))
                    CreateConnection(b.building, c);

                AddConnectedToTowerState(info);
            }
        }
    }

    void RemoveConnectedToTowerState(BuildingInfo b)
    {
        b.connectedToTower = false;
        m_connectedBuildings.Remove(b);

        foreach(var otherBuilding in b.connectedBuildings)
        {
            var c = m_connexions.Find(x => { return IsConnexion(x, b.building, otherBuilding);});
            if (c != null)
            {
                Destroy(c.connexion);
                m_connexions.Remove(c);
            }

            BuildingInfo otherNode;
            if (!m_connexionFinder.TryGetValue(otherBuilding, out otherNode))
                continue;

            if (otherNode.connectedToTower)
                RemoveConnectedToTowerState(otherNode);
        }
    }

    bool IsConnectedToTower(BuildingBase b)
    {
        if (b.GetBuildingType() == BuildingType.Tower)
            return true;

        HashSet<BuildingBase> visitedBuilding = new HashSet<BuildingBase>();
        visitedBuilding.Add(b);

        List<BuildingBase> openNodes = new List<BuildingBase>();
        openNodes.Add(b);

        while(openNodes.Count > 0)
        {
            var building = openNodes[openNodes.Count - 1];
            openNodes.RemoveAt(openNodes.Count - 1);

            BuildingInfo node;
            if (!m_connexionFinder.TryGetValue(building, out node))
                continue;

            foreach(var c in node.connectedBuildings)
            {
                if (c.GetBuildingType() == BuildingType.Tower)
                    return true;

                if (visitedBuilding.Contains(c))
                    continue;

                visitedBuilding.Add(c);
                openNodes.Add(c);
            }
        }

        return false;
    }

    void CreateConnection(BuildingBase b1, BuildingBase b2)
    {
        OneConnexion connexion = new OneConnexion();
        connexion.building1 = b1;
        connexion.building2 = b2;
        connexion.connexion = CreateLine(b1, b2);
        m_connexions.Add(connexion);
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
        BuildingInfo node;
        if (!m_connexionFinder.TryGetValue(building, out node))
            return false;
        return node.connectedToTower;
    }

    public List<BuildingBase> GetConnectedBuilding(BuildingBase building)
    {
        if (!m_connexionFinder.ContainsKey(building))
            return new List<BuildingBase>();

        var info = m_connexionFinder[building];
        return info.connectedBuildings;
    }

    bool AreConnectable(BuildingBase b1, BuildingBase b2)
    {
        var b1Node = BuildingTypeEx.IsNode(b1.GetBuildingType());
        var b2Node = BuildingTypeEx.IsNode(b2.GetBuildingType());

        if (!b1Node && !b2Node)
            return false;

        float maxDist = Global.instance.buildingDatas.GetRealPlaceRadius(b1.PlacementRadius(), b2.PlacementRadius());
        float sqrDist = (b1.GetGroundCenter() - b2.GetGroundCenter()).SqrMagnitudeXZ();

        return sqrDist < maxDist * maxDist;
    }

    bool IsConnexion(OneConnexion c, BuildingBase b1, BuildingBase b2)
    {
        if (c.building1 == b1 && c.building2 == b2)
            return true;

        if (c.building1 == b2 && c.building2 == b1)
            return true;

        return false;
    }
}
