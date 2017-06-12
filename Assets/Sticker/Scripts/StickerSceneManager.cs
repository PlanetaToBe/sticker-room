using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct SaveGameData
{
    public StickerTapeData[] stickerTapes;
    public StickerPlaneData[] stickerPlanes;
}

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

public struct StickerUVData {
    public Vector2 min;
    public Vector2 max;

    public StickerUVData(Vector2 _min, Vector2 _max) {
        this.min = _min;
        this.max = _max;
    }
}

[ExecuteInEditMode]
public class StickerSceneManager : MonoBehaviour {

    public GameObject stickerPrefab;
    public GameObject stickerTapePrefab;

    public float paintSpeed;
    public int sheetSize = 4096;

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

            if (_instance.data == null)
            {
                _instance.LoadStickerData();
            }
            return _instance;
        }
    }

    [HideInInspector]
    public List<string> allArtists;
    public Dictionary<string, List<StickerData>> stickersByArtist;
    public Dictionary<string, StickerData> stickersById;

    private void Awake()
    {
        Debug.Log("Awake");
        if (_instance)
        {
            Destroy(_instance);
        }

        _instance = this;

        LoadStickerData();
    }

    public void LoadStickerData()
    {
        Debug.Log("Load sticker data");

        string filePath = Path.Combine(Application.streamingAssetsPath, "StickerData/meta.json");

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            StickerSheetData loadedData = JsonUtility.FromJson<StickerSheetData>(dataAsJson);
            data = loadedData.stickers;

            allArtists = new List<string>();
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

    public StickerUVData GetUvsForSticker(string stickerId)
    {
        StickerData s_data = GetStickerById(stickerId);

		float xMax = s_data.x / sheetSize;
		float xMin = (s_data.x + s_data.width) / sheetSize;
		float yMax = 1 - (s_data.y / sheetSize);
		float yMin = 1 - (s_data.y + s_data.height) / sheetSize;

        Vector2 min = new Vector2(xMin, yMin);
        Vector2 max = new Vector2(xMax, yMax);

        return new StickerUVData(min, max);
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

    public void Reset()
    {
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        string saveParentDirectory = Application.persistentDataPath + "/saves";

        string saveDirectory = saveParentDirectory;

        Directory.CreateDirectory(saveParentDirectory);

        FileStream file = File.Create(saveDirectory + "/" + System.DateTime.Now.ToFileTime() + ".dat");

        SerializableStickerTapeRenderer[] stickerTapes = GetComponentsInChildren<SerializableStickerTapeRenderer>();
        StickerTapeData[] stickerTapeData = new StickerTapeData[stickerTapes.Length];
        for (int i = 0; i< stickerTapes.Length; ++i)
        {
            stickerTapeData[i] = stickerTapes[i].Dehydrate();
        }

        Sticker[] stickerPlanes = GetComponentsInChildren<Sticker>();
        StickerPlaneData[] stickerPlaneData = new StickerPlaneData[stickerPlanes.Length];
        for (int i = 0; i < stickerPlanes.Length; ++i)
        {
            stickerPlaneData[i] = stickerPlanes[i].Dehydrate();
        }

        SaveGameData saveData = new SaveGameData();
        saveData.stickerTapes = stickerTapeData;
        saveData.stickerPlanes = stickerPlaneData;

        bf.Serialize(file, saveData);
        file.Close();
    }

    public void Load(string path)
    {
        Load(path, false);
    }

    public void Load(string path, bool additive)
    {
        if (!additive)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(path, FileMode.Open);

        SaveGameData saveData = (SaveGameData)bf.Deserialize(file);

        file.Close();

        string[] filenameParts = file.Name.Split('/');
        string filename = filenameParts[filenameParts.Length - 1].Replace(".dat", "");
        GameObject parentObject = new GameObject(filename);
        parentObject.transform.parent = transform;

        foreach (StickerPlaneData planeData in saveData.stickerPlanes)
        {
			//v.1 Editor
            //GameObject stickerPlanePrefab = AssetDatabase.LoadAssetAtPath("Assets/Sticker/Prefabs/Sticker.prefab", typeof(GameObject)) as GameObject;
			//GameObject stickerPlane = PrefabUtility.InstantiatePrefab(stickerPlanePrefab) as GameObject;
			//GameObject stickerPlane = PrefabUtility.InstantiatePrefab(stickerPlanePrefab) as GameObject;
            
			//v.2 Build
			GameObject stickerPlane = Instantiate(Resources.Load("Sticker", typeof(GameObject))) as GameObject;
            stickerPlane.transform.parent = parentObject.transform;
            stickerPlane.GetComponentInChildren<Sticker>().Rehydrate(planeData);
        }

        foreach (StickerTapeData tapeData in saveData.stickerTapes)
        {
			//v.1 Editor
            //GameObject stickerTapePrefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/sticker_tape.prefab", typeof(GameObject)) as GameObject;
            //GameObject stickerTape = PrefabUtility.InstantiatePrefab(stickerTapePrefab) as GameObject;
            
			//v.2 Build
			GameObject stickerTape = Instantiate(Resources.Load("sticker_tape", typeof(GameObject))) as GameObject;
			stickerTape.transform.parent = parentObject.transform;
            stickerTape.GetComponent<SerializableStickerTapeRenderer>().Rehydrate(tapeData);
        }
    }
}
