using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SpawnHyperlinks : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;

    [SerializeField] private GameObject hyperlinkOverlayPrefab;

    private ARTrackedImage currentlyTrackedARImage;

    // Create a dictionary to store the hyperlink url and spatial coordinates of each AR hyperlink overlay
    private List<OverlayData> currentOverlayInformation = new List<OverlayData>();
    private List<GameObject> currentOverlays = new List<GameObject>();

    private string lastDetectedQRCodeData;


    // Listen for the following events:
    //   1. QR code detected
    //   2. AR image marker tracking updates
    private void OnEnable() {
        m_TrackedImageManager.trackedImagesChanged += OnChanged;
        QRCodeDetector.QRCodeDetectedEvent += OnQRCodeDetected;
    }

    private void OnDisable() {
        m_TrackedImageManager.trackedImagesChanged -= OnChanged;
        QRCodeDetector.QRCodeDetectedEvent -= OnQRCodeDetected;
    }

    private void OnQRCodeDetected(string data)
    {
        Debug.Log("QR Code Detected: " + data);

        // If a detected QR code is different than the previously detected QR code, then
        // we should reset the OverlayData list, and destroy any Overlay game objects that
        // were previously spawned 
        if (data != lastDetectedQRCodeData) {

            // updated last detected QR code
            lastDetectedQRCodeData = data;

            string jsonString = data;

            OverlayManager overlayManager = new OverlayManager(); // Assuming you have an OverlayManager
            List<OverlayData> myOverlayData = overlayManager.CreateOverlayDataFromJson(jsonString);

            Debug.Log("Overlay details:");
            currentOverlayInformation = myOverlayData;

            // Destroy previously spawned Overlay game objects
            foreach (GameObject obj in currentOverlays)
            {
                // Check if the object is not null before attempting to destroy it
                if (obj != null)
                {
                    // Destroy the object
                    Destroy(obj);
                }
            }

            currentOverlays = new List<GameObject>();
        }
    }

    private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // TODO: Handle case when more than 1 AR marker image is visible (shouldn't happen in most cases)
        
        foreach (var newImage in eventArgs.added)
        {
            Debug.Log("Event: Image added!");
            currentlyTrackedARImage = newImage;
            if (currentOverlays.Count == 0 && currentOverlayInformation.Count > 0)
            {
                SpawnOverlays();
            }
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            Debug.Log("Event: Image updated!");
            if (currentOverlays.Count == 0 && currentOverlayInformation.Count > 0)
            {
                SpawnOverlays();
            }
        }

        foreach (var removedImage in eventArgs.removed)
        {
            Debug.Log("Event: Image removed!");
            currentlyTrackedARImage = null;
        }
    }

    private void SpawnOverlays() {
        Debug.Log("Spawning overlays...");
        foreach (var overlayData in currentOverlayInformation)
        {
            // Debug.Log()
            GameObject hyperlinkOverlay = Instantiate(hyperlinkOverlayPrefab, currentlyTrackedARImage.transform);
            Vector3 offset = overlayData.offset;
            Vector3 scale = overlayData.scale;
            hyperlinkOverlay.transform.localPosition = offset;
            hyperlinkOverlay.transform.localScale = scale;
            currentOverlays.Add(hyperlinkOverlay);
        }
    }

    // The below methods are sometimes useful for debugging.
    // void ListAllImages()
    // {
    //     Debug.Log(
    //         $"There are {m_TrackedImageManager.trackables.count} images being tracked.");

    //     foreach (var trackedImage in m_TrackedImageManager.trackables)
    //     {
    //         Debug.Log($"Image: {trackedImage.referenceImage.name} is at " +
    //                   $"{trackedImage.transform.position}");
    //     }
    // }


    // void Update() {
        // ListAllImages();
    // }
}