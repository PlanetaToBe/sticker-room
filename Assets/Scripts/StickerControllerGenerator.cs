using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StickerControllerGenerator : MonoBehaviour {

	// from Controllers
	public event Action<GameObject, uint> OnCreateSticker;
	public event Action OnReleaseTrigger;
	public float createStickerInterval = 0.5f;

	private SteamVR_TrackedController controller;
	private bool isPainting = false;
	private GrabnStretch grabnStretch;
	private Vector3 stickerSize;
	private bool doHosing = false;

	public Vector3 StickerSize
	{
		get { return stickerSize * grabnStretch.PlayerScale;}
	}

	void Start()
	{
		grabnStretch = GetComponent<GrabnStretch> ();
		stickerSize = StickerSceneManager.instance.stickerPrefab.transform.localScale;
	}

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
		if(grabnStretch.InSelfScalingMode || grabnStretch.InSelfScalingSupportMode)
			return;

		doHosing = true;
		StartCoroutine (WaitAndHose());
		
//		GameObject sticker = Instantiate(StickerSceneManager.instance.stickerPrefab, transform.position, Quaternion.identity) as GameObject;
//		sticker.transform.localScale = StickerSize;
//
//		if (OnCreateSticker != null)
//			OnCreateSticker (sticker, controller.controllerIndex);
	}

	void ReleaseTrigger(object sender, ClickedEventArgs e)
	{
		if(grabnStretch.InSelfScalingMode || grabnStretch.InSelfScalingSupportMode)
			return;

		doHosing = false;
		
		if (OnReleaseTrigger != null)
			OnReleaseTrigger ();
	}

	private IEnumerator WaitAndHose()
	{
		while (doHosing)
		{
			GameObject sticker = Instantiate(StickerSceneManager.instance.stickerPrefab, transform.position, Quaternion.identity) as GameObject;
			sticker.transform.localScale = StickerSize;

			if (OnCreateSticker != null)
				OnCreateSticker (sticker, controller.controllerIndex);
			
			yield return new WaitForSeconds(createStickerInterval);
		}
	}
}
