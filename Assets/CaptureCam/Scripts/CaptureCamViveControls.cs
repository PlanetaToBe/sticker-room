using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace OoniCaptureCam
{
    public class CaptureCamViveControls : MonoBehaviour
    {
        SteamVR_ControllerManager controllerManager;
        GameObject currentTarget;

        private bool triggerPressed = false;

        void Start()
        {
            controllerManager = GameObject.FindObjectOfType<SteamVR_ControllerManager>();
        }

        void Update()
        {
            bool _triggerPressed = false;

            uint leftControllerIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            uint rightControllerIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);

            if (controllerManager.left && GetTriggerPressed(leftControllerIndex))
            {
                _triggerPressed = true;
                currentTarget = controllerManager.left;
            }

            if (controllerManager.right && GetTriggerPressed(rightControllerIndex))
            {
                _triggerPressed = true;
                currentTarget = controllerManager.right;
            }

            if (_triggerPressed && !triggerPressed)
            {
                OnTriggerDown();
            }

            triggerPressed = _triggerPressed;
        }

        void OnTriggerDown()
        {
            CaptureCam cam = GetComponent<CaptureCam>();
            cam.target = currentTarget;
            GetComponent<CaptureCam>().ToggleCapture();
        }

        bool GetTriggerPressed(uint controllerIndex)
        {
            if (controllerIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return false;
            else return SteamVR_Controller.Input((int)controllerIndex).GetPress(SteamVR_Controller.ButtonMask.Trigger);
        }
    }
}