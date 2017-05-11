using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticker : MonoBehaviour {

    public StickerData data;

	// Use this for initialization
	void Start () {
		data = StickerSceneManager.instance.GetRandomSticker();

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = StickerSceneManager.instance.GetSheetMaterial(data.sheetId);

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector2[] startUvs = mesh.uv;
        Vector2[] newUvs = new Vector2[startUvs.Length];

        float aspect = data.width / data.height;
        transform.localScale = new Vector3(aspect, transform.localScale.y, transform.localScale.z);

        float size = 2048;
        float xMin = data.x / size;
        float xMax = (data.x + data.width) / size;
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
