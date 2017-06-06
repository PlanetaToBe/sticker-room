using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOne : MonoBehaviour {

	public LevelManager levelManager;
	public int levelIndex = 1;

	[Header("Flythrough")]
	public FlyPathFinder flypathFinder;
	public GameObject player;

	void OnEnable()
	{
		levelManager.OnLevelTransition += OnLevelTransition;
		levelManager.OnLevelStart += OnLevelStart;
		levelManager.OnLevelEnd += OnLevelEnd;
	}

	void OnDisable()
	{
		levelManager.OnLevelTransition -= OnLevelTransition;
		levelManager.OnLevelStart -= OnLevelStart;
		levelManager.OnLevelEnd -= OnLevelEnd;
	}

	void OnLevelTransition(int _level)
	{
		if (_level == levelIndex)
		{
			flypathFinder.PauseAutoFly ();
		}
	}

	void OnLevelStart(int _level)
	{
		if (_level == levelIndex)
		{
			flypathFinder.DoAutoFly (player, 170f);
		}
	}

	void OnLevelEnd(int _level)
	{
		if (_level == levelIndex)
		{
			flypathFinder.CancelAutoFly ();
		}
	}

	void ToggleAudio(GvrAudioSource audioSource, bool turnOn, float volume)
	{
		if (turnOn)
		{
			audioSource.UnPause();

			if (!audioSource.isPlaying)
				audioSource.Play();

			// volume up!
			LeanTween.value(audioSource.gameObject, 0f, volume, 1f)
				.setOnUpdate((float val)=>{
					audioSource.volume = val;
				});
		}
		else
		{
			if (audioSource.isPlaying)
			{
				LeanTween.value(audioSource.gameObject, audioSource.volume, 0f, 1f)
					.setOnUpdate((float val)=>{
						audioSource.volume = val;
					})
					.setOnComplete(()=>{
						//a_source.Pause();
						audioSource.Stop();
					});
			}
		}
	}
}
