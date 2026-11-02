using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingWall : BuildingBase
{
    enum WallType
    {
        Crystal,
        Titanium,
        Glitch,
    }

    enum WallShape
    {
        Single,
        End,
        Line,
        Corner,
        TShape,
        Cross,
    }

    [Serializable]
    class ShapeInfo
    {
        public WallShape shape;
        public Rotation meshRotation;
        public Mesh mesh;
    }

    [SerializeField] WallType m_wallType;
    [SerializeField] float m_pathCost = 0;
    [SerializeField] List<ShapeInfo> m_wallShapes = new List<ShapeInfo>();

    MeshFilter m_meshFilter;
    WallShape m_currentShape;
    Rotation m_currentRotation;
    Rotation m_meshRotation;
    bool m_isSet = false;

    SubscriberList m_subscriberList = new SubscriberList();

    public override void Awake()
    {
        base.Awake();

        Transform meshTr = transform.Find("Mesh");
        if (meshTr != null)
            m_meshFilter = meshTr.GetComponent<MeshFilter>();

        m_subscriberList.Add(new Event<BuildingListAddEvent>.Subscriber(OnBuildingAdd));
        m_subscriberList.Add(new Event<BuildingListRemoveEvent>.Subscriber(OnBuildingRemove));
        m_subscriberList.Subscribe();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        m_subscriberList.Unsubscribe();
    }

    public override void Start()
    {
        base.Start();
        UpdateMesh();
    }

    public override BuildingType GetBuildingType()
    {
        if (m_wallType == WallType.Crystal)
            return BuildingType.WallCrystal;
        if (m_wallType == WallType.Titanium)
            return BuildingType.WallTitanium;
        return BuildingType.WallGlitch;
    }

    public override bool IsMoveValid(out float cost)
    {
        cost = m_pathCost;
        return true;
    }

    void UpdateMesh()
    {
        var pos = GetPos();

        bool up = IsSameWall(pos + new Vector3Int(0, 0, 1));
        bool down = IsSameWall(pos + new Vector3Int(0, 0, -1));
        bool left = IsSameWall(pos + new Vector3Int(-1, 0, 0));
        bool right = IsSameWall(pos + new Vector3Int(1, 0, 0));

        WallShape newShape;
        Rotation newRotation;
        GetWantedShape(up, down, left, right, out newShape, out newRotation);
        UpdateCurrentMesh(newShape, newRotation);
    }

    bool IsSameWall(Vector3Int pos)
    {
        if (BuildingList.instance == null)
            return false;

        if (!IsAdded())
        {
            var cursor = Event<GetPlaceWallCursorEvent>.Broadcast(new GetPlaceWallCursorEvent()).cursor;

            var tempBuilding = cursor.GetBuildingAt(pos);
            if(tempBuilding != null)
            {
                if (tempBuilding.GetBuildingType() == GetBuildingType() && tempBuilding.GetTeam() == GetTeam())
                    return true;
            }
        }

        var building = BuildingList.instance.GetBuildingAt(pos);
        if (building == null)
            return false;

        if (building.GetTeam() != GetTeam())
            return false;

        return building.GetBuildingType() == GetBuildingType();
    }

    void GetWantedShape(bool up, bool down, bool left, bool right, out WallShape shape, out Rotation rotation)
    {
        rotation = Rotation.rot_0;

        int count = 0;
        if (up) count++;
        if (down) count++;
        if (left) count++;
        if (right) count++;

        if (count == 0)
            shape = WallShape.Single;
        else if (count == 1)
        {
            shape = WallShape.End;
            if (left)
                rotation = Rotation.rot_90;
            else if (down)
                rotation = Rotation.rot_180;
            else if (right)
                rotation = Rotation.rot_270;
        }
        else if (count == 2)
        {
            if(left && right)
                shape = WallShape.Line;
            else if(up && down)
            {
                shape = WallShape.Line;
                rotation = Rotation.rot_90;
            }
            else
            {
                shape = WallShape.Corner;

                if (left && up)
                    rotation = Rotation.rot_90;
                else if (down && left)
                    rotation = Rotation.rot_180;
                else if (right && down)
                    rotation = Rotation.rot_270;
            }

        }
        else if (count == 3)
        {
            shape = WallShape.TShape;
            if (!left)
                rotation = Rotation.rot_90;
            else if (!down)
                rotation = Rotation.rot_180;
            else if (!right)
                rotation = Rotation.rot_270;
        }
        else if (count == 4)
            shape = WallShape.Cross;
        else shape = WallShape.Single;
    }

    void UpdateCurrentMesh(WallShape newShape, Rotation newRotation)
    {
        if (newShape != m_currentShape || m_meshFilter.mesh == null || !m_isSet)
        {
            m_currentShape = newShape;


            if (m_meshFilter != null)
            {
                var wallInfo = m_wallShapes.Find((x) => { return x.shape == newShape; });
                if (wallInfo != null && wallInfo.mesh != null)
                {
                    m_meshFilter.mesh = wallInfo.mesh;
                    m_meshRotation = wallInfo.meshRotation;
                    m_isSet = true;
                }
            }

            
        }

        m_currentRotation = newRotation;
        UpdateRotation();
    }

    public override void UpdateRotation()
    {
        SetRotation(RotationEx.Add(m_currentRotation, m_meshRotation));
    }

    void OnBuildingAdd(BuildingListAddEvent e)
    {
        OnBuildingChange(e.building);
    }

    void OnBuildingRemove(BuildingListRemoveEvent e)
    {
        OnBuildingChange(e.building);
    }

    void OnBuildingChange(BuildingBase b)
    {
        if (b == null)
            return;

        if (b.GetTeam() != GetTeam())
            return;

        if (b.GetBuildingType() != GetBuildingType())
            return;

        var currentPos = GetPos();
        var otherPos = b.GetPos();
        if (currentPos.y != otherPos.y)
            return;

        if (Mathf.Abs(currentPos.x - otherPos.x) + Mathf.Abs(currentPos.z - otherPos.z) != 1)
            return;

        UpdateMesh();
    }

    public void AskUpdateMesh()
    {
        UpdateMesh();
    }
}
