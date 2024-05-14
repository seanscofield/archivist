using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class SpawnHyperlinks : MonoBehaviour
{
    [SerializeField] public ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] public GameObject hyperlinkOverlayPrefab;
    public GameObject image;
    public Image imageDisplay; // Reference to the UI Image object to display the image

    private ARTrackedImage currentlyTrackedARImage;
    private string url = "https://raw.githubusercontent.com/seanscofield/archivist/main/Assets/CustomAssets";

    // List to store the hyperlink url and spatial coordinates of each AR hyperlink overlay
    private List<OverlayData> currentOverlayInformation = new List<OverlayData>();
    private List<GameObject> currentOverlays = new List<GameObject>();

    private string lastDetectedQRCodeData;
    private string lastDetectedQRCodeId = "";
    private int lastDetectedQRCodePageNum = -1;

    // Listen for QR code detection events & AR image marker tracking events
    private void OnEnable() {
        m_TrackedImageManager.trackedImagesChanged += OnChanged;
        QRCodeDetector.QRCodeDetectedEvent += OnQRCodeDetected;
    }

    private void OnDisable() {
        m_TrackedImageManager.trackedImagesChanged -= OnChanged;
        QRCodeDetector.QRCodeDetectedEvent -= OnQRCodeDetected;
    }

    [System.Serializable]
    public class QRData
    {
        public string id;
        public int page;
    }

    private void OnQRCodeDetected(string data)
    {
        // If a detected QR code is different than the previously detected QR code, then we
        // should reset the OverlayData list, and destroy any previously spawned Overlay game objects
        if (data != lastDetectedQRCodeData)
        {
            // TODO: Don't update `lastDetectedQRCodeData` if JSON not decoded properly

            // updated last detected QR code
            lastDetectedQRCodeData = data;

            // Parse JSON string to extract ID and page number
            QRData qrData = JsonUtility.FromJson<QRData>(data);
            string id = qrData.id;
            int page = qrData.page;

            if (id != lastDetectedQRCodeId || page != lastDetectedQRCodePageNum) {
                StartCoroutine(HyperlinkDataFetcher.FetchJSONFromId(id, page, processHyperlinkData));
            }
        }
    }

    private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // TODO: Handle case when more than 1 AR marker image is visible (shouldn't happen in most cases)
        
        foreach (var newImage in eventArgs.added)
        {
            currentlyTrackedARImage = newImage;
            
            if (currentOverlays.Count == 0 && currentOverlayInformation.Count > 0)
            {
                SpawnOverlays();
            }

            image.transform.SetParent(newImage.transform, true);
            imageDisplay.transform.SetParent(newImage.transform, true);
            image.transform.localPosition = new Vector3(0, 0, 0);
            imageDisplay.transform.localPosition = new Vector3(0, 0, 0);
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            if (currentOverlays.Count == 0 && currentOverlayInformation.Count > 0)
            {
                SpawnOverlays();
            }
        }

        foreach (var removedImage in eventArgs.removed)
        {
            currentlyTrackedARImage = null;
        }
    }

    private void SpawnOverlays() {
        foreach (var overlayData in currentOverlayInformation)
        {
            GameObject hyperlinkOverlay = Instantiate(hyperlinkOverlayPrefab, currentlyTrackedARImage.transform);
            
            // Set the URL metadata
            URLMetadata urlHolder = hyperlinkOverlay.GetComponent<URLMetadata>();
            if (urlHolder != null)
            {
                urlHolder.url = overlayData.url;
            }
            Vector3 offset = overlayData.offset;
            Vector3 scale = overlayData.scale;
            hyperlinkOverlay.transform.localPosition = offset;
            hyperlinkOverlay.transform.localScale = scale;
            currentOverlays.Add(hyperlinkOverlay);
        }
    }

    private void processHyperlinkData(string id, int pageNum, ARData arData) {
        currentOverlayInformation = new List<OverlayData>();
        float[] markerCoords = arData.ar_marker_coordinates;

        // Iterate over the pages
        for (int i = 0; i < arData.pages.Count; i++)
        {
            // Check if the current page matches the specified page number
            if (i == pageNum)
            {
                Page page = arData.pages[i];

                foreach (Hyperlink hyperlink in page.hyperlinks)
                {
                    float[] hyperlinkCoords = hyperlink.coordinates;

                    float[] scale = CoordinateConverter.CalculateHyperlinkScale(markerCoords[0], markerCoords[1], markerCoords[2], markerCoords[3],
                                                                            hyperlinkCoords[0], hyperlinkCoords[1], hyperlinkCoords[2], hyperlinkCoords[3]);
                    float[] offset = CoordinateConverter.CalculateHyperlinkOffset(markerCoords[0], markerCoords[1], markerCoords[2], markerCoords[3],
                                                                            hyperlinkCoords[0], hyperlinkCoords[1], hyperlinkCoords[2], hyperlinkCoords[3]);

                    Debug.Log($"Hyperlink URI: {hyperlink.uri}");
                    Debug.Log($"Scale - X: {scale[0]}, Z: {scale[1]}");
                    Debug.Log($"Offset - X: {offset[0]}, Z: {offset[1]}");

                    Vector3 scaleVector3 = new Vector3(scale[0], 0.001f, scale[1]);
                    Vector3 offsetVector3 = new Vector3(offset[0], 0.0f, offset[1]);
                    OverlayData newOverlayData = new OverlayData("<random-id>", hyperlink.uri, scaleVector3, offsetVector3);
                    currentOverlayInformation.Add(newOverlayData);
                }
            }
        }

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

        lastDetectedQRCodeId = id;
        lastDetectedQRCodePageNum = pageNum;
    }
}