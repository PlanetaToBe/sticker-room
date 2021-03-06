﻿using System.Collections;
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

	public GameObject stickerPrefab;
	private StickerArt stickerArt;
	private SwapArtist swapArtist;
	private ToolHub toolHub;
	private bool randomSticker = false;

	private float s_size_scalar = 0.2f;

	public Vector3 StickerSize
	{
		get { 
			if (toolHub) {
				return stickerSize * s_size_scalar * grabnStretch.PlayerScale * toolHub.TouchPadDrawWidth;
			} else {
				return stickerSize * s_size_scalar * grabnStretch.PlayerScale;
			}
		}
	}

	public float StickerPhysicScalar
	{
		get { 
			if (toolHub) {
				return grabnStretch.PlayerScale * toolHub.TouchPadDrawWidth;
			} else {
				return grabnStretch.PlayerScale;
			}
		}
	}

	void Start()
	{
		grabnStretch = GetComponent<GrabnStretch> ();
		//stickerSize = StickerSceneManager.instance.stickerPrefab.transform.localScale;
		stickerSize = stickerPrefab.transform.localScale;
		stickerArt = stickerPrefab.GetComponent<StickerArt> ();
		swapArtist = GetComponentInParent<SwapArtist> ();
		toolHub = myTool.transform.GetComponentInParent<ToolHub> ();
	}

	void OnEnable()
	{
		if (controller == null)
			controller = GetComponent<SteamVR_TrackedController> ();
		
		controller.TriggerClicked += CreateSticker;
		controller.TriggerUnclicked += ReleaseTrigger;

		controller.PadClicked += OnPadClick;
		controller.PadUnclicked += OnPadUnclick;

		if(myTool!=null)
			myTool.OnChangeToolStatus += OnToolStatusChange;
	}

	void OnDisable()
	{
		controller.TriggerClicked -= CreateSticker;
		controller.TriggerUnclicked -= ReleaseTrigger;

		controller.PadClicked -= OnPadClick;
		controller.PadUnclicked -= OnPadUnclick;

		if(myTool!=null)
			myTool.OnChangeToolStatus -= OnToolStatusChange;
	}

	private void OnToolStatusChange(bool _inUse, int toolIndex)
	{
		inUse = _inUse;
		// reset if not in use
		if(!inUse)
		{
			doHosing = false;

			if (OnReleaseTrigger != null)
				OnReleaseTrigger ();
		}
	}

	private void OnPadClick(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;

		randomSticker = true;
	}

	private void OnPadUnclick(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;

		randomSticker = false;
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
		
//		if(grabnStretch.InSelfScalingMode || grabnStretch.InSelfScalingSupportMode)
//			return;

		doHosing = false;
		
		if (OnReleaseTrigger != null)
			OnReleaseTrigger ();
	}

	private IEnumerator WaitAndHose()
	{
		while (doHosing)
		{
			// TODO: rotate with controller's transform.forward
			//GameObject sticker = Instantiate(StickerSceneManager.instance.stickerPrefab, transform.position, Quaternion.identity) as GameObject;

			GameObject sticker;
			if (swapArtist)
			{
                StickerData s_data;
				if (randomSticker)
                {
                     s_data = swapArtist.GetStickerDataRandom();
                } else
                {
                    s_data = swapArtist.GetStickerData();
                }

				sticker = Instantiate(stickerPrefab, transform.position, transform.rotation) as GameObject;
                sticker.GetComponent<Sticker>().SetSticker(s_data.id);
			}
			else
			{
				sticker = Instantiate(StickerSceneManager.instance.stickerPrefab, transform.position, transform.rotation) as GameObject;
			}
				
			sticker.transform.localScale = StickerSize;
			//sticker.tag = "Sticker";

			if (OnCreateSticker != null)
				OnCreateSticker (sticker, controller.controllerIndex);
			
			yield return new WaitForSeconds(createStickerInterval);

			// cuz hosing => auto removedJoint
			if (OnReleaseTrigger != null)
				OnReleaseTrigger ();
		}
	}
}
