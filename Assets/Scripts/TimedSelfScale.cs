using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedSelfScale : MonoBehaviour {

	public Transform cameraEye;
	public Transform player;

	public float[] targetScales = new float[] {0.05f, 1f, 4f};
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
			// do autoscale in order to match center
			LeanTween.scale( player.gameObject, Vector3.one, 3f );
			LeanTween.move( player.gameObject, Vector3.zero, 3f );
		}
		else
		{
			curr_targetScale = targetScales[level];
			shouldBeScaling = true;
		}
	}

	public void ScaleSelfTo(Transform target, float scaleSize)
	{
		// scale up
		float scaleFactor = 1f + 0.01f;// * PlayerScale;
		var endScale = target.transform.localScale * scaleFactor;

//		if (Mathf.Approximately (target.transform.localScale.x, endScale.x))
//			return;

		var pivot = cameraEye.transform.position;
		pivot.y = target.transform.position.y; // set pivot to be on the floor

		var diffP = target.transform.position - pivot;
		var finalPos = (diffP * scaleFactor) + pivot;

		target.transform.localScale = endScale;
		target.transform.position = finalPos;
	}
}
