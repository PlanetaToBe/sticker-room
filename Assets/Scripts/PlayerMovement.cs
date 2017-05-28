using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

	public Transform cameraRig;
	public Transform cameraEye;
	public float speedFactor = 1f;

	private Rigidbody p_rigidbody;
	public Rigidbody TheRigidbody
	{
		get { return p_rigidbody; }
	}
	private SphereCollider p_collider;
	public bool isFalling = false;
	public bool m_isFlying = false;
	public bool IsFlying
	{
		get { return m_isFlying; }
		set { 
			m_isFlying = value;
			if (m_isFlying)
			{
				p_rigidbody.isKinematic = true;
			}
			else
			{
				p_rigidbody.isKinematic = false;
				isFalling = true;

				// set collider to trigger type
				p_collider.isTrigger = true;

				// raycast down to found the landing collider
				// check triggerEnter if == landingCollider
				// if yes, set collider back to non-trigger type
				CheckForLandingCollider ();
			}
		}
	}
	private Fly flyScript;
	private Vector3 m_flyVector;
	public Vector3 FlyVector
	{
		get { return m_flyVector; }
		set { m_flyVector = value; }
	}
	public Vector3 RaycastPoint
	{
		get {
			var r_p = cameraEye.position;
			r_p.y = cameraRig.position.y;
			return r_p;
		}
	}

	private Collider landingTargetCollider;

	void Start()
	{
		p_rigidbody = GetComponent<Rigidbody> ();
		p_collider = GetComponent<SphereCollider> ();
		flyScript = GetComponentInChildren<Fly> ();
	}

//	void OnCollisionEnter(Collision _col)
//	{
////		Debug.Log ("OnCollisionsEnter");
//	}
//	void OnCollisionStay(Collision _col)
//	{
////		Debug.Log ("OnCollisionsStay");
//	}

	void OnCollisionExit(Collision _col)
	{		
		if(_col.gameObject.CompareTag("StickerFloor"))
		{
			_col.gameObject.tag = "Sticker";
			_col.gameObject.layer = 11; // Sticker
			Debug.Log ("On Collision Exit Sticker");

			// check for next collider
			CheckForLandingCollider();
			isFalling = true;
		}
	}

	void OnTriggerEnter(Collider _col)
	{
		if(isFalling && _col==landingTargetCollider)
		{
			p_collider.isTrigger = false;
			isFalling = false;
			Debug.Log ("landing!!");
		}
	}
		
	void FixedUpdate()
	{
		if (p_collider)
		{
			// adjust drag based on size
			p_rigidbody.drag = SuperLerp(transform.localScale.x, 0.05f, 4f, 50f, 0.1f);
				
			if(IsFlying)
			{
				p_rigidbody.MovePosition (p_rigidbody.transform.position + FlyVector*speedFactor);
			}

			// -->solve collide on pivot but FUCK_UP vive vertical movement
			//p_collider.center = cameraEye.localPosition;

			// -->instead, use vive's camera rig's height for target
			Vector3 col_targetPos = cameraEye.localPosition;
			col_targetPos.y = cameraRig.localPosition.y + p_collider.radius;
			p_collider.center = col_targetPos;
		}
	}

	private void FadeOut()
	{
		SteamVR_Fade.Start (Color.black, 1f);
	}

	private void FadeIn()
	{
		SteamVR_Fade.Start (Color.clear, 3f);
	}

	// source: https://forum.unity3d.com/threads/mapping-or-scaling-values-to-a-new-range.180090/
	public float SuperLerp (float x, float in_min, float in_max, float out_min, float out_max) {
		if (x <= in_min)
			return out_min;
		else if (x >= in_max)
			return out_max;
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}

	void CheckForLandingCollider()
	{
		RaycastHit hit;
		if (Physics.Raycast(RaycastPoint, Vector3.down, out hit, 50f, flyScript.FlyMask))
		{
			landingTargetCollider = hit.collider;
			Debug.Log ("will land on : " + landingTargetCollider.name);
			if(hit.collider.gameObject.CompareTag("Sticker"))
			{
				hit.collider.gameObject.tag = "StickerFloor";
				hit.collider.gameObject.layer = 9; // Thing
			}
		}
	}
}
