using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawManager : MonoBehaviour {

	//public ViveSimpleController viveController;
	private SteamVR_TrackedController controller;

	public Material material;
	public GameObject drawPoint;
	public float lineSize = 0.005f;


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

	private MeshLineRenderer currLine;
	private int numClicks = 0;


	void Start()
	{
		wallLayer = 1 << 8;
		thingLayer = 1 << 9;
		finalMask = wallLayer | thingLayer;
	}

	void OnEnable()
	{
		if (controller == null)
		{
			controller = GetComponent<SteamVR_TrackedController> ();
		}

		controller.PadClicked += OnTriggerDown;
		controller.PadTouching += OnTriggerTouch;
		controller.PadUnclicked += OnTriggerUp;
	}

	void OnDisable()
	{
		controller.PadClicked -= OnTriggerDown;
		controller.PadTouching -= OnTriggerTouch;
		controller.PadUnclicked -= OnTriggerUp;
	}

	private void OnTriggerDown(object sender, ClickedEventArgs e)
	{
		GameObject go = new GameObject ();
		go.AddComponent<MeshFilter> ();
		go.AddComponent<MeshRenderer> ();
		go.transform.position = drawPoint.transform.position;

		currLine = go.AddComponent<MeshLineRenderer> ();
		currLine.material = material;
		currLine.SetWidth (lineSize);
		currLine.drawPoint = drawPoint;

		switch(drawType)
		{
		case DrawType.OnThing:
			currLine.DrawOnThing = true;
			break;

		case DrawType.InAir:
			currLine.DrawOnThing = false;
			break;
		}
	}

	private void OnTriggerTouch(object sender, ClickedEventArgs e)
	{
		if (currLine == null)
			return;

		switch(drawType)
		{
		case DrawType.OnThing:
			RaycastHit hit;
			if (Physics.Raycast (transform.position, transform.forward, out hit, 50f, finalMask)) {
				currLine.SurfaceNormal = hit.normal;
				currLine.AddPoint (hit.point);
			} else {
				numClicks = 0;
				currLine = null;
			}
			break;

		case DrawType.InAir:
			currLine.AddPoint (drawPoint.transform.position);
			break;
		}

		numClicks++;
	}

	private void OnTriggerUp(object sender, ClickedEventArgs e)
	{
		numClicks = 0;
		currLine = null;
	}
}
