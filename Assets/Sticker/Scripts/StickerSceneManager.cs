using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public struct StickerSheetData
{
    public List<StickerData> stickers;
}

[System.Serializable]
public struct StickerData
{
    public float x;
    public float y;
    public float width;
    public float height;
    public string path;
    public string id;
    public string sheetId;
}

[System.Serializable]
public struct StickerMaterialData
{
    public string key;
    public Material material;
}

public class StickerSceneManager : MonoBehaviour {

    public GameObject stickerPrefab;
    public float paintSpeed;

    [HideInInspector]
    public List<StickerData> data;
    public List<StickerMaterialData> materials;

    public static StickerSceneManager _instance;
    public static StickerSceneManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new StickerSceneManager();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance)
        {
            Destroy(_instance);
        }

        _instance = this;

        LoadStickerData();
    }

    void LoadStickerData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "StickerData/meta.json");

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            StickerSheetData loadedData = JsonUtility.FromJson<StickerSheetData>(dataAsJson);
            data = loadedData.stickers;
        }
        else
        {
            Debug.LogError("Cannot load game data!");
        }
    }

    public StickerData GetRandomSticker()
    {
        return data[Random.Range(0, data.Count)];
    }

    public Material GetSheetMaterial(string sheetId)
    {
        Material stickerMaterial = materials[0].material;

        foreach (StickerMaterialData material in materials)
        {
            if (material.key == sheetId) stickerMaterial = material.material;
        }

        return stickerMaterial;
    }
}
