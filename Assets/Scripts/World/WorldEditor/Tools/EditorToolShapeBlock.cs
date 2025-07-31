using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum EditorToolShape
{
    Cuboid,
    Sphere
}

public class EditorToolShapeBlock : EditorToolBase
{
    EditorToolShape m_shape;
    GameObject m_cursor;
    MeshFilter m_cursorMesh;

    Vector3Int m_posStart;
    Vector3Int m_posEnd;
    bool m_started = false;
    bool m_placeBlock = false;

    SubscriberList m_subscriberList = new SubscriberList();

    public EditorToolShapeBlock(EditorToolShape shape)
    {
        m_shape = shape;

        m_subscriberList.Add(new Event<IsScrollLockedEvent>.Subscriber(ScrollLocked));
    }

    public override void Begin()
    {
        m_subscriberList.Subscribe();

        CreateCursor();
        SetSimpleCursor();
    }

    public override void Update()
    {
        var mousePos = Input.mousePosition;
        var cam = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());

        if (cam.camera == null)
            return;

        var ray = cam.camera.ScreenPointToRay(mousePos);

        if (m_started)
            UpdateStarted(ray);
        else UpdateNotStarted(ray);
    }

    void UpdateNotStarted(Ray cursorRay)
    {
        Vector3Int point;
        Vector3Int pointOnCollision;

        bool haveHit = EditorToolSimpleBlock.GetMouseBlockTarget(out point, out pointOnCollision);
        m_cursor.SetActive(haveHit);

        if(haveHit)
        {
            m_cursor.transform.position = pointOnCollision;

            if (Input.GetMouseButtonDown(0))
                StartPlace(point, true);
            else if (Input.GetMouseButtonDown(1))
                StartPlace(pointOnCollision, false);
        }
    }

    void StartPlace(Vector3Int pos, bool place)
    {
        m_posStart = pos;
        m_posEnd = pos;

        m_started = true;
        m_placeBlock = place;

        SetSelectionCursor();
    }

    void UpdateStarted(Ray cursorRay)
    {
        var cam = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (cam.camera == null)
            return;

        const int MaxSize = 100;

        Vector3Int oldPos = m_posEnd;

        var plane = new Plane(Vector3.up, m_posStart);

        var mousePos = Input.mousePosition;
        var ray = cam.camera.ScreenPointToRay(mousePos);

        float enter;
        if (!plane.Raycast(ray, out enter))
            return;

        Vector3 hit = ray.GetPoint(enter);
        m_posEnd = new Vector3Int(Mathf.RoundToInt(hit.x), Mathf.RoundToInt(hit.y), Mathf.RoundToInt(hit.z));
        m_posEnd.y = oldPos.y;

        m_posEnd.y += Mathf.FloorToInt(Input.mouseScrollDelta.y);

        for (int i = 0; i < 3; i++)
        {
            if (Mathf.Abs(m_posStart[i] - m_posEnd[i]) > MaxSize)
                m_posEnd[i] = m_posStart[i] + MaxSize * ((m_posEnd[i] - m_posStart[i]) >= 0 ? 1 : -1);
        }

        if (m_posEnd != oldPos)
            SetSelectionCursor();

        int button = m_placeBlock ? 0 : 1;
        if(Input.GetMouseButtonUp(button) || !Input.GetMouseButton(button))
        {
            SetBlocks();
            EndPlace();
        }
    }

    void EndPlace()
    {
        m_posStart = Vector3Int.zero;
        m_posEnd = Vector3Int.zero;

        m_started = false;
        m_placeBlock = false;

        SetSimpleCursor();
    }

    public override void End()
    {
        m_subscriberList.Unsubscribe();

        if (m_cursor != null)
            GameObject.Destroy(m_cursor);
    }

    void CreateCursor()
    {
        if (m_cursor != null)
            return;

        var obj = new GameObject("Cursor");
        obj.layer = LayerMask.NameToLayer(Global.instance.editorDatas.editorLayer);
        obj.transform.parent = m_holder.transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        var filter = obj.AddComponent<MeshFilter>();
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = Global.instance.editorDatas.cursorMaterial;

        if (filter.mesh != null)
            GameObject.Destroy(filter.mesh);

        m_cursor = obj;
        m_cursor.SetActive(false);
        m_cursorMesh = filter;
    }

    void SetSimpleCursor()
    {
        if (m_cursorMesh.mesh != null)
            GameObject.Destroy(m_cursorMesh.mesh);
        m_cursorMesh.mesh = WireframeMesh.SimpleCube(Vector3.one * 1.1f, Color.white);
    }

    void SetSelectionCursor()
    {
        if (m_cursorMesh.mesh != null)
            GameObject.Destroy(m_cursorMesh.mesh);

        if (m_shape == EditorToolShape.Cuboid)
            m_cursorMesh.mesh = WireframeMesh.Cuboid(m_posEnd - m_posStart, Color.white);
        else if (m_shape == EditorToolShape.Sphere)
            m_cursorMesh.mesh = WireframeMesh.Sphere(m_posEnd - m_posStart, Color.white);
    }

    void SetBlocks()
    {

    }
        
    void ScrollLocked(IsScrollLockedEvent e)
    {
        e.scrollLocked |= m_started;
    }
}
