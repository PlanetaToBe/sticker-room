using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LineStickerBrush : MonoBehaviour {

    public float stickerSpacing = .1f;
    public float stickerScale = .1f;
    public float stickerPadding = .02f;

    private bool isPainting = false;
    private Vector3 lastStickerPosition;
    private Quaternion lastStickerRotation;
    private SteamVR_TrackedController controller;
    private StickerData nextStickerData;

    void Start() {
		controller = GetComponent<SteamVR_TrackedController>();
        controller.TriggerClicked += StartPainting;
        controller.TriggerUnclicked += StopPainting;

        nextStickerData = StickerSceneManager.instance.GetRandomSticker();
    }

    void StartPainting(object sender, ClickedEventArgs e)
    {
        isPainting = true;
        // StartCoroutine(Paint());
    }

    void StopPainting(object sender, ClickedEventArgs e)
    {
        isPainting = false;
        lastStickerPosition = new Vector3(100, 100, 100);
    }

    private void Update()
    {
        if (isPainting && lastStickerPosition == null)
        {
            AddSticker();
        } else if (isPainting)
        {
            float nextStickerWidth = stickerScale * nextStickerData.width / nextStickerData.height;
            Vector3 offset = transform.position - lastStickerPosition;
            if (offset.magnitude > stickerSpacing) AddSticker();
        }
    }

    private void AddSticker()
    {

        Vector3 offset = lastStickerPosition - transform.position;
        Quaternion rot = Quaternion.Euler(0, 90, 0);
        Quaternion stickerTargetRotation = Quaternion.FromToRotation(Vector3.right, offset);
        Quaternion stickerRotation = stickerTargetRotation;

        GameObject sticker = Instantiate(StickerSceneManager.instance.stickerPrefab, transform.position, stickerRotation) as GameObject;
        Sticker newSticker = sticker.GetComponentInChildren<Sticker>();
        newSticker.stickerId = nextStickerData.id;

        sticker.transform.localScale = new Vector3(stickerScale, stickerScale, stickerScale);

        float stickerWidth = stickerScale * nextStickerData.width / nextStickerData.height;

        lastStickerRotation = stickerRotation;
        lastStickerPosition = transform.position;
        nextStickerData = StickerSceneManager.instance.GetRandomSticker();
    }
}
