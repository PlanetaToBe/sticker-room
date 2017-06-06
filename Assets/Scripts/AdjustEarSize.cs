using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustEarSize : MonoBehaviour {

	public Transform ears;
	private Transform player;

	void Start()
	{
		player = transform.parent;
	}

	void Update()
	{
		ears.localScale = Vector3.one / player.localScale.x;
	}
}
