using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelZero : MonoBehaviour {

	public LevelManager levelManager;
	public int levelIndex = 0;

	[Header("Level Objects")]
	public GameObject mask;
	public CanvasGroup startInfo;
	public CanvasGroup machineInfo;

	void OnEnable()
	{
		levelManager.OnLevelStart += OnLevelStart;
		levelManager.OnLevelEnd += OnLevelEnd;
	}

	void OnDisable()
	{
		levelManager.OnLevelStart -= OnLevelStart;
		levelManager.OnLevelEnd -= OnLevelEnd;
	}

	void OnLevelStart(int _level)
	{
		if (_level == levelIndex)
		{
			LeanTween.color(mask, Color.clear, 1f).setOnComplete(()=>{
				mask.SetActive(false);
			});

			LeanTween.value(gameObject, 1f, 0f, 1f)
				.setOnUpdate((float val)=>{
					startInfo.alpha = val;
				})
				.setOnComplete(()=>{
					LeanTween.value(gameObject, 0f, 1f, 1f)
						.setOnUpdate((float val)=>{
							machineInfo.alpha = val;
						});
				});
		}
	}

	void OnLevelEnd(int _level)
	{
		if (_level == levelIndex)
		{

		}
	}
}
