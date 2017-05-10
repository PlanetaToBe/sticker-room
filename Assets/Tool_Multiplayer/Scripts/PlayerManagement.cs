using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerManagement : MonoBehaviour {

	[Header("Player Attritubes")]
	public Color color;
	public int whoIam;
	public string username = "Anonymous";
	[Header("Object Assignment")]
	public GameObject player;
	public GameObject playerHead;
	public GameObject playerBody;
	public GameObject eyeCamera; 	// GVR
	public GameObject ViveRig;	// Vive
	public GameObject ViveCam;	// Vive
	[HideInInspector]
	public SocketManagement socketManagement;
	[HideInInspector]
	public GameObject nameTag;
	public string type = "player";
	public float startTime = 0f;

	private string vrType;
	private bool isLocalPlayer = false;
	private BodyManagement bodyMgmt;
	private Vector3 realPosition;
	private Vector3 realTagPosition;
	private Quaternion realRotation;
	private Quaternion realBillboardRotation;

	private Vector3 restartPosition = new Vector3(0,40f,0);
	private bool isFalling;
	//private PropManagement propManagement;
	private GameObject floor;
	//private ViveControllers viveControllers;

	public Rigidbody p_rigidbody;
	public SphereCollider p_collider;
	//public FloorManager floorManager;
	public bool iAmInER = false;

	void FixedUpdate()
	{
		if (isLocalPlayer)
		{
			if (p_collider)
			{
				// -->solve collide on pivot but FUCK_UP vive vertical movement
				//p_collider.center = playerBody.transform.localPosition;

				// -->instead, use vive's camera rig's height for target
				Vector3 col_targetPos = playerBody.transform.localPosition;
				col_targetPos.y = ViveRig.transform.localPosition.y;
				p_collider.center = col_targetPos;

				/*
				if (player.transform.position.y < -40f)
				{
					ResetPosition ();
				}*/
			}
		}
		else
		{
			/*
		player.transform.position = Vector3.Lerp (player.transform.position, realPosition, 0.1f);
		playerHead.transform.rotation = Quaternion.Lerp (playerHead.transform.rotation, realRotation, 0.1f);
		playerBody.transform.rotation = Quaternion.Lerp (playerBody.transform.rotation, realBillboardRotation, 0.1f);

		nameTag.transform.position = Vector3.Lerp (nameTag.transform.position, realTagPosition, 0.1f);
		nameTag.transform.localRotation = Quaternion.Lerp (nameTag.transform.localRotation, realBillboardRotation, 0.1f);
		*/
			player.transform.position = Vector3.Lerp (player.transform.position, realPosition, 0.1f);

			playerHead.transform.rotation = Quaternion.Lerp (playerHead.transform.rotation, realRotation, 0.1f);
			playerBody.transform.rotation = Quaternion.Lerp (playerBody.transform.rotation, realBillboardRotation, 0.1f);

			nameTag.transform.position = Vector3.Lerp (nameTag.transform.position, realTagPosition, 0.1f);
			nameTag.transform.localRotation = Quaternion.Lerp (nameTag.transform.localRotation, realBillboardRotation, 0.1f);
		}
	}

	public void InitPlayer(int _index, string _name)
	{
		color = new Color ();

		whoIam = _index;
		username = _name;
		nameTag.name = username + " name tag";
		nameTag.GetComponent<Text> ().text = username;
	}
		
	public void OnStartLocalPlayer()
	{
		isLocalPlayer = true;
		playerHead.SetActive (false);	// no need for hear cuz can't see self
		bodyMgmt = player.GetComponent<BodyManagement> ();
		bodyMgmt.enabled = true;		// so to send transformation to Server
		bodyMgmt.socketManagement = socketManagement;
		bodyMgmt.body = playerBody;
		bodyMgmt.eyeCam = eyeCamera;
		bodyMgmt.viveCam = ViveCam;
		bodyMgmt.nameTag = nameTag;

		//////
		/// 
		///
		/*
		propManagement = GameObject.Find ("PropManager").GetComponent<PropManagement>();
		floor = GameObject.Find ("Floor");
		floorManager = floor.GetComponent<FloorManager> ();
		floorManager.AssignStuff(this);
		transform.position = propManagement.startPoint.transform.position;
		*/
		//////
		/// 

		startTime = Time.time;

		if (socketManagement.isViveVR)
		{
			ViveRig.SetActive(true);
			//viveControllers = ViveRig.GetComponent<ViveControllers> ();
			//viveControllers.socketManagement = socketManagement;
			//viveControllers.playerManagement = this;
			vrType = "vive";
		} 
		else
		{
			//Vector3 startPosition = new Vector3 (Random.value*10f, 0f, Random.value*10f);
			//transform.position = startPosition;

			eyeCamera.SetActive (true);
			GvrViewer.Create ();
			vrType = "gvr";
		}

	}

	public void UpdateTrans(
		string type, float posX, float posY, float posZ,
		float rotX, float rotY, float rotZ, float rotW
	)
	{
		realPosition.Set (posX, posY, posZ);
		realRotation.Set (rotX, rotY, rotZ, rotW);

		if (type == "three")
		{
			realPosition.z *= -1;
			realRotation = Quaternion.Euler (Vector3.up * -180f);
			realRotation *= new Quaternion (rotX, -rotY, -rotZ, rotW);
			/*
			// Head
			// if(playerHead.activeSelf) // doesn't need to check cuz socket.io's broadcast doesn't send to self
			playerHead.transform.rotation = calRotation;

			// Body
			calRotation.x = 0;
			calRotation.z = 0;
			// need to normalize quaternion?
			playerBody.transform.rotation = calRotation;
			*/
		} 
		else
		{
			if (type == "vive")
				realPosition.y -= 2f;
			/*
			// Head
			// if(playerHead.activeSelf)
			playerHead.transform.rotation = calRotation;

			// Body
			calRotation.x = 0;
			calRotation.z = 0;
			playerBody.transform.rotation = calRotation;
			*/
		}

		//v.1 no interpolation
		/*
		player.transform.position = calPosition;
		playerHead.transform.rotation = calRotation;
		calRotation.x = 0;
		calRotation.z = 0;
		playerBody.transform.rotation = calRotation;
		*/

		//v.2 interpolation --> needs to be in Update()
		/*
		player.transform.position = Vector3.Lerp (player.transform.position, realPosition, 0.1f);
		playerHead.transform.rotation = Quaternion.Lerp (playerHead.transform.rotation, realRotation, 0.1f);
		realRotation.x = 0;
		realRotation.z = 0;
		playerBody.transform.rotation = Quaternion.Lerp (playerBody.transform.rotation, realRotation, 0.1f);
		*/
		realBillboardRotation.Set(0f, realRotation.y, 0f, realRotation.w);

		if (nameTag)
		{
			//realPosition.y += 1.8f;
			// v.1 no interpolation
			/*
			nameTag.transform.position = calPosition;
			nameTag.transform.localRotation = calRotation;
			*/
			// v.2 --> needs to be in Update()
			/*
			nameTag.transform.position = Vector3.Lerp (nameTag.transform.position, realPosition, 0.1f);
			nameTag.transform.localRotation = Quaternion.Lerp (nameTag.transform.localRotation, realRotation, 0.1f);
			*/
			realTagPosition.Set (realPosition.x, realPosition.y+1.8f, realPosition.z);
		}
	}

	public void Reset()
	{
		FadeOut ();		// take 1 sec
		// reposition
		Invoke ("Relocation", 2f);
	}

	private void ToRestartPosition()
	{
		p_rigidbody.velocity = Vector3.zero;
		p_rigidbody.angularVelocity = Vector3.zero;

		// need but hide cuz not using propManagement
		//player.transform.position = propManagement.startPoint.transform.position;

		FadeIn ();
	}

	private void ToER()
	{
		p_rigidbody.velocity = Vector3.zero;
		p_rigidbody.angularVelocity = Vector3.zero;

		// need but hide cuz not using propManagement
		//player.transform.position = propManagement.erPoint.transform.position;

		FadeIn ();

		/*
		if(floorManager.scoreManager.lifeCount==0)
			floorManager.scoreManager.ResetLife ();
		
		floorManager.scoreManager.WriteInfo ("The Ball is resting in emergency room...");
		*/
		iAmInER = true;
		// TODO: DISABLE controllers' ER function!!

		// stay for 5 sec then back to path again
		Invoke("Reset", 5f);
	}

	private void ToERTmp()
	{
		p_rigidbody.velocity = Vector3.zero;
		p_rigidbody.angularVelocity = Vector3.zero;

		// need but hide cuz not using propManagement
		//player.transform.position = propManagement.erPoint.transform.position;

		FadeIn ();

		/*
		if(floorManager.scoreManager.lifeCount==0)
			floorManager.scoreManager.ResetLife ();

		floorManager.scoreManager.WriteInfo ("The Ball is resting in emergency room...");
		*/

		iAmInER = true;
		// TODO: DISABLE controllers' ER function!!
	}

	private void FadeOut()
	{
		if (vrType == "vive")
		{
			SteamVR_Fade.Start (Color.black, 1f);
		}
	}

	private void Relocation()
	{
		/*
		floorManager.scoreManager.WriteInfo ("");

		// if no life left, sent to emergency room
		if(floorManager.scoreManager.lifeCount==0)
		{
			ToER();
		}
		else
		{
			// if still has life, reset position
			ToRestartPosition();
		}
		*/
	}

	private void FadeIn()
	{
		if (vrType == "vive")
		{
			SteamVR_Fade.Start (Color.clear, 3f);
		}
	}

	public void TakeMeToER()
	{
		FadeOut ();		// take 1 sec
		// reposition
		Invoke ("ToERTmp", 2f);
	}

	public void TakeMeBack()
	{
		//floorManager.scoreManager.WriteInfo ("");

		FadeOut ();		// take 1 sec
		// reposition
		Invoke ("ToRestartPosition", 2f);
	}

}
