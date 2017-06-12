using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grill : MonoBehaviour {

	public Tool fish;
	public StickerTool myTool;
	public Collider fire;
	private bool inUse;

	void OnEnable()
	{
		fish.OnCollideEnter += OnCollideEnter;
		fish.OnCollideExit += OnCollideExit;

		if(myTool!=null)
			myTool.OnChangeToolStatus += OnToolStatusChange;
	}

	void OnDisable()
	{
		fish.OnCollideEnter += OnCollideEnter;
		fish.OnCollideExit += OnCollideExit;

		if(myTool!=null)
			myTool.OnChangeToolStatus -= OnToolStatusChange;
	}

	public void OnCollideEnter(Collider _col)
	{
		if (!inUse)
			return;

		if(_col == fire)
		{
			// play particles
			// play sounds
			fish.OnDown();
		}
	}

	public void OnCollideExit(Collider _col)
	{
		if (!inUse)
			return;

		if(_col == fire)
		{
			// stop particles
			// stop sounds
			fish.OnUp();
		}
	}

	private void OnToolStatusChange(bool _inUse, int toolIndex)
	{
		inUse = _inUse;
	}
}