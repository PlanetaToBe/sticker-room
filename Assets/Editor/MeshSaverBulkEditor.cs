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

	public StickerTransform (Vector3 _position, Quaternion _rotation, Vector3 _scale, int _index)
	{
		this.position = _position;
		this.rotation = _rotation;
		this.scale = _scale;
		this.name = "sticker_tape_" + _index;
	}
}

public static class MeshSaverBulkEditor {

	static string json_path = Application.dataPath + "/Sticker/Exports/data.json";
	static string asset_path = "Assets/Sticker/Exports/Assets/";

	[MenuItem("CONTEXT/Transform/Save Children Mesh...")]
	public static void SaveChildrenMeshInPlace (MenuCommand menuCommand)
	{
		Transform parent = menuCommand.context as Transform;

		// Save MESH
		for(int i=0; i<parent.childCount; i++)
		{
			GameObject ch_sticker = parent.GetChild (i).gameObject;
			MeshFilter mf = ch_sticker.GetComponent<MeshFilter>();
			Mesh m = mf.sharedMesh;
			string m_name = asset_path + m.name + "_" + i + ".asset";

			// mesh, path, make_new_instance?, optimize_mesh?
			SaveMeshBulk(m, m_name, false, true);
		}

		//string json_path = Application.dataPath + "/Sticker/Exports/data.json";

		StickersTransformData savedData;
		savedData.stickersTran = new List<StickerTransform> ();
		// Save Transformation
		for(int i=0; i<parent.childCount; i++)
		{
			Transform ch_tran = parent.GetChild (i);
			StickerTransform s_t = new StickerTransform (ch_tran.position, ch_tran.rotation, ch_tran.localScale, i);
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

	[MenuItem("CONTEXT/Transform/Load n Create Children...")]
	public static void LoadnCreateChildren (MenuCommand menuCommand)
	{
		StickersTransformData loadedData;
		List<StickerTransform> tranData;

		Transform parent = menuCommand.context as Transform;

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

			GameObject go = new GameObject ();
			go.name = "sticker_tape";
			MeshFilter m_f = go.AddComponent<MeshFilter> ();
			m_f.sharedMesh = meeesh;
			go.AddComponent<MeshRenderer> ();
			go.transform.position = tranData[i].position;
			go.transform.rotation = tranData[i].rotation;
			go.transform.localScale = tranData[i].scale;
			go.transform.parent = parent;
			go.tag = "Sticker";
			go.layer = 11;

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
		Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;

		if (optimizeMesh)
			MeshUtility.Optimize(meshToSave);

		AssetDatabase.CreateAsset(meshToSave, name);
		AssetDatabase.SaveAssets();
	}
	
}
