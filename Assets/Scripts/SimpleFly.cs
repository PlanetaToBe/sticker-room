using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFly : MonoBehaviour {

    public ToolHub toolHub;
    public StickerTool tool;
    public Transform playerPosition;

    public AnimationCurve flySpeed;
    public float damping = .05f;

	public ParticleSystem particles;
	public SpriteRenderer tipRenderer;

    private bool toolActive = false;
    private bool isFlying = false;
	private bool tipActive = true;
    private float flyDuration = 0;
	private float stopDuration = 0;

    private bool isStopping = false;
	private bool particlesActive = false;

    private float velocity;
    private Vector3 direction;

    private SteamVR_TrackedController controller;

    void OnEnable()
    {
        if (controller == null)
        {
            controller = GetComponent<SteamVR_TrackedController>();
        }

        BindEvents();
    }

    void OnDisable()
    {
        UnbindEvents();
    }

    void BindEvents()
    {
        controller.TriggerClicked += HandleClick;
        controller.TriggerUnclicked += HandleUnclick;

        if (tool != null)
        {
            tool.OnChangeToolStatus += HandleToolStatusChange;
        }
    }

    void UnbindEvents()
    {
        controller.TriggerClicked -= HandleClick;
        controller.TriggerUnclicked -= HandleUnclick;

        if (tool != null)
        {
            tool.OnChangeToolStatus -= HandleToolStatusChange;
        }
    }

    void HandleClick(object o, ClickedEventArgs e)
    {
        StartFlying();
    }

    void HandleUnclick(object o, ClickedEventArgs e)
    {
        EndFlying();
    }

    void StartFlying()
    {
        isStopping = false;
        isFlying = true;
    }

    void EndFlying()
    {
        isFlying = false;
        flyDuration = 0;

        isStopping = true;
        stopDuration = 0;
    }

    private void FixedUpdate()
    {
        if (!toolActive) return;

        if (isFlying)
        {
			if (!particlesActive) {
				particles.Play ();
				particlesActive = true;
			}

			if (tipActive) {
				HideTip ();
			}

            flyDuration += Time.fixedDeltaTime;
            velocity += flySpeed.Evaluate(flyDuration) / 5000;
            direction = controller.transform.forward;
            playerPosition.position += velocity * direction;
        }

        if (isStopping)
        {
			if (particlesActive) {
				particles.Stop ();
				particlesActive = false;
			}

            velocity = velocity * (1 - damping);

            if (velocity < .00005)
            {
                isStopping = false;
            } else
            {
                playerPosition.position += velocity * direction;
            }
        }
    }

	private void HideTip() {
		LeanTween.value( gameObject, UpdateTipColorCallback, Color.white, Color.clear, 1f);
		tipActive = false;
	}
		
    private void HandleToolStatusChange(bool _isActive, int toolIndex)
    {
        toolActive = _isActive;

		if (toolActive) {
			ShowTip ();
		}

        if (!toolActive && isFlying)
        {
            EndFlying();
        }
    }

	private void ShowTip() {
		LeanTween.value( gameObject, UpdateTipColorCallback, Color.clear, Color.white, 1f);
	}

	private void UpdateTipColorCallback(Color val) {
		tipRenderer.color = val;
	}
}
