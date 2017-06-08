using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickerArt : MonoBehaviour {

    public StickerData data;
	public int gifDataIndex;
	public bool beStatic = true;
	public string stickerID = "";

	// Use this for initialization
	void Start () {
		if (beStatic)
		{
			if (stickerID != "") {
				data = StickerSceneManager.instance.GetStickerById (stickerID);
			} else {
				data = StickerSceneManager.instance.data [gifDataIndex];
			}
		}

		stickerID = data.id;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = StickerSceneManager.instance.GetSheetMaterial(data.sheetId);

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector2[] startUvs = mesh.uv;
        Vector2[] newUvs = new Vector2[startUvs.Length];

        float aspect = data.width / data.height;
		aspect *= transform.localScale.y;
        transform.localScale = new Vector3(aspect, transform.localScale.y, transform.localScale.z);

        float size = 4096;
//        float xMin = data.x / size;
//        float xMax = (data.x + data.width) / size;
		float xMin = (data.x + data.width) / size;
		float xMax = data.x / size;
        float yMin = 1 - (data.y / size);
        float yMax = 1 - (data.y + data.height) / size;

        for (int i = 0; i < startUvs.Length; i++)
        {
            newUvs[i] = new Vector2(
                xMin + startUvs[i].x * (xMax - xMin),
                yMin + startUvs[i].y * (yMax - yMin)
           );
        }

        mesh.uv = newUvs;
    }
}
