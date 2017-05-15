using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawManager : MonoBehaviour {

	//public ViveSimpleController viveController;
	private SteamVR_TrackedController controller;

	public Material material;
	public GameObject drawPoint;
	public float lineSize = 0.005f;

	[Header("Behaviors")]
	public bool snapToThing = false;
	private int wallLayer;

	private MeshLineRenderer currLine;
	private int numClicks = 0;

	void Start()
	{
		wallLayer = 1 << 8;
	}

	void OnEnable()
	{
		if (controller == null)
		{
			controller = GetComponent<SteamVR_TrackedController> ();
		}

		controller.TriggerClicked += OnTriggerDown;
		controller.TriggerDowning += OnTriggerTouch;
		controller.TriggerUnclicked += OnTriggerUp;
	}

	void OnDisable()
	{
		controller.TriggerClicked -= OnTriggerDown;
		controller.TriggerDowning -= OnTriggerTouch;
		controller.TriggerUnclicked -= OnTriggerUp;
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
	}

	private void OnTriggerTouch(object sender, ClickedEventArgs e)
	{
		if (currLine == null)
			return;

		if(snapToThing)
		{
			RaycastHit hit;
			if(Physics.Raycast(transform.position, transform.forward, out hit, 50f, wallLayer))
			{
				currLine.AddPoint (hit.point);
			}
		}
		else
		{
			currLine.AddPoint (drawPoint.transform.position);
		}

		numClicks++;
	}

	private void OnTriggerUp(object sender, ClickedEventArgs e)
	{
		numClicks = 0;
		currLine = null;
	}
}
