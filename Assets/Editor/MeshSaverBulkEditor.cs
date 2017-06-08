using UnityEditor;

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.IO;

[System.Serializable]
public struct StickersTransformData
{
	public List<StickerTransform> stickersTran;
}

[System.Serializable]
public struct StickerTransform
{
	public Vector3 position;
	public Vector3 scale;
	public Quaternion rotation;
	public string name;
	public StickerData stickerData;

	public StickerTransform (Vector3 _position, Quaternion _rotation, Vector3 _scale, string _name) : this()
	{
		this.position = _position;
		this.rotation = _rotation;
		this.scale = _scale;
		this.name = _name;
	}
}

[System.Serializable]
public struct StickersTransformIDData
{
	public List<StickerTransformID> stickersTran;
}

[System.Serializable]
public struct StickerTransformID
{
	public Vector3 position;
	public Vector3 scale;
	public Quaternion rotation;
	public string name;
	public string stickerID;

	public StickerTransformID (Vector3 _position, Quaternion _rotation, Vector3 _scale, string _name) : this()
	{
		this.position = _position;
		this.rotation = _rotation;
		this.scale = _scale;
		this.name = _name;
	}
}

[System.Serializable]
public struct BallTransformData
{
	public List<BallTransform> ballsTran;
}

[System.Serializable]
public struct BallTransform
{
	public string newVector3;

	public BallTransform (string _position) : this()
	{
		this.newVector3 = _position;
	}
}

public static class MeshSaverBulkEditor
{
	static string json_path = Application.dataPath + "/Sticker/Exports/data.json";
	static string json_path_2 = Application.dataPath + "/Sticker/Exports/data2.json";
	static string json_path_3 = Application.dataPath + "/Sticker/Exports/data3.json";
	static string asset_path = "Assets/Sticker/Exports/Assets/";

	static public List<StickerData> data;
	static public Dictionary<string, StickerData> stickersById;

	[MenuItem("CONTEXT/Transform/Save Tape in Children...")]
	public static void SaveChildrenMeshInPlace (MenuCommand menuCommand)
	{
		Transform parent = menuCommand.context as Transform;

		string[] uniqueName = new string[parent.childCount];
		// Save MESH
		for(int i=0; i<parent.childCount; i++)
		{
			GameObject ch_sticker = parent.GetChild (i).gameObject;
			MeshFilter mf = ch_sticker.GetComponent<MeshFilter>();
			Mesh m = mf.sharedMesh;
			string path = asset_path + m.name + ".asset";
			uniqueName [i] = m.name;

			// mesh, path, make_new_instance?, optimize_mesh?
			SaveMeshBulk(m, path, false, true);
		}

		StickersTransformData savedData;
		savedData.stickersTran = new List<StickerTransform> ();
		// Save Transformation & sticker data
		for(int i=0; i<parent.childCount; i++)
		{
			Transform ch_tran = parent.GetChild (i);
			StickerTransform s_t = new StickerTransform (ch_tran.position, ch_tran.rotation, ch_tran.localScale, uniqueName [i]);
			savedData.stickersTran.Add (s_t);
		}

		if (File.Exists (json_path))
		{
			string dataAsJson = JsonUtility.ToJson (savedData, true);
			File.WriteAllText (json_path, dataAsJson);
		}
		else
		{
			Debug.LogWarning ("json file doesn't exit for saving data");
		}
	}

	[MenuItem("CONTEXT/Transform/Load n Create Tape in Children...")]
	public static void LoadnCreateChildren (MenuCommand menuCommand)
	{
		StickersTransformData loadedData;
		List<StickerTransform> tranData;

		Transform parent = menuCommand.context as Transform;
		Material material = Resources.Load("Materials/StickerSheet0Material_0", typeof(Material)) as Material;

		// Load JSON
		if (File.Exists (json_path))
		{
			string dataAsJson = File.ReadAllText (json_path);
			loadedData = JsonUtility.FromJson<StickersTransformData> (dataAsJson);
			tranData = loadedData.stickersTran;
		}
		else
		{
			Debug.LogWarning ("json file doesn't exit for loading data");
			return;
		}

		// Load MESH
		for(int i=0; i<tranData.Count; i++)
		{
			string a_path = asset_path + tranData [i].name + ".asset";
			Mesh meeesh = (Mesh)AssetDatabase.LoadAssetAtPath(a_path, typeof(Mesh));

			if (meeesh == null)
				continue;
			
			GameObject go = new GameObject ();
			go.name = "sticker_tape";

			MeshFilter m_f = go.AddComponent<MeshFilter> ();
			m_f.sharedMesh = meeesh;
			MeshRenderer m_r = go.AddComponent<MeshRenderer> ();
			m_r.material = material;

			go.transform.position = tranData[i].position;
			go.transform.rotation = tranData[i].rotation;
			go.transform.localScale = tranData[i].scale;
			go.transform.parent = parent;
			go.tag = "Sticker";
			go.layer = 11;

			go.AddComponent<MeshCollider> ();
			go.AddComponent<VRInteractiveObject> ();
			//Debug.Log ("loaded: " + meeesh.name);
		}
	}

	[MenuItem("CONTEXT/Transform/Save Plane in Children...")]
	public static void SaveChildrenHoseMeshInPlace (MenuCommand menuCommand)
	{
		Transform parent = menuCommand.context as Transform;

		StickersTransformIDData savedData;
		savedData.stickersTran = new List<StickerTransformID> ();
		// Save Transformation + Data (name doesn't matter)
		for(int i=0; i<parent.childCount; i++)
		{
			Transform ch_tran = parent.GetChild (i);
			Sticker s_ = ch_tran.gameObject.GetComponent<Sticker> ();
			StickerArt s_art = ch_tran.gameObject.GetComponent<StickerArt> ();
			StickerTransformID s_t = new StickerTransformID (ch_tran.position, ch_tran.rotation, ch_tran.localScale, "sticker_plane");
			if (s_ != null) {
				s_t.stickerID = s_.stickerId;
			} else {
				s_t.stickerID = s_art.stickerID;
			}
			savedData.stickersTran.Add (s_t);
		}

		if (File.Exists (json_path_2))
		{
			string dataAsJson = JsonUtility.ToJson (savedData, true);
			File.WriteAllText (json_path_2, dataAsJson);
		}
		else
		{
			Debug.LogWarning ("json file 2 doesn't exit for saving data");
		}
	}

	[MenuItem("CONTEXT/Transform/Load n Create Hose in Children...")]
	public static void LoadnCreateHoseChildren (MenuCommand menuCommand)
	{
		StickersTransformIDData loadedData;
		List<StickerTransformID> tranData;

		Transform parent = menuCommand.context as Transform;
		Material material = Resources.Load("Materials/StickerSheet0Material", typeof(Material)) as Material;
		GameObject stickerPrefab = Resources.Load ("StickerV5", typeof(GameObject)) as GameObject;
		StickerArt s_art = stickerPrefab.GetComponent<StickerArt> ();

		// Load JSON
		LoadStickerData();

		if (File.Exists (json_path_2))
		{
			string dataAsJson = File.ReadAllText (json_path_2);
			loadedData = JsonUtility.FromJson<StickersTransformIDData> (dataAsJson);
			tranData = loadedData.stickersTran;
		}
		else
		{
			Debug.LogWarning ("json file 2 doesn't exit for loading data");
			return;
		}

		// Instantiate MESH
		for(int i=0; i<tranData.Count; i++)
		{
			s_art.data = stickersById [tranData[i].stickerID];
			GameObject go = GameObject.Instantiate(stickerPrefab) as GameObject;

			go.transform.position = tranData[i].position;
			go.transform.rotation = tranData[i].rotation;
			go.transform.localScale = tranData[i].scale;
			go.transform.parent = parent;

			go.AddComponent<VRInteractiveObject> ();
			//Debug.Log ("loaded: " + meeesh.name);
		}
	}

	[MenuItem("CONTEXT/MeshFilter/Save Mesh...")]
	public static void SaveMeshInPlace (MenuCommand menuCommand) {
		MeshFilter mf = menuCommand.context as MeshFilter;
		Mesh m = mf.sharedMesh;
		SaveMesh(m, m.name, false, true);
	}

	[MenuItem("CONTEXT/MeshFilter/Save Mesh As New Instance...")]
	public static void SaveMeshNewInstanceItem (MenuCommand menuCommand) {
		MeshFilter mf = menuCommand.context as MeshFilter;
		Mesh m = mf.sharedMesh;
		SaveMesh(m, m.name, true, true);
	}

	public static void SaveMesh (Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh) {
		string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
		if (string.IsNullOrEmpty(path)) return;
        
		path = FileUtil.GetProjectRelativePath(path);

		Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;
		
		if (optimizeMesh)
		     MeshUtility.Optimize(meshToSave);
        
		AssetDatabase.CreateAsset(meshToSave, path);
		AssetDatabase.SaveAssets();
	}

	public static void SaveMeshBulk (Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh)
	{
		// check if already existed or not
		if(AssetDatabase.Contains(mesh))
			return;

		Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;

		if (optimizeMesh)
			MeshUtility.Optimize(meshToSave);

		AssetDatabase.CreateAsset(meshToSave, name);
		AssetDatabase.SaveAssets();
	}

	public static void LoadStickerData()
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, "StickerData/meta.json");

		if (File.Exists(filePath))
		{
			string dataAsJson = File.ReadAllText(filePath);
			StickerSheetData loadedData = JsonUtility.FromJson<StickerSheetData>(dataAsJson);
			data = loadedData.stickers;

			stickersById = new Dictionary<string, StickerData>();
			foreach (StickerData sticker in data)
			{
				stickersById[sticker.id] = sticker;
			}
			Debug.Log("Load stickers * " + data.Count);
		}
		else
		{
			Debug.LogError("Cannot load game data!");
		}
	}

	//============================== Spline =======================================
	[MenuItem("CONTEXT/Transform/Save Balls in Children...")]
	public static void SaveChildrenBalls (MenuCommand menuCommand)
	{
		Transform parent = menuCommand.context as Transform;

		BallTransformData savedData;
		savedData.ballsTran = new List<BallTransform> ();
		// Save Transformation + Data (name doesn't matter)
		for(int i=0; i<parent.childCount; i++)
		{
			Transform ch_tran = parent.GetChild (i);
			BallTransform s_t = new BallTransform (ch_tran.position.ToString());
			savedData.ballsTran.Add (s_t);
		}

		if (File.Exists (json_path_3))
		{
			string dataAsJson = JsonUtility.ToJson (savedData, true);
			File.WriteAllText (json_path_3, dataAsJson);
		}
		else
		{
			Debug.LogWarning ("json file 3 doesn't exit for saving data");
		}
	}
	
}
