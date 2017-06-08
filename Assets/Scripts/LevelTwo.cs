using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTwo : MonoBehaviour {

	public LevelManager levelManager;
	public int levelIndex = 2;

	[Header("Level Objects")]
	public GameObject mask;
	public GameObject[] thingsToShrink;
	public GameObject[] parentsToShrink;

	[Header("Light")]
	public Light houseLight;
	public float minFlickerSpeed = 0.01f;
	public float maxFlickerSpeed = 0.1f;
	public float minIntensity = 0f;
	public float maxIntensity = 1f;
	private IEnumerator flickerCoroutine;
	private bool doLightEffect = false;

	[Header("Audio")]
	public GvrAudioSource pop;
	public GvrAudioSource party;
	public AudioClip[] popClips;
	public GvrAudioSource poof;

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
		flickerCoroutine = FlickerLight ();
	}

	void OnLevelTransition(int _level)
	{
		if (_level == levelIndex)
		{
			// shrink forrest
			//ShrinkStuff();

			ShrinkChildren ();
		}
	}

	void OnLevelStart(int _level)
	{
		if (_level == levelIndex)
		{
			// play party sound
			//ToggleAudio(party, true, 1f);

			// fire light
			houseLight.range = 50f;
			houseLight.gameObject.SetActive (true);
			houseLight.enabled = true;
			doLightEffect = true;
			StartCoroutine(flickerCoroutine);
		}
	}

	void OnLevelEnd(int _level)
	{
		if (_level == levelIndex)
		{
			ToggleAudio(party, false, 0f);
			doLightEffect = false;
			houseLight.enabled = true;
			poof.Play ();
			LeanTween.value(houseLight.gameObject, houseLight.intensity, 0f, 2f)
				.setOnUpdate((float val)=>{
					houseLight.intensity = val;
				})
				.setOnComplete(()=>{
					houseLight.enabled = false;
				});
		}
	}

	IEnumerator FlickerLight()
	{
		while (doLightEffect)
		{
			//houseLight.enabled = true;
			houseLight.intensity = Random.Range(minIntensity, maxIntensity);
			yield return new WaitForSeconds (Random.Range(minFlickerSpeed, maxFlickerSpeed));
			//houseLight.enabled = false;
			//yield return new WaitForSeconds (Random.Range(minFlickerSpeed, maxFlickerSpeed));
		}
	}

	void ShrinkStuff()
	{
		for(int i=0; i<thingsToShrink.Length; i++)
		{
			LeanTween.scale (thingsToShrink [i], Vector3.one/10f, 2f)
				.setEaseInOutBack ()
				.setDelay (i / 2f)
				.setOnStart (()=>{
					pop.PlayOneShot(popClips[Random.Range(0, popClips.Length)]);
				})
				.setOnComplete(()=>{
					thingsToShrink [i].SetActive(false);
				});
		}
	}

	void ShrinkChildren()
	{
		for(int i=0; i<parentsToShrink.Length; i++)
		{
			for(int j=0; j<parentsToShrink[i].transform.childCount; j++)
			{
				GameObject child = parentsToShrink [i].transform.GetChild (j).gameObject;
				LeanTween.scale (child, Vector3.one/10f, 1f)
					.setEaseInOutBack ()
					.setDelay (Random.Range(0, 4f))
					.setOnStart (()=>{
						pop.PlayOneShot(popClips[0]);
					})
					.setOnComplete(()=>{
						child.SetActive(false);
					});
			}
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
