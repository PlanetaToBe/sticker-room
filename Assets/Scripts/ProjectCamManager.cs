using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectCamManager : MonoBehaviour {

	public Transform player;

	private Vector3 initialOffset;


	void Start()
	{
		transform.localScale = player.localScale;
		initialOffset = (transform.position - player.position) * transform.localScale.x;

	}

	void Update()
	{
		
	}
}
