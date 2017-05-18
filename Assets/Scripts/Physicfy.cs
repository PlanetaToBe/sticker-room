using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Physicfy : MonoBehaviour {

	public GameObject stickersParent;
	public PhysicMaterial artPhyMat;

	private StickerControllerGenerator stickerGenerator;
	private int wallLayer;

	private VRInteractiveObject tmp_interactiveObject;
	private int tmp_c_index;

	public enum ShootType
	{
		SpringRightAway,
		SpringAfterHit
	}
	public ShootType shootType = ShootType.SpringAfterHit;
	public float shootForce = 15f;
	private GrabnStretch grabnstretch;
	public float RealShootForce
	{
		get { return shootForce * grabnstretch.PlayerScale;}
	}

	private RaycastHit springHit;

	void OnEnable()
	{
		if (stickerGenerator == null)
			stickerGenerator = GetComponent<StickerControllerGenerator> ();
		
		stickerGenerator.OnCreateSticker += ApplyPhysics;
		stickerGenerator.OnReleaseTrigger += RemoveFixedJoint;
	}

	void OnDisable()
	{
		stickerGenerator.OnCreateSticker -= ApplyPhysics;
		stickerGenerator.OnReleaseTrigger -= RemoveFixedJoint;
	}

	void Start()
	{
		wallLayer = 1 << 8;
		grabnstretch = GetComponent<GrabnStretch> ();
	}

	private void ApplyPhysics(GameObject sticker, uint c_index)
	{
		sticker.transform.SetParent (stickersParent.transform);
		//var s_c = sticker.transform.GetChild (0).gameObject;
		GameObject s_c;
		if (sticker.transform.childCount > 0)
			s_c = sticker.transform.GetChild (0).gameObject;
		else
			s_c = sticker;
			
		BoxCollider b_c = s_c.AddComponent<BoxCollider> ();

		tmp_interactiveObject = s_c.AddComponent<VRInteractiveObject> ();
		tmp_interactiveObject.usePhysics = true;
		tmp_interactiveObject.Mass = 2f * grabnstretch.PlayerScale;
		tmp_c_index = (int)c_index;

		// TODO: should it move to VRInteractiveObject??
		// add collider to sticker
		StartCoroutine(AdjustPhysics(b_c));

		ApplyFixedJoint ();

		// Find the connect anchor for SPRING_RIGHT_AWAY shooting
		switch(shootType)
		{
		case ShootType.SpringRightAway:
			b_c.material = artPhyMat;
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, 50f, wallLayer))
			{
				ApplySpring (hit);
			}
			break;
		}
	}

	void ApplyFixedJoint()
	{
		tmp_interactiveObject.AddJoint (GetComponent<GrabnStretch>().attachPoint);
	}

	void RemoveFixedJoint()
	{
		if (tmp_interactiveObject != null)
			tmp_interactiveObject.RemoveJoint ();
		else
			return;

		switch(shootType)
		{
		case ShootType.SpringAfterHit:
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, 50f, wallLayer))
			{
				AddForwardForce (hit);
			}
			break;
		}

		tmp_interactiveObject = null;
		tmp_c_index = -1;
	}

	void AddForwardForce(RaycastHit hit)
	{
		tmp_interactiveObject.Rigidbody.velocity = transform.forward*RealShootForce;
		tmp_interactiveObject.IsShooting = true;
	}

	void ApplySpring(RaycastHit hit)
	{		
		var anchor = hit.point;
		tmp_interactiveObject.AddSpringJoint (hit.rigidbody, anchor, 150f);
	}

	IEnumerator AdjustPhysics(BoxCollider b_collider)
	{
		yield return 0;
		b_collider.size = new Vector3 (b_collider.size.x, b_collider.size.y+0.0005f, b_collider.size.z);
	}

	IEnumerator ApplyForce(VRInteractiveObject _inter, uint c_index)
	{
		yield return 0;

		var device = GetDevice ((int)c_index);
		Debug.Log (device.velocity);
		_inter.Rigidbody.velocity = device.velocity;
		_inter.Rigidbody.angularVelocity = device.angularVelocity;
		_inter.Rigidbody.maxAngularVelocity = _inter.Rigidbody.angularVelocity.magnitude;
	}

	SteamVR_Controller.Device GetDevice(int _index)
	{
		return SteamVR_Controller.Input (_index);
	}
}
