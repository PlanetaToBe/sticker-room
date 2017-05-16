/// <summary>
/// Mesh line renderer.
/// source & reference: https://www.youtube.com/watch?v=eMJATZI0A7c
/// http://www.everyday3d.com/blog/index.php/2010/03/15/3-ways-to-draw-3d-lines-in-unity3d/
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class StickerTapeRenderer : MonoBehaviour {

	public Material material;

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

	[HideInInspector]
	public GameObject drawPoint;
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

	void Start()
	{
		m_mesh = GetComponent<MeshFilter> ().mesh;
		GetComponent<MeshRenderer> ().material = material;
		parentsQ = Quaternion.identity;
	}

	public void SetWidth(float width)
	{
		lineSize = width;
	}

	public void AddPoint(Vector3 point)
	{
		if(startVec != Vector3.zero)
		{
			AddLine (m_mesh, MakeQuad(startVec, point, lineSize, firstQuad));
			firstQuad = false;
		}
		startVec = point;
	}

	public void AddLine(Mesh _mesh, Vector3[] quad)
	{
		int vl = _mesh.vertices.Length;
		Vector3[] vs = _mesh.vertices;
		vs = ResizeVertices (vs, 2*quad.Length); // expand vertices count

		for(int i=0; i<2*quad.Length; i+=2)
		{
			vs [vl + i] = quad [i / 2];
			var thickerQuad = quad [i / 2] + tapeNormal.normalized * 0.01f;
			vs [vl + i + 1] = thickerQuad;
		}

		Vector2[] uvs = _mesh.uv;
		uvs = ResizeUVs (uvs, 2*quad.Length); // expand UVs count

//		if(quad.Length == 4) {
//			uvs[vl + 0] = Vector2.zero;
//			uvs[vl + 1] = Vector2.zero;
//			uvs[vl + 2] = Vector2.right;
//			uvs[vl + 3] = Vector2.right;
//			uvs[vl + 4] = Vector2.up;
//			uvs[vl + 5] = Vector2.up;
//			uvs[vl + 6] = Vector2.one;
//			uvs[vl + 7] = Vector2.one;
//		} else {
//			if(vl % 8 == 0) {
//				uvs[vl + 0] = Vector2.zero;
//				uvs[vl + 1] = Vector2.zero;
//				uvs[vl + 2] = Vector2.right;
//				uvs[vl + 3] = Vector2.right;
//			} else {
//				uvs[vl + 0] = Vector2.up;
//				uvs[vl + 1] = Vector2.up;
//				uvs[vl + 2] = Vector2.one;
//				uvs[vl + 3] = Vector2.one;
//			}
//		}

//		uvs[vl + 0] = Vector2.up;
//		uvs[vl + 1] = Vector2.up;
//		uvs[vl + 2] = Vector2.one;
//		uvs[vl + 3] = Vector2.one;
//		uvs[vl + 4] = Vector2.zero;
//		uvs[vl + 5] = Vector2.zero;
//		uvs[vl + 6] = Vector2.right;
//		uvs[vl + 7] = Vector2.right;

		int uv_vertical_index = quadCount / m_texture_horizontal_count;
		int uv_horizonal_index = quadCount - (uv_vertical_index*m_texture_horizontal_count);

		uvs[vl + 0] = uvs[vl + 1] = new Vector2 (uv_horizonal_index*horizontal_increment, vertical_increment*(uv_vertical_index+1));		//up   (0,1)
		uvs[vl + 2] = uvs[vl + 3] = new Vector2 (horizontal_increment*(uv_horizonal_index+1), vertical_increment*(uv_vertical_index+1));		//one  (1,1)
		uvs[vl + 4] = uvs[vl + 5] = new Vector2 (uv_horizonal_index*horizontal_increment, uv_vertical_index*vertical_increment);			//zero (0,0)
		uvs[vl + 6] = uvs[vl + 7] = new Vector2 (horizontal_increment*(uv_horizonal_index+1), uv_vertical_index*vertical_increment);		//right(1,0)

		int tl = _mesh.triangles.Length;

		int[] ts = _mesh.triangles;
		ts = ResizeTriangles(ts, 12);
//		ts = ResizeTriangles(ts, 36);

//		if(quad.Length == 2) {
//			vl -= 4;
//		}
			
		// front-facing quad
		ts [tl] = vl;
		ts [tl + 1] = vl + 2;
		ts [tl + 2] = vl + 4;

		ts [tl + 3] = vl + 4;
		ts [tl + 4] = vl + 2;
		ts [tl + 5] = vl + 6;

		// back-facing quad
		ts [tl + 6] = vl + 3;
		ts [tl + 7] = vl + 1;
		ts [tl + 8] = vl + 5;

		ts [tl + 9] = vl + 5;
		ts [tl + 10] = vl + 7;
		ts [tl + 11] = vl + 3;

		// side quad
//		int count_tris = tl + 12;
//		for(int i=0; i<quad.Length; i++)
//		{
//			// triangles around the object
//			int n = (i+1)%quad.Length;
//
//			ts[count_tris] = i;
//			ts[count_tris+1] = n;
//			ts[count_tris+2] = i+quad.Length;
//
//			ts[count_tris+3] = n;
//			ts[count_tris+4] = n+quad.Length;
//			ts[count_tris+5] = i+quad.Length;
//
//			count_tris += 6;
//		}

		//
//		ts [tl + 12] = vl + 0;
//		ts [tl + 13] = vl + 1;
//		ts [tl + 14] = vl + 2;
//
//		ts [tl + 15] = vl + 2;
//		ts [tl + 16] = vl + 1;
//		ts [tl + 17] = vl + 3;
//		//
//		ts [tl + 18] = vl + 2;
//		ts [tl + 19] = vl + 3;
//		ts [tl + 20] = vl + 6;
//
//		ts [tl + 21] = vl + 6;
//		ts [tl + 22] = vl + 3;
//		ts [tl + 23] = vl + 7;
//		//
//		ts [tl + 24] = vl + 4;
//		ts [tl + 25] = vl + 5;
//		ts [tl + 26] = vl + 0;
//
//		ts [tl + 27] = vl + 0;
//		ts [tl + 28] = vl + 5;
//		ts [tl + 29] = vl + 1;
//		//
//		ts [tl + 30] = vl + 6;
//		ts [tl + 31] = vl + 7;
//		ts [tl + 32] = vl + 4;
//
//		ts [tl + 33] = vl + 4;
//		ts [tl + 34] = vl + 7;
//		ts [tl + 35] = vl + 5;
		//
		_mesh.vertices = vs;
		_mesh.uv = uvs;
		_mesh.triangles = ts;
		_mesh.RecalculateBounds ();
		_mesh.RecalculateNormals ();

//		_mesh.subMeshCount = quadCount;
//		subMaterials.Add (material);
//		selfObject.GetComponent<Renderer>().materials = subMaterials.ToArray();
	}

	// start, end, line width, is_it_firstQuad?
	public Vector3[] MakeQuad(Vector3 s, Vector3 e, float w, bool firstQuad)
	{
		w /= 2;

		Vector3[] q;
//		if (firstQuad)
			q = new Vector3[4];
//		else
//			q = new Vector3[2];

		//Vector3 n = Vector3.Cross (s, e);
		Vector3 n;
		if(m_drawOnThing)
		{
			s = s + m_surfaceNormal.normalized*0.01f;
			e = e + m_surfaceNormal.normalized*0.01f;
			n = m_surfaceNormal;
		}
		else
		{
			n = Vector3.Cross (s, e);
		}

		// be affected by parent rotation
		if (firstQuad)
		{
			if(!m_drawOnThing)
				parentsQ = drawPoint.transform.rotation;
		}
		n = parentsQ * n;
		tapeNormal = n;

		Vector3 l = Vector3.Cross (n, e-s);

		if (l != Vector3.zero)	// prevent adding to zero when drawing stright line
		{
			lastGoodOrientation = l;
		}
		else
		{
			l = lastGoodOrientation;
		}
		l.Normalize ();

//		if(firstQuad)
//		{
			q [0] = transform.InverseTransformPoint (s + l*w); //from world space to local space
			q [1] = transform.InverseTransformPoint (s + l*-w);
			q [2] = transform.InverseTransformPoint (e + l*w);
			q [3] = transform.InverseTransformPoint (e + l*-w);
//		}
//		else
//		{
//			q [0] = transform.InverseTransformPoint (s + l*w);
//			q [1] = transform.InverseTransformPoint (s + l*-w);
//		}

		quadCount++;

		return q;
	}

	private Vector3[] ResizeVertices(Vector3[] ovs, int ns)
	{
		Vector3[] nvs = new Vector3[ovs.Length + ns];
		for(int i=0; i<ovs.Length; i++)
		{
			nvs [i] = ovs [i];
		}
		return nvs;
	}

	private Vector2[] ResizeUVs(Vector2[] uvs, int ns)
	{
		Vector2[] nvs = new Vector2[uvs.Length + ns];
		for(int i=0; i<uvs.Length; i++)
		{
			nvs [i] = uvs [i];
		}
		return nvs;
	}

	private int[] ResizeTriangles(int[] ovs, int ns)
	{
		int[] nvs = new int[ovs.Length + ns];
		for(int i=0; i<ovs.Length; i++)
		{
			nvs [i] = ovs [i];
		}
		return nvs;
	}
}
