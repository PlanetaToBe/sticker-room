using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyManager : MonoBehaviour {

	public AzureSkyController azureSky;
	public float currentTime;

	public float[] levelTimes;
	public float transitionTime = 1f;

	private List<int> runningTweenIds = new List<int>();

	void Start()
	{
		azureSky.AzureSetTime (currentTime, 0f);
		currentTime = levelTimes [0];
		SetFloor (0);
	}

	void OnDisable()
	{
		ClearRunningTweens();
	}

	public void SetFloor(int floorIndex)
	{
		ClearRunningTweens();

		UpdateSkyTime(floorIndex);
	}

	public void UpdateSkyTime(int level)
	{
		if (currentTime == levelTimes [level])
			return;
		
		LTDescr tween = LeanTween.value(transform.gameObject, currentTime, levelTimes[level], transitionTime);
		tween.setOnUpdate((float val) => {
			azureSky.AzureSetTime (val, 0f);
		});

		runningTweenIds.Add(tween.id);
		currentTime = levelTimes [level];
	}

	private void ClearRunningTweens()
	{
		if (runningTweenIds.Count == 0)
			return;

		foreach (int tweenId in runningTweenIds)
		{
			LeanTween.cancel(tweenId);
		}

		runningTweenIds.Clear();
	}
}
