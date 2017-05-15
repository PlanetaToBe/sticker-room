using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Physicfy : MonoBehaviour {

	public GameObject stickersParent;

	private StickerControllerGenerator stickerGenerator;
	private int wallLayer;

	private VRInteractiveObject tmp_interactiveObject;
	private int tmp_c_index;

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
	}

	private void ApplyPhysics(GameObject sticker, uint c_index)
	{
		sticker.transform.SetParent (stickersParent.transform);
		var s_c = sticker.transform.GetChild (0).gameObject;
		BoxCollider b_c = s_c.AddComponent<BoxCollider> ();

		tmp_interactiveObject = s_c.AddComponent<VRInteractiveObject> ();
		tmp_interactiveObject.usePhysics = true;
		tmp_c_index = (int)c_index;

		// TODO: should it move to VRInteractiveObject??
		// add collider to sticker
		StartCoroutine(AdjustPhysics(b_c));

		ApplyFixedJoint ();
	}

	void ApplyFixedJoint()
	{
		tmp_interactiveObject.AddJoint (GetComponent<ViveSimpleController>().attachPoint);
	}

	void RemoveFixedJoint()
	{
		tmp_interactiveObject.RemoveJoint ();

		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.forward, out hit, 50f, wallLayer))
		{
			// v.1
			//ApplySpring (hit);

			// v.2
			AddForwardForce (hit);
		}

		tmp_interactiveObject = null;
		tmp_c_index = -1;
	}

	void AddForwardForce(RaycastHit hit)
	{
		tmp_interactiveObject.Rigidbody.velocity = transform.forward*10f;
		tmp_interactiveObject.IsShooting = true;
	}

	void ApplySpring(RaycastHit hit)
	{		
		var tmpAnchor = hit.point - hit.rigidbody.position;
		var anchor = new Vector3 (tmpAnchor.y / hit.transform.localScale.x, 0f, tmpAnchor.z / hit.transform.localScale.z);
		//Debug.Log (hit.point);
		//Debug.Log (hit.rigidbody.position);
		//Debug.Log (anchor);
		tmp_interactiveObject.AddSpringJoint (hit.rigidbody, anchor);
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
