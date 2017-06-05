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
	public CanvasGroup artistInfo;
	public Light houseLight;
	public GameObject[] thingsToBeLift;

	[Header("Light")]
	public float minFlickerSpeed = 0.01f;
	public float maxFlickerSpeed = 0.1f;
	public float minIntensity = 0f;
	public float maxIntensity = 1f;
	private IEnumerator flickerCoroutine;
	private bool doLightEffect = false;
	private bool realFlicker = false;
	private RainbowLight rainbowLight;

	[Header("Audio")]
	public GvrAudioSource noise;
	public GvrAudioSource elect;
	public AudioClip electClip;
	public GvrAudioSource shatter;

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
		flickerCoroutine = FlickerLight (10f);
		rainbowLight = houseLight.GetComponent<RainbowLight> ();
	}

	void OnLevelTransition(int _level)
	{
		if (_level == levelIndex)
		{
			rainbowLight.StartRainbowLight ();

			// flickering the light, StartCoroutin
			doLightEffect = true;
			minIntensity = 0.4f;
			minFlickerSpeed = 0.5f;
			maxFlickerSpeed = 1f;
			StartCoroutine (flickerCoroutine);

			// increase intensity
			Invoke("FlickerIntense", 15f);

			// lift the house
			Invoke("LiftHouse", 19f);
		}
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
			//
		}
	}

	IEnumerator FlickerLight(float delayT)
	{
		yield return new WaitForSeconds (delayT);

		// play noise audio
		ToggleAudio(noise, true, 1f);

		int electCount = 0;

		while (doLightEffect)
		{
			houseLight.enabled = true;
			houseLight.intensity = Random.Range(minIntensity, maxIntensity);
			yield return new WaitForSeconds (Random.Range(minFlickerSpeed, maxFlickerSpeed));
			if (realFlicker)
			{
				houseLight.enabled = false;
				yield return new WaitForSeconds (Random.Range(minFlickerSpeed, maxFlickerSpeed));

				// elect sound
				if(electCount%5==0)
				{
					elect.PlayOneShot (electClip, 1f);
				}
				electCount++;
			}
		}
	}

	void LiftHouse()
	{
		for(int i=0; i<thingsToBeLift.Length; i++)
		{
			LeanTween.moveLocalY (thingsToBeLift [i], thingsToBeLift [i].transform.localPosition.y + .5f, 6f)
				.setEaseInOutQuad ()
				.setOnStart (()=>{
					doLightEffect = false;
					houseLight.enabled = false;
					shatter.Play();
					ToggleAudio(noise, false, 0f);
					ToggleAudio(elect, false, 0f);
				})
				.setOnComplete(()=>{
					noise.Stop();
					ToggleAudio(shatter, false, 0f);
				});
		}
	}

	void FlickerIntense()
	{
		minIntensity = 0f;
		minFlickerSpeed = 0.01f;
		maxFlickerSpeed = 0.1f;
		realFlicker = true;

		LeanTween.value (gameObject, 1f, 0f, 1f)
			.setOnUpdate ((float val) => {
				artistInfo.alpha = val;
			});
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
