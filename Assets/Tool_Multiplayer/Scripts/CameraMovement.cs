using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	GvrHead gvrHead;
	public GameObject body;

	void Start () {
		StartCoroutine (LateStart());
	}

	protected IEnumerator LateStart()
	{
		yield return new WaitWhile (()=> GetComponent<GvrHead>() == null);
		Debug.Log ("Get gvr head!");
		gvrHead = GetComponent<GvrHead> ();
		gvrHead.trackPosition = true;
		gvrHead.target = body.transform;
	}
}
