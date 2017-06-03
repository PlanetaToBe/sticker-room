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
			// flickering the light, StartCoroutin
			doLightEffect = true;
			minIntensity = 0.4f;
			minFlickerSpeed = 0.5f;
			maxFlickerSpeed = 1f;
			StartCoroutine (flickerCoroutine);

			// lift the house
			Invoke("FlickerIntense", 5f);

			// lift the house
			Invoke("LiftHouse", 9f);
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

	IEnumerator FlickerLight()
	{
		while (doLightEffect)
		{
			houseLight.enabled = true;
			houseLight.intensity = Random.Range(minIntensity, maxIntensity);
			yield return new WaitForSeconds (Random.Range(minFlickerSpeed, maxFlickerSpeed));
			if (realFlicker)
			{
				houseLight.enabled = false;
				yield return new WaitForSeconds (Random.Range(minFlickerSpeed, maxFlickerSpeed));
			}
		}
	}

	void LiftHouse()
	{
		for(int i=0; i<thingsToBeLift.Length; i++)
		{
			LeanTween.moveLocalY (thingsToBeLift [i], thingsToBeLift [i].transform.localPosition.y + .5f, 6f)
				//.setEaseInOutBack ()
				.setOnStart (()=>{
					doLightEffect = false;
					houseLight.enabled = false;
				});
		}
	}

	void FlickerIntense()
	{
		minIntensity = 0f;
		minFlickerSpeed = 0.01f;
		maxFlickerSpeed = 0.1f;
		realFlicker = true;
	}
}
