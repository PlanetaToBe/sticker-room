using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedSelfScale : MonoBehaviour {

	public Transform cameraEye;
	public Transform player;
	public float[] targetScales = new float[] {0.1f, 1f, 20f};
	public GvrAudioSource growSound;

	private float curr_targetScale;
	private bool shouldBeScaling = false;
	private LevelManager levelManager;

	void OnEnable()
	{
		if (levelManager == null)
			levelManager = GetComponent<LevelManager> ();

		levelManager.OnLevelStart += SetTargetScale;
	}

	void OnDisable()
	{
		levelManager.OnLevelStart -= SetTargetScale;
	}

	void Update()
	{
		if (!shouldBeScaling)
			return;
		
		if (player.transform.localScale.x < curr_targetScale)
		{
			ScaleSelfTo (player, curr_targetScale);
			growSound.gameObject.transform.position = cameraEye.position;
		}
		else
		{
			shouldBeScaling = false;
		}
	}

	public void SetTargetScale(int level)
	{
		// delay scaling to happen after seeing the event
		StartCoroutine(DoScale(level, 0f));
	}

	private IEnumerator DoScale(int level, float delayTime)
	{
		yield return new WaitForSeconds(delayTime);

		if(level==1)
		{
//			growSound.Play ();

//			// do autoscale in order to match center
//			LeanTween.scale( player.gameObject, Vector3.one/2f, 5f ).setEaseInOutQuad();
//			LeanTween.move( player.gameObject, Vector3.zero, 5f )
//				.setEaseInOutQuad()
//				.setOnUpdate((float val)=>{
//					growSound.gameObject.transform.position = cameraEye.position;
//				});

//			curr_targetScale = 0.1f;
//			shouldBeScaling = true;

			//Invoke ("ScaleToOne", 60f);
		}
		else if (level !=0)
		{
			//v.1
			//curr_targetScale = targetScales[level];
			//shouldBeScaling = true;

			//v.2
			TweenScaleSelfTo(player, targetScales[level]);

			growSound.Play ();
		}
	}

	void ScaleToOne()
	{
		growSound.Play ();
		curr_targetScale = 1f;
		shouldBeScaling = true;
	}

	public void ScaleSelfTo(Transform target, float scaleSize)
	{
		// scale up
		float scaleFactor = 1f + 0.01f;// * PlayerScale;
		var endScale = target.transform.localScale * scaleFactor;

		var pivot = cameraEye.transform.position;
		pivot.y = target.transform.position.y; // set pivot to be on the floor

		var diffP = target.transform.position - pivot;
		var finalPos = (diffP * scaleFactor) + pivot;

		target.transform.localScale = endScale;
		target.transform.position = finalPos;
	}

	public void TweenScaleSelfTo(Transform target, float scaleSize)
	{
		Vector3 oldScale = target.localScale;
		LeanTween.scale( target.gameObject, Vector3.one*scaleSize, 10f )
			.setEaseInOutQuad()
			.setOnUpdateVector3((Vector3 scale)=>{
				float scaleFactor = scale.x / oldScale.x;

				var pivot = cameraEye.transform.position;
				pivot.y = target.transform.position.y;
				var diffP = target.transform.position - pivot;
				var finalPos = (diffP * scaleFactor) + pivot;
				target.transform.position = finalPos;
				//Debug.Log("player scale " + player.localScale.x + ", scale " + scale.x + ", oldScale " + oldScale.x);
				oldScale = target.localScale;
			});
	}
}
