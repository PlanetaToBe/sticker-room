using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOne : MonoBehaviour {

	public LevelManager levelManager;
	public int levelIndex = 1;
	public EventTrigger ceilingTrigger;
	private bool moveTriggered;

	public GvrAudioSource partyMusic;
	public GvrAudioRoom partyRoom;

	[Header("Flythrough")]
	public bool enableAutoFly = false;
	public FlyPathFinder flypathFinder;
	public GameObject player;
	public float flyDuration = 120f;

	public Collider ceilingCollider;

	void OnEnable()
	{
		levelManager.OnLevelTransition += OnLevelTransition;
		levelManager.OnLevelStart += OnLevelStart;
		levelManager.OnLevelEnd += OnLevelEnd;
		ceilingTrigger.OnTrigger += MoveToNextLevel;
	}

	void OnDisable()
	{
		levelManager.OnLevelTransition -= OnLevelTransition;
		levelManager.OnLevelStart -= OnLevelStart;
		levelManager.OnLevelEnd -= OnLevelEnd;
		ceilingTrigger.OnTrigger -= MoveToNextLevel;
	}

	void MoveToNextLevel() {
		if (!moveTriggered) {
			moveTriggered = true;
			Debug.Log ("Next level");
			levelManager.MoveToNextLevel ();
		}
	}

	void OnLevelTransition(int _level)
	{
		if (_level == levelIndex)
		{
			if(enableAutoFly)
				flypathFinder.PauseAutoFly ();
		}
	}

	void OnLevelStart(int _level)
	{
		if (_level == levelIndex)
		{
			if(enableAutoFly)
				flypathFinder.DoAutoFly (player, flyDuration);

			ToggleAudio (partyMusic, true, 1f);
		}
	}

	void OnLevelEnd(int _level)
	{
		if (_level == levelIndex)
		{
			if(enableAutoFly)
				flypathFinder.CancelAutoFly ();

			partyRoom.enabled = false;
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
