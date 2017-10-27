using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OoniCaptureCam
{
    public class CaptureCamUI : MonoBehaviour
    {
        public Image recordDot;
        public Text instructionsText;
        public float recordButtonBlinkTime = 1.0f;
        public GameObject exportInfoPrefab;

        private CaptureCam parentCam;
        private bool animatingRecordButton;
        private GameObject exportInfo;

        void Start()
        {
            parentCam = GetComponent<CaptureCam>();

            if (parentCam)
            {
                parentCam.ShowCameraAction += ShowInstructions;
                parentCam.StartCaptureAction += ShowRecordButton;
                parentCam.FinishCaptureAction += Reset;
                parentCam.StartEncodingAction += ShowExportInfo;
                parentCam.FinishEncodingAction += ShowExportComplete;
            }
        }

        void ShowInstructions()
        {
            HideRecordButton();
            instructionsText.CrossFadeAlpha(1.0f, .3f, true);
        }

        void HideInstructions()
        {
            instructionsText.CrossFadeAlpha(0f, .3f, true);
        }

        void ShowRecordButton()
        {
            HideInstructions();
            animatingRecordButton = true;
            StartCoroutine(AnimateRecordButton());
        }

        void HideRecordButton()
        {
            animatingRecordButton = false;
            recordDot.CrossFadeAlpha(0f, 0f, true);
        }

        private void Reset()
        {
            HideRecordButton();
            ShowInstructions();
        }

        IEnumerator AnimateRecordButton()
        {
            return AnimateRecordButton(true);
        }

        IEnumerator AnimateRecordButton(bool show)
        {
            if (!animatingRecordButton)
            {
                recordDot.CrossFadeAlpha(0f, 0f, true);
            }
            else
            {
                if (show)
                {
                    recordDot.CrossFadeAlpha(1f, 0f, true);
                }
                else
                {
                    recordDot.CrossFadeAlpha(0f, 0f, true);
                }

                yield return new WaitForSeconds(recordButtonBlinkTime);
                yield return AnimateRecordButton(!show);
            }
        }

        void ShowExportInfo(string videoPath)
        {
            exportInfo = Instantiate(exportInfoPrefab);
            exportInfo.transform.position = transform.position;
            exportInfo.transform.LookAt(SteamVR_Render.Top().gameObject.transform);
            exportInfo.transform.Rotate(0, 180, 0);
            exportInfo.GetComponentInChildren<Text>().text = "Saving video file to:\n\n" + videoPath;
        }

        void ShowExportComplete(string videoPath)
        {
            exportInfo.GetComponentInChildren<Text>().text = "Video file saved to\n\n" + videoPath;
            StartCoroutine(ShowExportComplete());
        }

        IEnumerator ShowExportComplete()
        {
            yield return new WaitForSeconds(5f);

            HideExportInfo();
        }

        void HideExportInfo()
        {
            Destroy(exportInfo);
        }
    }
}