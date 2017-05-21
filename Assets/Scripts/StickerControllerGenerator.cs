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

	public StickerTool myTool;
	private bool inUse;

	public Vector3 StickerSize
	{
		get { return stickerSize * grabnStretch.PlayerScale; }
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

		if(myTool!=null)
			myTool.OnChangeToolStatus += OnToolStatusChange;
	}

	void OnDisable()
	{
		controller.TriggerClicked -= CreateSticker;
		controller.TriggerUnclicked -= ReleaseTrigger;

		if(myTool!=null)
			myTool.OnChangeToolStatus -= OnToolStatusChange;
	}

	private void OnToolStatusChange(bool _inUse, int toolIndex)
	{
		inUse = _inUse;
	}

	void CreateSticker(object sender, ClickedEventArgs e)
	{
		if(!inUse)
			return;
		
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
		if(!inUse)
			return;
		
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
			// TODO: rotate with controller's transform.forward
			GameObject sticker = Instantiate(StickerSceneManager.instance.stickerPrefab, transform.position, Quaternion.identity) as GameObject;
			sticker.transform.localScale = StickerSize;
			sticker.tag = "Sticker";

			if (OnCreateSticker != null)
				OnCreateSticker (sticker, controller.controllerIndex);
			
			yield return new WaitForSeconds(createStickerInterval);

			// cuz hosing => auto removedJoint
			if (OnReleaseTrigger != null)
				OnReleaseTrigger ();
		}
	}
}
