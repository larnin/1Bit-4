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

    Projector m_projector;
    RenderTexture m_renderTexture;

    BuildingType m_buildingType;
    float m_placementRadius;

    private void Awake()
    {
        m_projector = GetComponentInChildren<Projector>();

        if (m_projector != null)
            m_projector.orthographicSize = m_size;

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
        m_projector.farClipPlane = 200;

        m_buildingType = type;
        m_placementRadius = placementRadius;
    }

    public void UpdateVisual()
    {
        var mat = m_projector.material;
        if (mat == null)
            return;

        RenderTexture.active = m_renderTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;

        if(ConnexionSystem.instance != null && m_circleMaterial != null)
        {
            m_circleMaterial.SetFloat(borderName, m_borderThickness);

            RenderTextureEx.BeginOrthoRendering(m_renderTexture);

            int pixelSize = m_size * m_pixelPerUnit * 2;

            for (int i = 0; i < ConnexionSystem.instance.GetConnectedBuildingNb(); i++)
            {
                var b = ConnexionSystem.instance.GetConnectedBuildingFromIndex(i);

                if (!BuildingTypeEx.IsNode(b.GetBuildingType()))
                    continue;

                var pos = b.GetGroundCenter();
                var radius = Global.instance.buildingDatas.GetRealPlaceRadius(b.PlacementRadius(), m_placementRadius);

                var dirToCenter = transform.position - pos;
                if (Mathf.Abs(dirToCenter.x) < m_size)
                    dirToCenter.x = 0;
                else dirToCenter.x -= Mathf.Sign(dirToCenter.x) * m_size;
                if (Mathf.Abs(dirToCenter.z) < m_size)
                    dirToCenter.z = 0;
                else dirToCenter.z -= Mathf.Sign(dirToCenter.z) * m_size;

                float sqrDistToCenter = dirToCenter.SqrMagnitudeXZ();
                if (sqrDistToCenter > radius * radius)
                    continue;

                dirToCenter = transform.position - pos;
                Vector2 posCircle = new Vector2(-dirToCenter.x - radius + m_size, -dirToCenter.z - radius + m_size);
                posCircle *= m_pixelPerUnit;

                Vector2 sizeCircle = new Vector2(radius * 2 * m_pixelPerUnit, radius * 2 * m_pixelPerUnit);

                m_circleMaterial.SetFloat(radiusName, radius * m_pixelPerUnit);
                RenderTextureEx.DrawQuad(m_renderTexture, m_circleMaterial, new Rect(posCircle / pixelSize, sizeCircle / pixelSize));
            }

            RenderTextureEx.EndRendering(m_renderTexture);
        }

        mat.SetTexture(ProjectorTextureName, m_renderTexture);
    }
}
