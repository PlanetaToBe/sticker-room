using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlySingle : MonoBehaviour {

	public enum FlyType
	{
		NonPhysics,
		Physics
	}
	public FlyType flyType = FlyType.NonPhysics;

	public float flySpeed = 0.2f;
	public float flyDistance = 2f;
	public ToolHub toolHub;
	public FlySingle otherController;

	private SteamVR_TrackedController controller;
	private int padDownCount = 0;
	private Transform controllerTran;
	private bool isFlying = false;
	private bool justFinishFlying = false;

	private Transform player;
	private Vector3 roomCenter = new Vector3 (0, 1.7f, 0);

	private float newGroundHeight = 0f;
	private float normalGroundHeight = 0f;
	private Collider newGroundCollider;
	private int wallLayer;
	private int stickerLayer;
	private int finalLandingMask;
	public int FlyMask
	{
		get { return finalLandingMask; }
	}
	private PlayerMovement playerMovement;

	[Header("Raycast Reference")]
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

	[Header("Effect")]
	public ParticleSystem particle;
	private Vector3 particleOriPosition;

	private bool m_inFlyingMode = false;
	private bool m_inFlyingSupportMode = false;
	public bool InFlyingSupportMode
	{
		get { return m_inFlyingSupportMode; }
	}
	public bool InFlyingMode
	{
		get { return m_inFlyingMode; }
	}

	public StickerTool myTool;
	public Tool rocket;
	private bool inUse;

	private Vector3 fallVel;
	private int velTweenID;
	private int relativeToGround = 1;

	void OnEnable()
	{
		if (controller == null)
		{
			controller = GetComponent<SteamVR_TrackedController> ();
		}

		controller.TriggerClicked += HandleDown;
		controller.TriggerUnclicked += HandleUp;
		controller.TriggerDowning += HandleTouching;

		if(myTool!=null)
			myTool.OnChangeToolStatus += OnToolStatusChange;
	}

	void OnDisable()
	{
		controller.TriggerClicked -= HandleDown;
		controller.TriggerUnclicked -= HandleUp;
		controller.TriggerDowning -= HandleTouching;

		if(myTool!=null)
			myTool.OnChangeToolStatus -= OnToolStatusChange;
	}

	private void OnToolStatusChange(bool _inUse, int toolIndex)
	{
		inUse = _inUse;

		if (!inUse)
			Reset ();
	}

	void Start()
	{
		controllerTran = controller.transform;
		player = transform.parent.parent;

		wallLayer = 1 << 8;
		stickerLayer = 1 << 11;
		int thingLayer = 1 << 9;
		finalLandingMask = wallLayer | stickerLayer | thingLayer;

		playerMovement = GetComponentInParent<PlayerMovement> ();
		particleOriPosition = particle.transform.localPosition;
	}

	void Update()
	{
		switch(flyType)
		{
		case FlyType.NonPhysics:
			if (justFinishFlying)
			{
				// v.1 Land on Stickers
				/*
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
				}
				*/

				// v.2 Land on Floor
				// above ground
				if(relativeToGround==1 && player.position.y > normalGroundHeight - fallVel.y)
				{
					player.Translate (fallVel);
				}
				// below ground
				else if(relativeToGround==-1 && player.position.y < normalGroundHeight - fallVel.y)
				{
					player.Translate (fallVel);
				}
				else
				{
					Debug.Log ("land on floor!");
					player.position = new Vector3 (player.position.x, normalGroundHeight, player.position.z);
					justFinishFlying = false;
					LeanTween.cancel (velTweenID);
				}
			}
			break;
		}
	}

	private void HandleDown(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;
		
		if(otherController.InFlyingMode)
		{
			m_inFlyingSupportMode = true;
			m_inFlyingMode = false;

			if(!particle.isPlaying)
			{
				particle.Play ();
			}
		}
		else
		{
			m_inFlyingSupportMode = false;
			m_inFlyingMode = true;
		}

		if(m_inFlyingMode && !isFlying)
		{
			// disable all tool functions
			toolHub.DisableAllTools ();
			isFlying = true;
			particle.Play ();

			switch (flyType)
			{
			case FlyType.Physics:
				// v.1 controllers decide direction
				Vector3 aveVec;
				if (otherController.InFlyingSupportMode)
					aveVec = (controllerTran.forward + otherController.transform.forward) / 2f;
				else
					aveVec = controllerTran.forward;

				FlyVector = aveVec * FlyStep;
				playerMovement.FlyVector = FlyVector;
				playerMovement.IsFlying = true;
				break;
			}
		}
	}

	private void HandleUp(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;
		
		m_inFlyingSupportMode = false;
		m_inFlyingMode = false;

		if(isFlying)
		{
			switch(flyType)
			{
			case FlyType.NonPhysics:
				justFinishFlying = true;
				fallVel = Vector3.zero;

				if (player.position.y < normalGroundHeight)
					relativeToGround = -1;
				else
					relativeToGround = 1;
				
				velTweenID = LeanTween.value (gameObject, Vector3.zero, new Vector3 (0f, -0.1f * relativeToGround, 0f), 3f)
					.setEaseInExpo()
					.setOnUpdate((Vector3 val)=>{
						fallVel = val;
					}).id;

				// Raycast down to see if will landing on stickers
				RaycastHit hit;
				if (Physics.Raycast(Pivot.position, Vector3.down, out hit, 50f, finalLandingMask))
				{
					newGroundCollider = hit.collider;
					newGroundHeight = hit.point.y;
					Debug.Log ("hit something first time: " + hit.collider.name);

					Debug.DrawLine(Pivot.position, hit.point);
				}
				toolHub.EnableAllTools ();
				break;

			case FlyType.Physics:
				playerMovement.IsFlying = false;
				toolHub.EnableAllTools ();
				break;
			}
			//particle.Stop ();
		}
		if(particle.isPlaying)
		{
			particle.Stop ();
		}
		isFlying = false;
	}

	private void HandleTouching(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;
		
		if(isFlying)
		{			
			if (IsInBounds (player.position, roomCenter))
			{
				Vector3 aveVec;
				if (otherController.InFlyingSupportMode)
					aveVec = (controllerTran.forward + otherController.transform.forward) / 2f;
				else
					aveVec = controllerTran.forward;

				if (otherController.InFlyingSupportMode)
					FlyVector = aveVec * FlyStep * 3f;
				else
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

	void Reset()
	{
		m_inFlyingSupportMode = false;
		m_inFlyingMode = false;

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
				toolHub.EnableAllTools ();
				break;
			}

			particle.Stop ();
		}
		isFlying = false;
	}
}
