using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    static ConnexionSystem m_instance = null;
    public static ConnexionSystem instance { get { return m_instance; } }

    List<BuildingBase> m_connectedBuildings = new List<BuildingBase>();
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
        m_connectedBuildings.Clear();
        List<OneConnexion> connexions = new List<OneConnexion>();

        if (BuildingList.instance == null)
            return;

        List<BuildingBase> openList = new List<BuildingBase>();

        var tower = BuildingList.instance.GetFirstBuilding(BuildingType.Tower);
        if (tower == null)
            return;

        m_connectedBuildings.Add(tower);
        openList.Add(tower);

        int nbBuilding = BuildingList.instance.GetBuildingNb();

        while(openList.Count > 0)
        {
            var current = openList[0];
            openList.RemoveAt(0);

            Vector3 currentPos = current.GetGroundCenter();
            float currentRadius = current.PlacementRadius();

            for (int i = 0; i < nbBuilding; i++)
            {
                var building = BuildingList.instance.GetBuildingFromIndex(i);
                if (building == current)
                    continue;

                Vector3 pos = building.GetPos();

                float dist = VectorEx.SqrMagnitudeXZ(pos - currentPos);

                float radius = currentRadius + building.PlacementRadius();

                if(dist <= radius * radius)
                {
                    if (!ConnexionExist(connexions, current, building))
                    {
                        var connexion = new OneConnexion();
                        connexion.building1 = current;
                        connexion.building2 = building;

                        connexions.Add(connexion);
                    }

                    if (m_connectedBuildings.Contains(building))
                        continue;

                    m_connectedBuildings.Add(building);

                    var type = building.GetBuildingType();

                    if (type == BuildingType.Tower || type == BuildingType.Pylon || type == BuildingType.BigPylon)
                        openList.Add(building);
                }
            }
        }

        foreach(var c in connexions)
        {
            bool found = false;
            foreach(var old in m_connexions)
            {
                if((old.building1 == c.building1 && old.building2 == c.building2)
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

        m_connexions = connexions;
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

        return m_connectedBuildings[index];
    }

    public bool IsConnected(BuildingBase building)
    {
        return m_connectedBuildings.Contains(building);
    }

    public List<BuildingBase> GetConnectedBuilding(BuildingBase building)
    {
        List<BuildingBase> connected = new List<BuildingBase>();

        foreach(var c in m_connexions)
        {
            if (c.building1 == building)
                connected.Add(c.building2);
            if (c.building2 == building)
                connected.Add(c.building1);
        }

        return connected;
    }
}
