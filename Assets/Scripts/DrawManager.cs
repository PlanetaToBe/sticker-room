using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawManager : MonoBehaviour {

	//public ViveSimpleController viveController;
	private SteamVR_TrackedController controller;
	private GrabnStretch grabnstretch;

	public Material material;

	public GameObject drawPoint;
	public float minDrawDistance = 0.07f;
	public int textureHorizontalCount = 1;
	public int textureVerticalCount = 1;
	public Transform allStickerTapesParent;

	public float DrawDistance
	{
		get { return minDrawDistance * grabnstretch.PlayerScale;}
	}

	public enum DrawType
	{
		InAir,
		OnThing
	}
	[Header("Behaviors")]
	public DrawType drawType = DrawType.OnThing;
	private int wallLayer;
	private int thingLayer;
	private int finalMask;
	//private MeshLineRenderer currLine;
	private StickerTapeRenderer currLine;
	private int numClicks = 0;

	private Vector3 past_DrawPosition;
	private Vector3 past_HitPosition;

	public StickerTool[] myTools;
	public Tool ketchup;
	private bool inUse;

	private SwapArtist swapArtist;

	[Header("Audios")]
	public AudioClip penSound;
	public AudioClip spraySound;
	private GvrAudioSource audioSource;

	void Start()
	{
		wallLayer = 1 << 8;
		thingLayer = 1 << 9;
		finalMask = wallLayer | thingLayer;
		grabnstretch = GetComponent<GrabnStretch> ();
		swapArtist = GetComponentInParent<SwapArtist> ();
		audioSource = GetComponent<GvrAudioSource> ();
	}

	void OnEnable()
	{
		if (controller == null)
		{
			controller = GetComponent<SteamVR_TrackedController> ();
		}

		controller.TriggerClicked += OnDown;
		controller.TriggerDowning += OnTouch;
		controller.TriggerUnclicked += OnUp;

		if (myTools.Length != 0) {
			for(int i=0; i<myTools.Length; i++)
			{
				myTools[i].OnChangeToolStatus += OnToolStatusChange;
			}
		}			
	}

	void OnDisable()
	{
		controller.TriggerClicked -= OnDown;
		controller.TriggerDowning -= OnTouch;
		controller.TriggerUnclicked -= OnUp;

		if (myTools.Length != 0) {
			for(int i=0; i<myTools.Length; i++)
			{
				myTools[i].OnChangeToolStatus -= OnToolStatusChange;
			}
		}	
	}

	private void OnToolStatusChange(bool _inUse, int toolIndex)
	{
		inUse = _inUse;

		if(inUse)
		{
			if(toolIndex==1)
				drawType = DrawType.OnThing;
			else
				drawType = DrawType.InAir;
		}
	}

	private void OnDown(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;
		
		GameObject go = new GameObject ();
		go.name = "sticker_tape";
		go.AddComponent<MeshFilter> ();
		go.AddComponent<MeshRenderer> ();
		go.transform.position = drawPoint.transform.position;
		go.transform.parent = allStickerTapesParent;
		go.tag = "Sticker";
		go.layer = 11;

		currLine = go.AddComponent<StickerTapeRenderer> ();
		//currLine = go.AddComponent<MeshLineRenderer> ();
		if(swapArtist)
			currLine.swapArtist = swapArtist;
		currLine.material = material;
		currLine.SetWidth (DrawDistance);
		currLine.drawPoint = drawPoint;
		currLine.selfObject = go;
		currLine.TextureHorizontalCount = textureHorizontalCount;
		currLine.TextureVerticalCount = textureVerticalCount;

		switch(drawType)
		{
		case DrawType.OnThing:
			currLine.DrawOnThing = true;

			if(ketchup)
				ketchup.OnDown ();

			if (audioSource)
			{
				audioSource.clip = spraySound;
				audioSource.time = Random.Range (0f, 5f);
				audioSource.Play ();
			}
			break;

		case DrawType.InAir:
			currLine.DrawOnThing = false;
			currLine.AddPoint (drawPoint.transform.position, false);
			numClicks++;

			if (audioSource)
			{
				audioSource.clip = penSound;
				audioSource.time = Random.Range (0f, 10f);
				audioSource.Play ();
			}
			break;
		}

		past_DrawPosition = drawPoint.transform.position;
	}

	private void OnTouch(object sender, ClickedEventArgs e)
	{
		if (!inUse || currLine == null)
			return;

		Vector3 offset;

		switch(drawType)
		{
		case DrawType.OnThing:
			RaycastHit hit;
			if (Physics.Raycast (transform.position, transform.forward, out hit, 50f, finalMask))
			{
				offset = hit.point - past_HitPosition;
				if (offset.sqrMagnitude < DrawDistance * DrawDistance)
					return;

				currLine.SurfaceNormal = hit.normal;
				currLine.AddPoint (hit.point, false);
				past_HitPosition = hit.point;
				numClicks++;
			}
			else {
				// if hit nothing, end and break the tape
				numClicks = 0;
				currLine = null;
			}
			break;

		case DrawType.InAir:
			// if the controller is not moving, velocity near zero => return
			offset = drawPoint.transform.position - past_DrawPosition;
			float sqrLen = offset.sqrMagnitude;
			//Debug.Log (sqrLen);
			if (sqrLen < DrawDistance * DrawDistance)
				return;
			
			currLine.AddPoint (drawPoint.transform.position, false);
			numClicks++;
			break;
		}

		past_DrawPosition = drawPoint.transform.position;
	}

	private void OnUp(object sender, ClickedEventArgs e)
	{
		if (inUse && currLine != null)
		{
			if (numClicks > 3) {
				var m_c = currLine.gameObject.AddComponent<MeshCollider> ();
				//m_c.convex = true;
			} else {
				currLine.gameObject.AddComponent<BoxCollider> ();
			}
			currLine.gameObject.AddComponent<VRInteractiveObject> ();
		}

		numClicks = 0;
		currLine = null;

		switch (drawType)
		{
		case DrawType.OnThing:
			if(ketchup)
				ketchup.OnUp ();
			break;
		}

		if (audioSource)
		{
			audioSource.Stop ();
		}
	}

	SteamVR_Controller.Device GetDevice(int _index)
	{
		return SteamVR_Controller.Input (_index);
	}
}
