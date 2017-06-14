using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SaveLoadMenu : EditorWindow {
    [MenuItem("Sticker Room/Save", false, 120)]
    static void Save()
    {
        StickerSceneManager.instance.Save();
    }

    [MenuItem("Sticker Room/Load", false, 120)]
    static void Load()
    {
        string path = EditorUtility.OpenFilePanel("Choose save file", "C:\\Users\\planeta\\AppData\\LocalLow\\OONI\\StickerTime\\saves", "dat");

        if (path != "")
        {
            StickerSceneManager.instance.Load(path);
        }
    }

    [MenuItem("Sticker Room/Load Additive", false, 120)]
    static void LoadAdditive()
    {
        string path = EditorUtility.OpenFilePanel("Choose save file", "C:\\Users\\planeta\\AppData\\LocalLow\\OONI\\StickerTime\\saves", "dat");

        if (path != "")
        {
            StickerSceneManager.instance.Load(path, true);
        }
    }

	[MenuItem("Sticker Room/Clean", false, 120)]
	static void Clean() {
		StickerSceneManager.instance.Clean ();
	}

    [MenuItem("Sticker Room/Reset", false, 120)]
    static void Reset()
    {
        StickerSceneManager.instance.Reset();
    }
}
