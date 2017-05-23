using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Glove : Tool {

	private Animator animator;

	void Start()
	{
		animator = GetComponent<Animator> ();
	}

	public void OnDown()
	{
		animator.SetTrigger ("Down");
	}

	public void OnUp()
	{
		animator.SetTrigger ("Up");
	}
}
