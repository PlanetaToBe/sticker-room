/// <summary>
/// Mesh line renderer.
/// source & reference: https://www.youtube.com/watch?v=eMJATZI0A7c
/// http://www.everyday3d.com/blog/index.php/2010/03/15/3-ways-to-draw-3d-lines-in-unity3d/
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StickerTapeData
{
    public string materialId;

    public bool onThing;

    public float positionX;
    public float positionY;
    public float positionZ;

    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;

    public float scaleX;
    public float scaleY;
    public float scaleZ;

    public float originX;
    public float originY;
    public float originZ;

    public StickerTapeLineData[] lines;
}

[System.Serializable]
public struct StickerTapeLineData
{
    public string stickerId;
    public float lineSize;

    public float startX;
    public float startY;
    public float startZ;

    public float endX;
    public float endY;
    public float endZ;

    public float normalX;
    public float normalY;
    public float normalZ;

    public StickerTapeLineData(string _stickerId, float _lineSize, Vector3 start, Vector3 end, Vector3 normal)
    {
        this.stickerId = _stickerId;
        this.lineSize = _lineSize;

        this.startX = start.x;
        this.startY = start.y;
        this.startZ = start.z;

        this.endX = end.x;
        this.endY = end.y;
        this.endZ = end.z;

        this.normalX = normal.x;
        this.normalY = normal.y;
        this.normalZ = normal.z;
    }
}

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class SerializableStickerTapeRenderer : MonoBehaviour
{

    public bool DrawOnThing
    {
        get { return m_drawOnThing; }
        set { m_drawOnThing = value; }
    }

    public Vector3 SurfaceNormal
    {
        get { return m_surfaceNormal; }
        set { m_surfaceNormal = value; }
    }

    public int TextureVerticalCount
    {
        set { m_texture_vertical_count = value; }
    }

    public int TextureHorizontalCount
    {
        set { m_texture_horizontal_count = value; }
    }

    private Mesh m_mesh;
    private Vector3 startVec;
    private float lineSize = .1f;
    private bool firstQuad = true;
    private int quadCount = 0;
    private Vector3 lastGoodOrientation;
    private Quaternion parentsQ;

    private bool m_drawOnThing = false;
    private Vector3 m_surfaceNormal;
    private Vector3 tapeNormal;

    private List<StickerTapeLineData> lineData = new List<StickerTapeLineData>();

    [HideInInspector]
    public GameObject drawPoint;
    private Vector3 origin;
    [HideInInspector]
    public GameObject selfObject;

    private int m_texture_vertical_count = 1;
    private int m_texture_horizontal_count = 1;
    private float vertical_increment
    {
        get { return 1f / m_texture_vertical_count; }
    }
    private float horizontal_increment
    {
        get { return 1f / m_texture_horizontal_count; }
    }

    private StickerData s_data;
    [HideInInspector]
    public SwapArtist swapArtist;
    [HideInInspector]
    public bool doRandomSticker;

    void Start()
    {
        m_mesh = GetComponent<MeshFilter>().mesh;

        parentsQ = Quaternion.identity;
    }

    public void SetWidth(float width)
    {
        lineSize = width;
    }

    public void AddPoint(Vector3 point, bool isDummy)
    {
        if (swapArtist == null)
        {
            s_data = StickerSceneManager.instance.GetRandomSticker();
        }
        else
        {
            if (doRandomSticker)
                s_data = swapArtist.GetStickerDataRandom();
            else
                s_data = swapArtist.GetStickerData();
        }

        if (startVec == Vector3.zero)
        {
            startVec = point;
            origin = drawPoint.transform.position;
            return;
        }

        AddLine(startVec, point, lineSize, s_data.id);
    }

    public void AddLine(Vector3 start, Vector3 end, float lineSize, string stickerId)
    {
        this.lineData.Add(new StickerTapeLineData(stickerId, lineSize, start, end, m_surfaceNormal));

        Vector3[] quad = MakeQuad(start, end, lineSize, firstQuad);

        if (firstQuad)
        {
            StickerData s_data = StickerSceneManager.instance.GetStickerById(stickerId);
            GetComponent<MeshRenderer>().material = StickerSceneManager.instance.GetSheetMaterial(s_data.sheetId);
            firstQuad = false;
        }

        m_mesh = GetComponent<MeshFilter>().mesh;

        //----- Verticese --------
        //------------------------
        int vl = m_mesh.vertices.Length;
        Vector3[] vs = m_mesh.vertices;
        vs = ResizeVertices(vs, 2 * quad.Length); // expand vertices count

        for (int i = 0; i < 2 * quad.Length; i += 2)
        {
            vs[vl + i] = quad[i / 2];
            var thickerQuad = quad[i / 2] - tapeNormal.normalized * (lineSize / 7f);
            vs[vl + i + 1] = quad[i / 2]; // thickerQuad;
        }

        //-------- UV ------------
        //------------------------
        Vector2[] uvs = m_mesh.uv;
        uvs = ResizeUVs(uvs, 2 * quad.Length); // expand UVs count

        StickerUVData uvPos = StickerSceneManager.instance.GetUvsForSticker(stickerId);

        uvs[vl + 0] = uvs[vl + 1] = new Vector2(uvPos.min.x, uvPos.max.y);        //up   (0,1)
        uvs[vl + 2] = uvs[vl + 3] = new Vector2(uvPos.max.x, uvPos.max.y);        //one  (1,1)
        uvs[vl + 4] = uvs[vl + 5] = new Vector2(uvPos.min.x, uvPos.min.y);        //zero (0,0)
        uvs[vl + 6] = uvs[vl + 7] = new Vector2(uvPos.max.x, uvPos.min.y);        //right(1,0)

        //------ Triangle --------
        //------------------------
        int tl = m_mesh.triangles.Length;
        int[] ts = m_mesh.triangles;
        ts = ResizeTriangles(ts, 12);

        // front-facing quad
        ts[tl] = vl;
        ts[tl + 1] = vl + 2;
        ts[tl + 2] = vl + 4;

        ts[tl + 3] = vl + 4;
        ts[tl + 4] = vl + 2;
        ts[tl + 5] = vl + 6;

        // back-facing quad
        ts[tl + 6] = vl + 3;
        ts[tl + 7] = vl + 1;
        ts[tl + 8] = vl + 5;

        ts[tl + 9] = vl + 5;
        ts[tl + 10] = vl + 7;
        ts[tl + 11] = vl + 3;

        //
        m_mesh.vertices = vs;
        m_mesh.uv = uvs;
        m_mesh.triangles = ts;
        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();

        startVec = end;
    }

    public Vector3[] MakeQuad(Vector3 s, Vector3 e, float w, bool firstQuad)
    {
        w /= 2;

        Vector3[] q;
        q = new Vector3[4];

        Vector3 n;
        if (m_drawOnThing)
        {
            s = s + m_surfaceNormal.normalized * (lineSize / 7f); //0.01f
            e = e + m_surfaceNormal.normalized * (lineSize / 7f); //0.01f
            n = m_surfaceNormal;
        }
        else
        {
            n = Vector3.Cross(s, e);
        }

        // be affected by parent rotation
        if (firstQuad)
        {
            // if (!m_drawOnThing)
                // parentsQ = drawPoint.transform.rotation;
        }
        n = parentsQ * n;
        tapeNormal = n;

        Vector3 l = Vector3.Cross(n, e - s);

        if (l != Vector3.zero)  // prevent adding to zero when drawing stright line
        {
            lastGoodOrientation = l;
        }
        else
        {
            l = lastGoodOrientation;
        }
        l.Normalize();

        q[0] = transform.InverseTransformPoint(s + l * w); //from world space to local space
        q[1] = transform.InverseTransformPoint(s + l * -w);
        q[2] = transform.InverseTransformPoint(e + l * w);
        q[3] = transform.InverseTransformPoint(e + l * -w);

        quadCount++;

        return q;
    }

    private Vector3[] ResizeVertices(Vector3[] ovs, int ns)
    {
        Vector3[] nvs = new Vector3[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++)
        {
            nvs[i] = ovs[i];
        }
        return nvs;
    }

    private Vector2[] ResizeUVs(Vector2[] uvs, int ns)
    {
        Vector2[] nvs = new Vector2[uvs.Length + ns];
        for (int i = 0; i < uvs.Length; i++)
        {
            nvs[i] = uvs[i];
        }
        return nvs;
    }

    private int[] ResizeTriangles(int[] ovs, int ns)
    {
        int[] nvs = new int[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++)
        {
            nvs[i] = ovs[i];
        }
        return nvs;
    }

    // Save/Load

    public StickerTapeData Dehydrate()
    {
        StickerTapeData data = new StickerTapeData();

        data.onThing = m_drawOnThing;

        data.positionX = transform.position.x;
        data.positionY = transform.position.y;
        data.positionZ = transform.position.z;

        data.rotX = transform.rotation.x;
        data.rotY = transform.rotation.y;
        data.rotZ = transform.rotation.z;

        data.scaleX = transform.localScale.x;
        data.scaleY = transform.localScale.y;
        data.scaleZ = transform.localScale.z;

        data.originX = origin.x;
        data.originY = origin.y;
        data.originZ = origin.z;

        data.lines = lineData.ToArray();

        return data;
    }

    public void Rehydrate (StickerTapeData data)
    {
        if (data.lines.Length == 0) return;

        m_drawOnThing = data.onThing;

        transform.position = new Vector3(data.originX, data.originY, data.originZ);

        foreach (StickerTapeLineData lineData in data.lines)
        {
            Vector3 start = new Vector3(lineData.startX, lineData.startY, lineData.startZ);
            Vector3 end = new Vector3(lineData.endX, lineData.endY, lineData.endZ);
            Vector3 normal = new Vector3(lineData.normalX, lineData.normalY, lineData.normalZ);

            if (startVec == Vector3.zero)
            {
                startVec = start;
            }

            m_surfaceNormal = normal;
            this.AddLine(start, end, lineData.lineSize, lineData.stickerId);
        }

        StickerData firstSticker = StickerSceneManager.instance.GetStickerById(data.lines[0].stickerId);
        GetComponent<Renderer>().material = StickerSceneManager.instance.GetSheetMaterial(firstSticker.sheetId);

        transform.position = new Vector3(data.positionX, data.positionY, data.positionZ);
        transform.localScale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);
        transform.rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);

        // GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }
}
