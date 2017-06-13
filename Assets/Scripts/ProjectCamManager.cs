using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectCamManager : MonoBehaviour {

	public Transform player;
	public LevelManager levelManager;

	private Vector3 theOffset;
	private bool toFollow;

	void Start()
	{
		transform.localScale = player.localScale;
		theOffset = (transform.position - player.position) / transform.localScale.x;
	}

	void OnEnable()
	{
		levelManager.OnLevelStart += OnLevelStart;
	}

	void OnDisable()
	{
		levelManager.OnLevelStart -= OnLevelStart;
	}

	void FixedUpdate()
	{
		if (toFollow)
		{
			Follow ();
		}
	}

	void OnLevelStart(int levelIndex)
	{
		if(levelIndex==1)
		{
			toFollow = true;
			Debug.Log ("toFollow = true");
		}
		else
		{
			Invoke ("Unfollow", 10.5f);
		}
	}

	void Follow()
	{
		transform.localScale = player.localScale;
		transform.position = player.position + theOffset * transform.localScale.x;
	}

	void Unfollow()
	{
		//Follow ();
		toFollow = false;
		Debug.Log ("toFollow = false");
	}
}
