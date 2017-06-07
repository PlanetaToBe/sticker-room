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
public class StickerData
{
    public float x;
    public float y;
    public float width;
    public float height;
    public string path;
    public string id;
    public string sheetId;
    public string artist;
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

    public List<string> allArtists;
    public Dictionary<string, List<StickerData>> stickersByArtist;
    public Dictionary<string, StickerData> stickersById;

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

            stickersByArtist = new Dictionary<string, List<StickerData>>();
            stickersById = new Dictionary<string, StickerData>();
            foreach (StickerData sticker in data)
            {
                stickersById[sticker.id] = sticker;

                if (!stickersByArtist.ContainsKey(sticker.artist))
                {
                    stickersByArtist[sticker.artist] = new List<StickerData>();
                }

                stickersByArtist[sticker.artist].Add(sticker);

                if (!allArtists.Contains(sticker.artist))
                {
                    allArtists.Add(sticker.artist);
                }
            }

			Debug.Log("Load stickers * " + data.Count);
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

    public StickerData GetStickerById(string id)
    {
        return stickersById[id];
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
