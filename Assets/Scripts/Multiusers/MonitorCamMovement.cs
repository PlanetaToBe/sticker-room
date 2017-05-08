using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonitorCamMovement : MonoBehaviour {

	public GameObject floor;
	public GameObject localPlayerBody;
	public GameObject startPoint;
	public bool toFollow = false;

	private Vector3 initialOffset;
	private float initialHeight;
	private Vector3 targetPosition;
	private Vector3 initialBodyOffset;

	private void Update()
	{
		if(toFollow)
		{
			FollowPlayer ();
		}
	}
	public void GetFixedDistance()
	{
		initialHeight = transform.localPosition.y - floor.transform.position.y;

		initialOffset = transform.position - startPoint.transform.position;
		//initialBodyOffset = startPoint.transform.position - startPoint.transform.localPosition;

		toFollow = true;
	}

	private void FollowPlayer()
	{
		targetPosition.Set (
			0f,
			floor.transform.position.y + initialHeight,
			localPlayerBody.transform.position.z + initialOffset.z
		);

		transform.localPosition = targetPosition;
	}
}
