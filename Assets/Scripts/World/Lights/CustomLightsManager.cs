using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CustomLightsManager : MonoBehaviour
{
    const string radiusName = "_Size";

    const string lightTextureName = "_LightTex";
    const string lightTextureSizeName = "_LightTexSize";

    [SerializeField] RenderTexture m_renderTexture;
    [SerializeField] Material m_circleMaterial;

    List<CustomLight> m_lights = new List<CustomLight>();

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

        var grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);
        if (grid.grid == null)
            return;

        var gridSize = GridEx.GetRealSize(grid.grid);

        RenderTextureEx.BeginOrthoRendering(m_renderTexture);

        GL.Clear(true, true, Color.black);

        foreach (var l in m_lights)
        {
            if (l == null)
                continue;

            Vector3 pos3 = l.transform.position;
            Vector2 pos = new Vector2((pos3.x - l.GetRadius()) / gridSize, (pos3.z - l.GetRadius()) / gridSize);
            Vector2 size = new Vector2(l.GetRadius() * 2 / gridSize, l.GetRadius() * 2 / gridSize);

            m_circleMaterial.SetFloat(radiusName, l.GetRadius());
            RenderTextureEx.DrawQuad(m_renderTexture, m_circleMaterial, new Rect(pos, size));
        }

        RenderTextureEx.EndRendering(m_renderTexture);

        UpdateMaterial(Global.instance.blockDatas.defaultMaterial, grid.grid);
    }

    void UpdateMaterial(Material mat, Grid grid)
    {
        mat.SetTexture(lightTextureName, m_renderTexture);

        var gridSize = GridEx.GetRealSize(grid);
        mat.SetFloat(lightTextureSizeName, gridSize);
    }
}
