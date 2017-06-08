using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LineStickerBrushTool : MonoBehaviour {

    public float stickerSpacing = .1f;
    public float stickerScale = .1f;
    public float stickerPadding = .02f;

    private bool isPainting = false;
    private Vector3 lastStickerPosition;
    private Quaternion lastStickerRotation;
    private SteamVR_TrackedController controller;
    private StickerData nextStickerData;

	public StickerTool myTool;
	public GameObject stickersParent;
	private bool inUse;
	private bool randomSticker = false;

	private SwapArtist swapArtist;
	private ToolHub toolHub;
	private GrabnStretch grabnStretch;
	private GameObject stickerPrefab;

	public float StickerSize
	{
		get { 
			if (toolHub) {
				return stickerScale * grabnStretch.PlayerScale * toolHub.TouchPadDrawWidth;
			} else {
				return stickerScale * grabnStretch.PlayerScale;
			}
		}
	}

    void Start()
	{
		grabnStretch = GetComponent<GrabnStretch> ();
		swapArtist = GetComponentInParent<SwapArtist> ();
		toolHub = myTool.transform.GetComponentInParent<ToolHub> ();
		stickerPrefab = StickerSceneManager.instance.stickerPrefab;

		if (swapArtist)
		{
			if(randomSticker)
				nextStickerData = swapArtist.GetStickerDataRandom();
			else
				nextStickerData = swapArtist.GetStickerData();
		}
		else
		{
			nextStickerData = StickerSceneManager.instance.GetRandomSticker();
		}
    }

	void OnEnable()
	{
		if (controller == null)
			controller = GetComponent<SteamVR_TrackedController> ();

		controller.TriggerClicked += StartPainting;
		controller.TriggerUnclicked += StopPainting;

		controller.PadClicked += OnPadClick;
		controller.PadUnclicked += OnPadUnclick;

		if(myTool!=null)
			myTool.OnChangeToolStatus += OnToolStatusChange;
	}

	void OnDisable()
	{
		controller.TriggerClicked -= StartPainting;
		controller.TriggerUnclicked -= StopPainting;

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
			isPainting = false;
			lastStickerPosition = new Vector3(100, 100, 100);
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

    void StartPainting(object sender, ClickedEventArgs e)
    {
		if(!inUse)
			return;
		
        isPainting = true;
        // StartCoroutine(Paint());
    }

    void StopPainting(object sender, ClickedEventArgs e)
    {
		if(!inUse)
			return;
		
        isPainting = false;
        lastStickerPosition = new Vector3(100, 100, 100);
    }

    private void Update()
    {
        if (isPainting && lastStickerPosition == null)
        {
            AddSticker();
        } else if (isPainting)
        {
            float nextStickerWidth = stickerScale * nextStickerData.width / nextStickerData.height;
            Vector3 offset = transform.position - lastStickerPosition;
            if (offset.magnitude > stickerSpacing) AddSticker();
        }
    }

    private void AddSticker()
    {

        Vector3 offset = lastStickerPosition - transform.position;
        Quaternion rot = Quaternion.Euler(0, 90, 0);
        Quaternion stickerTargetRotation = Quaternion.FromToRotation(Vector3.right, offset);
        Quaternion stickerRotation = stickerTargetRotation;

		Sticker newSticker = stickerPrefab.GetComponent<Sticker>();
        newSticker.stickerId = nextStickerData.id;
		GameObject sticker = Instantiate(stickerPrefab, transform.position, stickerRotation) as GameObject;
        
		VRInteractiveObject intObj = sticker.AddComponent<VRInteractiveObject> ();
		intObj.usePhysics = false;

        //sticker.transform.localScale = new Vector3(stickerScale, stickerScale, stickerScale);
		sticker.transform.localScale = new Vector3(StickerSize, StickerSize, StickerSize);

        //float stickerWidth = stickerScale * nextStickerData.width / nextStickerData.height;
		float stickerWidth = StickerSize * nextStickerData.width / nextStickerData.height;

        lastStickerRotation = stickerRotation;
        lastStickerPosition = transform.position;

		if (swapArtist)
		{
			if (randomSticker) {
				nextStickerData = swapArtist.GetStickerDataRandom ();
				Debug.Log ("GetStickerDataRandom");
			} else {
				nextStickerData = swapArtist.GetStickerData ();
				Debug.Log ("GetStickerData");
			}
		}
		else
		{
			nextStickerData = StickerSceneManager.instance.GetRandomSticker();
			Debug.Log ("GetRandomSticker");
		}

		sticker.transform.SetParent (stickersParent.transform);
    }
}
