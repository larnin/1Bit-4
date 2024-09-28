using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class RenderTextureEx
{
    static Mesh quad;
    static RenderTexture previousRT;

    public static void BeginOrthoRendering(RenderTexture rt, float zBegin = -100, float zEnd = 100)
    {
        Matrix4x4 projectionMatrix = Matrix4x4.Ortho(0, 1, 0, 1, zBegin, zEnd);

        BeginRendering(rt, projectionMatrix);
    }

    public static void BeginRendering(RenderTexture rt, Matrix4x4 projectionMatrix)
    {
        if (Camera.current != null)
            projectionMatrix *= Camera.current.worldToCameraMatrix.inverse;

        previousRT = RenderTexture.active;
        RenderTexture.active = rt;

        GL.PushMatrix();
        GL.LoadProjectionMatrix(projectionMatrix);
    }

    public static void EndRendering(RenderTexture rt)
    {
        GL.PopMatrix();
        GL.invertCulling = false;

        RenderTexture.active = previousRT;
        previousRT = null;
    }
    public static void DrawMesh(RenderTexture rt, Mesh mesh, Material material, in Matrix4x4 objectMatrix, int pass = 0)
    {
        bool canRender = material.SetPass(pass);

        if (canRender)
            Graphics.DrawMeshNow(mesh, objectMatrix);
    }

    public static void DrawQuad(RenderTexture rt, Material material, in Rect rect)
    {
        Matrix4x4 objectMatrix = Matrix4x4.TRS(
            rect.position, Quaternion.identity, rect.size);

        DrawMesh(rt, GetQuad(), material, objectMatrix);
    }

    static Mesh GetQuad()
    {
        if (quad)
            return quad;

        Mesh mesh = new Mesh();

        float width = 1;
        float height = 1;

        Vector3[] vertices = new Vector3[4]
        {
                new Vector3(0, 0, 0),
                new Vector3(width, 0, 0),
                new Vector3(0, height, 0),
                new Vector3(width, height, 0)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6]
        {
                // lower left triangle
                0, 2, 1,
                // upper right triangle
                2, 3, 1
        };
        mesh.triangles = tris;

        Vector2[] uv = new Vector2[4]
        {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
        };
        mesh.uv = uv;

        quad = mesh;
        return quad;
    }
}
