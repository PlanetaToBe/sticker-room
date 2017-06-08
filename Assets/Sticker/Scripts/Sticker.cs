using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StickerPlaneData
{
    public string stickerId;

    public float scaleX;
    public float scaleY;
    public float scaleZ;

    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;

    public float positionX;
    public float positionY;
    public float positionZ;
}

public class Sticker : MonoBehaviour {

	public string stickerId;
    private GameObject stickerPlane;

    private Vector2[] startUvs;

    private void Awake()
    {
        startUvs = (Vector2[])GetComponentInChildren<MeshFilter>().mesh.uv;
    } 

    void Start () {
		if (stickerId == null)
        {
            SetSticker(StickerSceneManager.instance.GetRandomSticker().id);
        } else
        {
            SetSticker(stickerId);
        }
    }

    public void SetSticker(string _stickerId)
    {
        stickerId = _stickerId;
        StickerData data = StickerSceneManager.instance.GetStickerById(stickerId);

        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        renderer.material = StickerSceneManager.instance.GetSheetMaterial(data.sheetId);

        Mesh mesh = GetComponentInChildren<MeshFilter>().mesh;
        Vector2[] newUvs = new Vector2[startUvs.Length];

        float aspect = data.width / data.height;

        stickerPlane = GetComponentInChildren<MeshRenderer>().gameObject;
        stickerPlane.transform.localScale = new Vector3(aspect, 1, 1);

        StickerUVData uvData = StickerSceneManager.instance.GetUvsForSticker(data.id);

        for (int i = 0; i < startUvs.Length; i++)
        {
            newUvs[i] = new Vector2(
                uvData.min.x + startUvs[i].x * (uvData.max.x - uvData.min.x),
                uvData.min.y + startUvs[i].y * (uvData.max.y - uvData.min.y)
           );

        }

        mesh.uv = newUvs;
    }

    public StickerPlaneData Dehydrate()
    {
        StickerPlaneData stickerData = new StickerPlaneData();

        stickerData.stickerId = stickerId;

        stickerData.positionX = transform.position.x;
        stickerData.positionY = transform.position.y;
        stickerData.positionZ = transform.position.z;

        stickerData.rotX = transform.rotation.x;
        stickerData.rotY = transform.rotation.y;
        stickerData.rotZ = transform.rotation.z;
        stickerData.rotW = transform.rotation.w;

        stickerData.scaleX = transform.localScale.x;
        stickerData.scaleY = transform.localScale.y;
        stickerData.scaleZ = transform.localScale.z;

        return stickerData;
    }

    public void Rehydrate(StickerPlaneData stickerData)
    {
        stickerId = stickerData.stickerId;
        transform.position = new Vector3(stickerData.positionX, stickerData.positionY, stickerData.positionZ);
        transform.localScale = new Vector3(stickerData.scaleX, stickerData.scaleY, stickerData.scaleZ);
        transform.rotation = new Quaternion(stickerData.rotX, stickerData.rotY, stickerData.rotZ, stickerData.rotW);
    }
}
