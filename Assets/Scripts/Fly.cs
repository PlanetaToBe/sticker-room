using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : MonoBehaviour {

	public enum FlyType
	{
		NonPhysics,
		Physics
	}
	public FlyType flyType = FlyType.NonPhysics;

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
	public int FlyMask
	{
		get { return finalLandingMask; }
	}
	private PlayerMovement playerMovement;

	public Transform cameraRig;
	public Transform cameraEye;
	public Transform Pivot
	{
		get {
			var tmpP = cameraEye;
			tmpP.localPosition.Set(tmpP.localPosition.x, 0, tmpP.localPosition.z);
			return tmpP;
		}
	}

	public float PlayerSize
	{
		get { return player.localScale.x; }
	}

	private Vector3 m_flyVector;
	public Vector3 FlyVector
	{
		get { return m_flyVector; }
		set { m_flyVector = value; }
	}
	public float FlyStep
	{
		get { return Time.deltaTime * flySpeed * PlayerSize; }
	}

	public ParticleSystem[] particles;
	private Vector3[] particleOriPositions;

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
		int thingLayer = 1 << 9;
		finalLandingMask = wallLayer | stickerLayer | thingLayer;

		playerMovement = GetComponentInParent<PlayerMovement> ();

		particleOriPositions = new Vector3[particles.Length];
		particleOriPositions [0] = particles [0].transform.localPosition;
		particleOriPositions [1] = particles [1].transform.localPosition;
	}

	void Update()
	{
		switch(flyType)
		{
		case FlyType.NonPhysics:
			if (justFinishFlying)
			{
				RaycastHit hit;
				if (Physics.Raycast(Pivot.position, Vector3.down, out hit, 50f, finalLandingMask))
				{
					if(newGroundCollider != hit.collider)
					{
						newGroundCollider = hit.collider;
						Debug.Log ("hit something diff when falling: " + hit.collider.name);
					}
					newGroundHeight = hit.point.y;
					Debug.DrawLine(Pivot.position, hit.point);
				}

				if(player.position.y > newGroundHeight + FlyStep*2)
				{
					player.Translate (Vector3.down * FlyStep*2);
				}
				else
				{
					Debug.Log ("land on floor!");
					player.position = new Vector3 (player.position.x, newGroundHeight, player.position.z);
					justFinishFlying = false;
					for(int i=0; i<toolHubs.Length; i++)
					{
						toolHubs [i].EnableAllTools ();
					}
				}

				//v.1
				/*
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
				*/
			}
			/*
			else if(!isFlying)
			{
				// during walking, check if ground height change
				RaycastHit hit;
				if (Physics.Raycast(cameraEye.position, Vector3.down, out hit, 50f, finalLandingMask))
				{
					newGroundHeight = hit.point.y;
					Debug.DrawLine(cameraEye.position, hit.point);

					if (newGroundCollider != hit.collider)
					{
						newGroundCollider = hit.collider;
						Debug.Log ("hit something diff when walking: " + hit.collider.name);
						justFinishFlying = true;
						return;
					}

					player.position.Set (player.position.x, newGroundHeight, player.position.z);
				}
			}*/
			break;
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

			for(int i=0; i<particles.Length; i++)
			{
				var pos = controllers [i].transform.position;
				pos.y -= 0.156f * PlayerSize;
				particles [i].transform.position = pos;
				particles [i].transform.localScale *= 5f;
				var main = particles [i].main;
				main.loop = true;
				particles [i].Play ();
			}

			switch (flyType)
			{
			case FlyType.Physics:
				// v.1 controllers decide direction
				//Vector3 aveVec = (controllerTrans [0].forward + controllerTrans [1].forward) / 2f;

				// v.2 face decide direction
				Vector3 aveVec = cameraEye.forward;

				FlyVector = aveVec * FlyStep;
				playerMovement.FlyVector = FlyVector;
				playerMovement.IsFlying = true;
				break;
			}
		}
	}

	private void HandlePadUp(object sender, ClickedEventArgs e)
	{
		padDownCount--;
		//Debug.Log ("padDownCount: " + padDownCount);
		if(isFlying)
		{
			switch(flyType)
			{
			case FlyType.NonPhysics:
				justFinishFlying = true;

				// Raycast down to see if will landing on stickers
				RaycastHit hit;
				if (Physics.Raycast(Pivot.position, Vector3.down, out hit, 50f, finalLandingMask))
				{
					newGroundCollider = hit.collider;
					newGroundHeight = hit.point.y;
					Debug.Log ("hit something first time: " + hit.collider.name);

					Debug.DrawLine(Pivot.position, hit.point);
				}
				break;

			case FlyType.Physics:
				playerMovement.IsFlying = false;
				for(int i=0; i<toolHubs.Length; i++)
				{
					toolHubs [i].EnableAllTools ();
				}
				break;
			}

			for(int i=0; i<particles.Length; i++)
			{
				particles [i].transform.localPosition = particleOriPositions[i];
				particles [i].transform.localScale = Vector3.one;
				var main = particles [i].main;
				main.loop = false;
				particles [i].Stop ();
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
				//Vector3 aveVec = (controllerTrans[0].forward + controllerTrans[1].forward) / 2f;
				Vector3 aveVec = cameraEye.forward;

				FlyVector = aveVec * FlyStep;

				switch (flyType)
				{
				case FlyType.NonPhysics:
					player.Translate (FlyVector);
					break;

				case FlyType.Physics:
					playerMovement.FlyVector = FlyVector;
					break;
				}
			}
		}
	}

	public bool IsInBounds(Vector3 pos, Vector3 center)
	{
//		bool inRadius = (pos - center).sqrMagnitude < (flyDistance * flyDistance);
//		bool aboveGround = pos.y >= 0;
		return (pos - center).sqrMagnitude < (flyDistance * flyDistance) ||  pos.y >= 0;
	}
}
