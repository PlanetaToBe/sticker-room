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

	private float newGroundHeight = 0f;
	private Collider newGroundCollider;
	private int wallLayer;
	private int stickerLayer;
	private int finalLandingMask;

	public Transform pivot;
	public Transform spine;

	public Vector3 RaycastSpot
	{
		get { 
			return (spine.position - pivot.position) / 4f;
		}
	}

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

		wallLayer = 1 << 8;
		stickerLayer = 1 << 11;
		finalLandingMask = wallLayer | stickerLayer;
	}

	void Update()
	{
		if (justFinishFlying)
		{
			if (player.position.y < newGroundHeight)
			{
				Debug.Log ("land on floor!");
				player.position = new Vector3 (player.position.x, newGroundHeight, player.position.z);
				justFinishFlying = false;
				for(int i=0; i<toolHubs.Length; i++)
				{
					toolHubs [i].EnableAllTools ();
				}
			}
			else
			{
				// when falling, check if new ground height change
				RaycastHit hit;
				if (Physics.Raycast(pivot.position, Vector3.down, out hit, 50f, finalLandingMask))
				{
					if (newGroundCollider != hit.collider)
					{
						newGroundCollider = hit.collider;
						Debug.Log ("hit something diff when falling: " + hit.collider.name);
					}						

					newGroundHeight = hit.point.y;
					Debug.DrawLine(pivot.position, hit.point);
				}
				player.Translate (Vector3.down * Time.deltaTime * flySpeed * 2f * PlayerSize);
			}
		}
//		else if(!isFlying)
//		{
//			// during walking, check if new ground change
//			RaycastHit hit;
//			if (Physics.Raycast(RaycastSpot, Vector3.down, out hit, 50f, finalLandingMask))
//			{
//				if (newGroundCollider != hit.collider)
//				{
//					newGroundCollider = hit.collider;
//					Debug.Log ("hit something diff when walking: " + hit.collider.name);
//				}
//				newGroundHeight = hit.point.y;
//				Debug.DrawLine(pivot.position, hit.point);
//			}
//
////			float steps = Time.deltaTime * flySpeed * 2f * PlayerSize;
////			if (player.position.y > newGroundHeight + steps) {
////				player.Translate (Vector3.down * steps);
////			}
//		}

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

			// Raycast down to see if will landing on stickers
			RaycastHit hit;
			if (Physics.Raycast(pivot.position, Vector3.down, out hit, 50f, finalLandingMask))
			{
				newGroundCollider = hit.collider;
				newGroundHeight = hit.point.y;
//				sphere.transform.position = hit.point;
				Debug.Log ("hit something first time: " + hit.collider.name);

				Debug.DrawLine(pivot.position, hit.point);
			}
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
//		bool inRadius = (pos - center).sqrMagnitude < (flyDistance * flyDistance);
//		bool aboveGround = pos.y >= 0;
		return (pos - center).sqrMagnitude < (flyDistance * flyDistance) ||  pos.y >= 0;
	}
}
