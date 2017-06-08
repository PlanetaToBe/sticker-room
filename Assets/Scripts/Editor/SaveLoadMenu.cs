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
        string path = EditorUtility.OpenFilePanel("Overwrite with png", "C:\\Users\\planeta\\AppData\\LocalLow\\OONI\\StickerTime\\saves", "dat");
        StickerSceneManager.instance.Load(path);
    }
}
