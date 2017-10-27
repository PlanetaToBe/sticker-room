using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OoniCaptureCam
{
    public class CaptureCamAutoTester : MonoBehaviour
    {
        public int testDuration = 10;

        IEnumerator Start()
        {
            while (Application.isPlaying)
            {
                yield return new WaitForSeconds(1f);
                gameObject.GetComponent<CaptureCam>().StartCapture();
                yield return new WaitForSeconds(testDuration);
                gameObject.GetComponent<CaptureCam>().FinishCapture();
            }
        }
    }
}
