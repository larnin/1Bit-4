using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CustomLightsManager : MonoBehaviour
{
    const string radiusName = "_Size";
    const string borderName = "_Border";

    const string lightTextureName = "_LightTex";
    const string lightTextureSizeName = "_LightTexSize";

    const string lightTopName = "_LightTop";
    const string lightLeftName = "_LightLeft";
    const string lightFrontName = "_LightFront";
    const string lightRangeName = "_LightBaseRange";

    const string noiseName = "_PerlinTex";
    const string noiseScaleName = "_NoiseTextureScale";
    const string noiseTimeName = "_NoiseTime";
    const string noiseAmplitudeName = "_NoiseAmplitude";

    [SerializeField] RenderTexture m_renderTexture;
    [SerializeField] Material m_circleMaterial;
    [SerializeField] List<Material> m_lightedMaterials;
    [SerializeField] List<Material> m_unlitMaterials;
    [SerializeField] float m_borderSize = 1;
    [SerializeField] float m_increaseRadius = -1;
    [SerializeField] float m_lightTop = 1;
    [SerializeField] float m_lightLeft = 1;
    [SerializeField] float m_lightFront = 1;
    [SerializeField] float m_lightBaseRange = 0.2f;
    [SerializeField] Texture m_noiseTexture;
    [SerializeField] float m_noiseTextureScale = 1;
    [SerializeField] float m_noiseSpeed = 1;
    [SerializeField] float m_noiseAmplitude = 1;
    [SerializeField] bool m_discoverEverything = false;

    List<CustomLight> m_lights = new List<CustomLight>();
    float m_noiseTime = 0;

    static CustomLightsManager m_instance = null;
    public static CustomLightsManager instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    public void Register(CustomLight light)
    {
        m_lights.Add(light);
    }

    public void UnRegister(CustomLight light)
    {
        m_lights.Remove(light);
    }

    private void Update()
    {
        if (m_renderTexture == null)
            return;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        if (!GameInfos.instance.paused)
            m_noiseTime += Time.deltaTime * m_noiseSpeed / GridEx.GetRealSize(grid.grid);

        var gridSize = GridEx.GetRealSize(grid.grid);

        RenderTextureEx.BeginOrthoRendering(m_renderTexture);

        if(m_discoverEverything)
            GL.Clear(true, true, Color.white);
        else
        {
            GL.Clear(true, true, Color.black);

            m_circleMaterial.SetFloat(borderName, m_borderSize);

            foreach (var l in m_lights)
            {
                if (l == null)
                    continue;

                float renderRadius = l.GetRadius() + m_borderSize / 2 + m_increaseRadius;

                Vector3 pos3 = l.transform.position;
                Vector2 pos = new Vector2((pos3.x - renderRadius) / gridSize, (pos3.z - renderRadius) / gridSize);
                Vector2 size = new Vector2(renderRadius * 2 / gridSize, renderRadius * 2 / gridSize);

                m_circleMaterial.SetFloat(radiusName, renderRadius);
                RenderTextureEx.DrawQuad(m_renderTexture, m_circleMaterial, new Rect(pos, size));

                int x = grid.grid.LoopX() ? 1 : 0;
                int y = grid.grid.LoopZ() ? 1 : 0;

                for(int i = -x; i <= x; i++)
                {
                    for(int j = -y; j <= y; j++)
                    {
                        if (i == 0 && j == 0)
                            continue;

                        Vector2 newPos = pos + new Vector2(i, j);

                        if (newPos.x > 1 && newPos.y > 1 && newPos.x + size.x < 0 && newPos.y + size.y < 0)
                            continue;

                        RenderTextureEx.DrawQuad(m_renderTexture, m_circleMaterial, new Rect(newPos, size));
                    }
                }
            }
        }
        
        RenderTextureEx.EndRendering(m_renderTexture);

        foreach (var m in m_lightedMaterials)
        {
            UpdateMaterial(m, grid.grid);
            UpdateLights(m);
        }

        foreach (var m in m_unlitMaterials)
            UpdateLights(m);
    }

    void UpdateMaterial(Material mat, Grid grid)
    {
        mat.SetTexture(lightTextureName, m_renderTexture);

        var gridSize = GridEx.GetRealSize(grid);
        mat.SetFloat(lightTextureSizeName, gridSize);
    }

    void UpdateLights(Material mat)
    {
        mat.SetFloat(lightTopName, m_lightTop);
        mat.SetFloat(lightLeftName, m_lightLeft);
        mat.SetFloat(lightFrontName, m_lightFront);
        mat.SetFloat(lightRangeName, m_lightBaseRange);
        mat.SetTexture(noiseName, m_noiseTexture);
        mat.SetFloat(noiseAmplitudeName, m_noiseAmplitude);
        mat.SetFloat(noiseScaleName, m_noiseTextureScale);
        mat.SetFloat(noiseTimeName, m_noiseTime);
    }

    public bool IsPosVisible(Vector3 pos)
    {
        return IsPosVisible(new Vector2(pos.x, pos.z));
    }

    public bool IsPosVisible(Vector2 pos)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return false;

        var gridSize = GridEx.GetRealSize(grid.grid);
        int x = grid.grid.LoopX() ? 1 : 0;
        int y = grid.grid.LoopZ() ? 1 : 0;

        for(int i = -x; i <= x; i++)
        {
            for(int j = -y; j <= y; j++)
            {
                foreach (var l in m_lights)
                {
                    if (l == null)
                        continue;

                    Vector3 pos3 = l.transform.position;
                    float size = l.GetRadius();

                    float dist = (new Vector2(pos3.x + gridSize * i, pos3.z + gridSize * j) - pos).sqrMagnitude;
                    if (dist < size * size)
                        return true;
                }
            }
        }

        return false;
    }
}
