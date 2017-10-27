using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OoniCaptureCam
{
    public class CaptureCamKeyControls : MonoBehaviour
    {
        private string toggleKeyName = "space";

        void Update()
        {
            if (Input.GetKeyDown(toggleKeyName))
            {
                gameObject.GetComponent<CaptureCam>().ToggleCapture();
            }
        }
    }
}
