using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grill : MonoBehaviour {

	public Renderer fishRenderer;
	public Tool fish;
	public StickerTool myTool;
	public Collider fire;
	public Color cookingFish;

	private bool inUse;
	private int tweenID = -1;

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

//			if (tweenID>0){
//				if(LeanTween.isTweening(tweenID))
//					LeanTween.cancel (tweenID);
//			}
//			tweenID = LeanTween.value( gameObject, UpdateFishColorCallback, Color.white, cookingFish, 1f).id;
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

//			if (tweenID>0){
//				if(LeanTween.isTweening(tweenID))
//					LeanTween.cancel (tweenID);
//			}
//			tweenID = LeanTween.value( gameObject, UpdateFishColorCallback, fishRenderer.material.color, Color.white, 1f).id;
		}
	}

	private void OnToolStatusChange(bool _inUse, int toolIndex)
	{
		inUse = _inUse;
	}

	void UpdateFishColorCallback(Color val)
	{
		fishRenderer.material.color = val;
	}
}