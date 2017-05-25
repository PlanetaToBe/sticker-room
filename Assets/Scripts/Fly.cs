using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : MonoBehaviour {

	public float flySpeed = 0.2f;
	public float flyDistance = 2f;
	public ToolHub[] toolHubs;
	public SteamVR_TrackedController[] controllers;

	private int padDownCount = 0;
	private List<Transform> controllerTrans = new List<Transform>();
	private bool isFlying = false;
	private bool justFinishFlying = false;
	private Transform player;
	private Vector3 roomCenter = new Vector3 (0, 1.7f, 0);

	public float PlayerSize
	{
		get { return player.localScale.x; }
	}

	void OnEnable()
	{
		for(int i=0; i<controllers.Length; i++)
		{
			controllers[i].PadClicked += HandlePadDown;
			controllers[i].PadUnclicked += HandlePadUp;
			controllers[i].PadTouching += HandlePadTouching;
		}
	}

	void OnDisable()
	{
		for(int i=0; i<controllers.Length; i++)
		{
			controllers[i].PadClicked -= HandlePadDown;
			controllers[i].PadUnclicked -= HandlePadUp;
			controllers[i].PadTouching -= HandlePadTouching;
		}
	}

	void Start()
	{
		for(int i=0; i<controllers.Length; i++)
		{
			controllerTrans.Add(controllers[i].transform);
		}

		player = transform.parent;
	}

	void Update()
	{
		if (justFinishFlying)
		{
			if (player.position.y<=0f)
			{
				//Debug.Log ("land on floor!");
				player.position = new Vector3 (player.position.x, 0f, player.position.z);
				justFinishFlying = false;
				for(int i=0; i<toolHubs.Length; i++)
				{
					toolHubs [i].EnableAllTools ();
				}
			} else {
				player.Translate (Vector3.down * Time.deltaTime * flySpeed * 2f * PlayerSize);
			}
		}
	}

	private void HandlePadDown(object sender, ClickedEventArgs e)
	{
		padDownCount++;

		if(padDownCount==2 && !isFlying)
		{
			// disable all tool functions
			for(int i=0; i<toolHubs.Length; i++)
			{
				toolHubs [i].DisableAllTools ();
			}
			isFlying = true;
		}
	}

	private void HandlePadUp(object sender, ClickedEventArgs e)
	{
		padDownCount--;
		//Debug.Log ("padDownCount: " + padDownCount);
		if(isFlying)
		{
			justFinishFlying = true;
		}
		isFlying = false;
	}

	private void HandlePadTouching(object sender, ClickedEventArgs e)
	{
		if(isFlying)
		{
			if (IsInBounds (player.position, roomCenter))
			{
				Vector3 aveVec = (controllerTrans[0].forward + controllerTrans[1].forward) / 2f;
				player.Translate (aveVec * Time.deltaTime * flySpeed * PlayerSize);
			}
			//Debug.Log ("flying!");
		}
	}

	public bool IsInBounds(Vector3 pos, Vector3 center)
	{
		//float distToCenter = (pos - center).sqrMagnitude;
		//Debug.Log (distToCenter);
		return (pos-center).sqrMagnitude < (flyDistance * flyDistance);
	}
}
