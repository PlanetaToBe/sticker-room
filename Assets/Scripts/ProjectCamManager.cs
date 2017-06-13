using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectCamManager : MonoBehaviour {

	public Transform player;
    public Transform cameraEye;
    public LevelManager levelManager;
    public Renderer camMask;

	private Vector3 theOffset;
	private bool toFollow = false;
    private Vector3 velocity;

	void Start()
	{
		transform.localScale = player.localScale;
		theOffset = (transform.position - player.position) / transform.localScale.x;
	}

	void OnEnable()
	{
		levelManager.OnLevelStart += OnLevelStart;
        levelManager.OnEndStart += OnEndStart;
    }

	void OnDisable()
	{
		levelManager.OnLevelStart -= OnLevelStart;
        levelManager.OnEndStart -= OnEndStart;
    }

	void FixedUpdate()
	{
		if (toFollow)
		{
			Follow ();
		}
	}

	void OnLevelStart(int levelIndex)
	{
        if(levelIndex==0)
        {
            LeanTween.color(camMask.gameObject, Color.clear, 2f)
                .setOnComplete(()=> {
                    camMask.enabled = false;
                });
        }
		else if(levelIndex==1)
		{
			toFollow = true;
			Debug.Log ("toFollow = true");
		}
		else
		{
			Invoke ("Unfollow", 10.5f);

            LeanTween.rotateX(gameObject, 30f, 5f);
		}
	}

    void OnEndStart()
    {
        LeanTween.color(camMask.gameObject, Color.black, 3f)
               .setOnStart(() => {
                   camMask.enabled = true;
               });
    }

	void Follow()
	{
		transform.localScale = player.localScale;
        //transform.position = player.position + theOffset * transform.localScale.x;
        //transform.position = cameraEye.position + theOffset * transform.localScale.x;

        Vector3 targetPosition = cameraEye.position + theOffset * transform.localScale.x;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.5f);
    }

	void Unfollow()
	{
		//Follow ();
		toFollow = false;
		Debug.Log ("toFollow = false");
	}
}
