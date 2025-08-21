using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorToolSelect : EditorToolBase
{
    GameObject m_cursor;

    GameObject m_selectedObject;

    public override void Begin()
    {
        CreateCursor();
        m_selectedObject = null;
    }

    public override void Update()
    {
        var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (camera.camera == null)
            return;

        if(Input.GetMouseButtonDown(0))
        {
            var ray = camera.camera.ScreenPointToRay(Input.mousePosition);

            GameObject newTarget = SelectCursor.LoopHoverRatcast(ray, Global.instance.editorDatas.toolHoverLayer);

            if (newTarget != null)
            {
                var type = GameSystem.GetEntityType(newTarget);
                if (type == EntityType.Building || type == EntityType.Building || type == EntityType.Quest)
                    SelectObject(newTarget);
                else SelectObject(null);
            }
            else SelectObject(null);
        }

        if(Input.GetKeyDown(KeyCode.Delete) && m_selectedObject != null)
        {
            GameObject.Destroy(m_selectedObject);
            m_selectedObject = null;
            SelectObject(null);
        }
    }

    public override void End()
    {
        if (m_cursor != null)
            GameObject.Destroy(m_cursor);

        SelectObject(null);
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

        filter.mesh = WireframeMesh.SimpleCube(Vector3.one, Color.white);

        m_cursor = obj;
        m_cursor.SetActive(false);
    }

    void SelectObject(GameObject obj)
    {
        m_selectedObject = obj;

        m_cursor.SetActive(obj != null);

        if(obj != null)
        {
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
            {
                var bound = collider.bounds;
                m_cursor.transform.position = bound.center;
                m_cursor.transform.localScale = bound.size;
            }
            else m_cursor.transform.localScale = Vector3.one;
        }
    }

}
