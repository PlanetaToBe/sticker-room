using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerSaveManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        SteamVR_TrackedController controller = GetComponent<SteamVR_TrackedController>();
        controller.MenuButtonClicked += TriggerSave;
    }

    public void TriggerSave(object sender, ClickedEventArgs e)
    {
        StickerSceneManager.instance.Save();
    }
}
