using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Physicfy.
/// 1. Spring Right Away
/// 	=> Trigger down to generate => Add fixed joint + spring joint => Trigger up to remove fixed joint
/// 	=> After 3 sec of hitting wall, remove Rigidbody and spring joint
/// 2. Spring After Hit (Hosing)
/// 	=> Trigger to generate => Out with forward force => Hit wall or floor
/// </summary>
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
	public ShootType shootType = ShootType.SpringAfterHit;	// as hosing, snap if hit the wall, stack up if hit the floor

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

	private void ApplyPhysics (GameObject sticker, uint c_index)
	{
		// Replacing stickers
		sticker.transform.SetParent (stickersParent.transform);

		GameObject s_c;
		if (sticker.transform.childCount > 0)
			s_c = sticker.transform.GetChild (0).gameObject;
		else
			s_c = sticker;
			
		BoxCollider b_c = s_c.AddComponent<BoxCollider> ();

		Rigidbody r_b = s_c.AddComponent<Rigidbody> ();
		r_b.mass = 2f * grabnstretch.PlayerScale;

		tmp_interactiveObject = s_c.AddComponent<VRInteractiveObject> ();
		tmp_interactiveObject.usePhysics = true;
		tmp_c_index = (int) c_index;

		// adjust collider thickness
		StartCoroutine (AdjustPhysics(b_c));

		// Find the connect anchor for SPRING_RIGHT_AWAY shooting
		RaycastHit hit;
		b_c.material = artPhyMat;

		switch(shootType)
		{
		case ShootType.SpringRightAway:

			ApplyFixedJoint ();

			if (Physics.Raycast(transform.position, transform.forward, out hit, 50f, wallLayer))
			{
				tmp_interactiveObject.AddSpringJoint (hit.rigidbody, hit.point, 150f);
			}
			break;
		
		case ShootType.SpringAfterHit:
			//Add Forward Force
			tmp_interactiveObject.TheRigidbody.velocity = transform.forward * RealShootForce;
			tmp_interactiveObject.IsShooting = true;
			break;
		}
	}

	void ApplyFixedJoint()
	{
		tmp_interactiveObject.AddJoint (GetComponent<GrabnStretch>().attachPoint);
	}

	void RemoveFixedJoint()
	{
		if (tmp_interactiveObject == null)
			return;

		switch(shootType)
		{
		case ShootType.SpringRightAway:
			tmp_interactiveObject.RemoveJoint ();
			break;
		}
		tmp_interactiveObject = null;
		tmp_c_index = -1;
	}

	IEnumerator AdjustPhysics(BoxCollider b_collider)
	{
		// wait for one frame to get updated collider
		yield return 0;

		// TODO: y*1.01f
		b_collider.size = new Vector3 (b_collider.size.x, b_collider.size.y + 0.0005f, b_collider.size.z);
	}

	SteamVR_Controller.Device GetDevice(int _index)
	{
		return SteamVR_Controller.Input (_index);
	}
}
