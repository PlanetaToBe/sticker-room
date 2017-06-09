using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFly : MonoBehaviour {

    public ToolHub toolHub;
    public StickerTool tool;
    public Transform playerPosition;

    public AnimationCurve flySpeed;
    public float damping = .05f;

    private bool toolActive = false;
    private bool isFlying = false;
    private float flyDuration = 0;

    private bool isStopping = false;

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
            flyDuration += Time.fixedDeltaTime;
            velocity += flySpeed.Evaluate(flyDuration) / 5000;
            direction = controller.transform.forward;
            playerPosition.position += velocity * direction;
        }

        if (isStopping)
        {
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

    private void HandleToolStatusChange(bool _isActive, int toolIndex)
    {
        toolActive = _isActive;

        if (!toolActive && isFlying)
        {
            EndFlying();
        }
    }
}
