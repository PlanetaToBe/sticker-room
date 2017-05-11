using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StickerControllerGenerator : MonoBehaviour {

	// from Controllers
	public event Action<GameObject, uint> OnCreateSticker;
	public event Action OnReleaseTrigger;

	private SteamVR_TrackedController controller;
	private bool isPainting = false;

	void OnEnable()
	{
		if (controller == null)
			controller = GetComponent<SteamVR_TrackedController> ();
		
		controller.TriggerClicked += CreateSticker;
		controller.TriggerUnclicked += ReleaseTrigger;
	}

	void OnDisable()
	{
		controller.TriggerClicked -= CreateSticker;
		controller.TriggerUnclicked -= ReleaseTrigger;
	}
		
	void CreateSticker(object sender, ClickedEventArgs e)
	{
		GameObject sticker = Instantiate(StickerSceneManager.instance.stickerPrefab, transform.position, Quaternion.identity) as GameObject;

		if (OnCreateSticker != null)
			OnCreateSticker (sticker, controller.controllerIndex);
	}

	void ReleaseTrigger(object sender, ClickedEventArgs e)
	{
		if (OnCreateSticker != null)
			OnReleaseTrigger ();
	}
}
