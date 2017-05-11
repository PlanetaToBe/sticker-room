using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawManager : MonoBehaviour {

	public ViveSimpleController viveController;
	public Material material;
	public GameObject drawPoint;
	public float lineSize = 0.005f;

	private MeshLineRenderer currLine;
	private int numClicks = 0;

	void OnEnable()
	{
		if (viveController == null)
		{
			viveController = GetComponent<ViveSimpleController> ();
		}

		viveController.OnTriggerDown += OnTriggerDown;
		viveController.OnTriggerTouch += OnTriggerTouch;
		viveController.OnTriggerUp += OnTriggerUp;
	}

	void OnDisable()
	{
		viveController.OnTriggerDown -= OnTriggerDown;
		viveController.OnTriggerTouch -= OnTriggerTouch;
		viveController.OnTriggerUp -= OnTriggerUp;
	}

	private void OnTriggerDown(GameObject _object)
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

	private void OnTriggerTouch(GameObject _object)
	{
		if (currLine == null)
			return;
		
		currLine.AddPoint (drawPoint.transform.position);
		numClicks++;
	}

	private void OnTriggerUp(GameObject _object)
	{
		numClicks = 0;
		currLine = null;
	}
}
