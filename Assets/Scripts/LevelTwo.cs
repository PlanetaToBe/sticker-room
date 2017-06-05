using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTwo : MonoBehaviour {

	public LevelManager levelManager;
	public int levelIndex = 2;

	[Header("Level Objects")]
	public GameObject mask;
	public GameObject[] thingsToShrink;

	[Header("Light")]
	public Light houseLight;
	public float minFlickerSpeed = 0.5f;
	public float maxFlickerSpeed = 1f;
	public float minIntensity = 0.4f;
	public float maxIntensity = 1f;
	private IEnumerator flickerCoroutine;
	private bool doLightEffect = false;

	[Header("Audio")]
	public GvrAudioSource pop;
	public GvrAudioSource party;
	public AudioClip[] popClips;

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

	void Start()
	{
		//flickerCoroutine = FlickerLight ();
	}

	void OnLevelTransition(int _level)
	{
		if (_level == levelIndex)
		{
			// shrink forrest
			ShrinkStuff();

			// play party sound
			ToggleAudio(party, true, 1f);

			// flicker light
			flickerCoroutine = FlickerLight ();
			StartCoroutine(flickerCoroutine);
		}
	}

	void OnLevelStart(int _level)
	{
		if (_level == levelIndex)
		{
			//
		}
	}

	void OnLevelEnd(int _level)
	{
		if (_level == levelIndex)
		{
			ToggleAudio(party, false, 0f);
		}
	}

	IEnumerator FlickerLight()
	{
		while (doLightEffect)
		{
			houseLight.enabled = true;
			houseLight.intensity = Random.Range(minIntensity, maxIntensity);
			yield return new WaitForSeconds (Random.Range(minFlickerSpeed, maxFlickerSpeed));
		}
	}

	void ShrinkStuff()
	{
		for(int i=0; i<thingsToShrink.Length; i++)
		{
			LeanTween.scale (thingsToShrink [i], Vector3.one/100f, 2f)
				.setEaseInOutBack ()
				.setDelay (i / 2f)
				.setOnStart (()=>{
					pop.PlayOneShot(popClips[Random.Range(0, popClips.Length)]);
				});
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
