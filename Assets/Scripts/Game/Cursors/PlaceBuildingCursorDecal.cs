using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlaceBuildingCursorDecal : MonoBehaviour
{
    const string radiusName = "_Size";
    const string borderName = "_Border";

    const string ProjectorTextureName = "_ShadowTex";

    [SerializeField] int m_size = 10;
    [SerializeField] int m_pixelPerUnit = 16;
    [SerializeField] Material m_circleMaterial;
    [SerializeField] float m_borderThickness;
    [SerializeField] GameObject m_duplicationDecalPrefab;

    List<ProjectorData> m_projectors = new List<ProjectorData>();
    RenderTexture m_renderTexture;

    class ProjectorData
    {
        public GameObject obj;
        public Projector projector;
        public Vector2Int offset;
    }

    BuildingType m_buildingType;
    float m_placementRadius;

    private void Awake()
    {
        int pixelSize = m_size * m_pixelPerUnit * 2;
        m_renderTexture = new RenderTexture(pixelSize, pixelSize, 0);
        m_renderTexture.filterMode = FilterMode.Point;
        m_renderTexture.wrapMode = TextureWrapMode.Clamp;
        m_renderTexture.useMipMap = false;
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetTarget(Vector3Int pos, BuildingType type, float placementRadius)
    {
        pos.y += 100;
        transform.position = pos;

        m_buildingType = type;
        m_placementRadius = placementRadius;

        var grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);
        if (grid.grid == null || m_duplicationDecalPrefab == null)
            return;

        int x = grid.grid.LoopX() ? 1 : 0;
        int y = grid.grid.LoopZ() ? 1 : 0;
        int size = GridEx.GetRealSize(grid.grid);

        int index = 0;

        for(int i = -x; i <= x; i++)
        {
            for(int j = -y; j <= y; j++)
            {
                while(m_projectors.Count <= index)
                {
                    var instance = new ProjectorData();
                    instance.obj = Instantiate(m_duplicationDecalPrefab);
                    instance.projector = instance.obj.GetComponent<Projector>();
                    if (instance.projector != null)
                    {
                        instance.projector.orthographicSize = m_size;
                        instance.projector.farClipPlane = 200;
                    }
                    instance.obj.transform.parent = transform;
                    m_projectors.Add(instance);
                }

                var p = m_projectors[index];

                p.offset = new Vector2Int(i, j);
                p.obj.transform.position = transform.position + new Vector3(i * size, 0, j * size);

                index++;
            }
        }

        while(index < m_projectors.Count)
        {
            Destroy(m_projectors[index].obj);
            m_projectors.RemoveAt(index);
        }
    }

    public void UpdateVisual()
    {
        RenderTexture.active = m_renderTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;

        var grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);

        if(ConnexionSystem.instance != null && m_circleMaterial != null && grid.grid != null)
        {
            m_circleMaterial.SetFloat(borderName, m_borderThickness);

            RenderTextureEx.BeginOrthoRendering(m_renderTexture);

            int pixelSize = m_size * m_pixelPerUnit * 2;

            int size = GridEx.GetRealSize(grid.grid);
            int x = grid.grid.LoopX() ? 1 : 0;
            int y = grid.grid.LoopZ() ? 1 : 0;

            for (int index = 0; index < ConnexionSystem.instance.GetConnectedBuildingNb(); index++)
            {
                var b = ConnexionSystem.instance.GetConnectedBuildingFromIndex(index);

                if (!BuildingTypeEx.IsNode(b.GetBuildingType()))
                    continue;

                var pos = b.GetGroundCenter();
                var radius = Global.instance.buildingDatas.GetRealPlaceRadius(b.PlacementRadius(), m_placementRadius);

                for(int i = -x; i <= x; i++)
                {
                    for(int j = -y; j <= y; j++)
                    {
                        var tempPos = pos + new Vector3(i * size, 0, j * size);

                        var dirToCenter = transform.position - tempPos;
                        if (Mathf.Abs(dirToCenter.x) < m_size)
                            dirToCenter.x = 0;
                        else dirToCenter.x -= Mathf.Sign(dirToCenter.x) * m_size;
                        if (Mathf.Abs(dirToCenter.z) < m_size)
                            dirToCenter.z = 0;
                        else dirToCenter.z -= Mathf.Sign(dirToCenter.z) * m_size;

                        float sqrDistToCenter = dirToCenter.SqrMagnitudeXZ();
                        if (sqrDistToCenter > radius * radius)
                            continue;

                        dirToCenter = transform.position - tempPos;
                        Vector2 posCircle = new Vector2(-dirToCenter.x - radius + m_size, -dirToCenter.z - radius + m_size);
                        posCircle *= m_pixelPerUnit;

                        Vector2 sizeCircle = new Vector2(radius * 2 * m_pixelPerUnit, radius * 2 * m_pixelPerUnit);

                        m_circleMaterial.SetFloat(radiusName, radius * m_pixelPerUnit);
                        RenderTextureEx.DrawQuad(m_renderTexture, m_circleMaterial, new Rect(posCircle / pixelSize, sizeCircle / pixelSize));
                    }
                }
            }

            RenderTextureEx.EndRendering(m_renderTexture);
        }
        foreach(var p in m_projectors)
        {
            var mat = p.projector.material;
            if (mat == null)
                continue;
            mat.SetTexture(ProjectorTextureName, m_renderTexture);
        }
    }
}
