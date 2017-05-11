using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickerBrush : MonoBehaviour {

    private bool isPainting = false;

    void Start() {
        SteamVR_TrackedController controller = GetComponent<SteamVR_TrackedController>();
        controller.TriggerClicked += StartPainting;
        controller.TriggerUnclicked += StopPainting;
    }

    void StartPainting(object sender, ClickedEventArgs e)
    {
        isPainting = true;
        StartCoroutine(Paint());
    }

    IEnumerator Paint()
    {
        if (isPainting)
        {
            GameObject sticker = Instantiate(StickerSceneManager.instance.stickerPrefab, transform.position, transform.rotation) as GameObject;
            yield return new WaitForSeconds(1f / StickerSceneManager.instance.paintSpeed);
            yield return Paint();
        } else
        {
            yield return 0;
        }
    }

    void StopPainting(object sender, ClickedEventArgs e)
    {
        isPainting = false;
    }
}
