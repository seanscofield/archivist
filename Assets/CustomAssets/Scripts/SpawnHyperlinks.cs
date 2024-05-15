using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

/* This script listens for notifications about newly detected QR codes
 * and newly tracked AR markers, and spawns AR hyperlink "overlays"
 * on top of the hyperlinks within a document based on the data it
 * retrieves from the dynamic QR code.
 */
public class SpawnHyperlinks : MonoBehaviour
{
    [SerializeField] public ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] public GameObject internalHyperlinkOverlayPrefab;
    [SerializeField] public GameObject externalHyperlinkOverlayPrefab;
    [SerializeField] public GameObject imageHyperlinkOverlayPrefab;
    [SerializeField] public GameObject legendPrefab;

    private ARTrackedImage currentlyTrackedARImage;

    // List to store the hyperlink url and spatial coordinates of each AR hyperlink overlay
    private List<OverlayData> currentOverlayInformation = new List<OverlayData>();
    private List<GameObject> currentOverlays = new List<GameObject>();

    private string lastDetectedQRCodeData;
    private string lastDetectedQRCodeId = "";
    private int lastDetectedQRCodePageNum = -1;

    // Listen for QR code detection events & AR image marker tracking events
    private void OnEnable() {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        QRCodeDetector.QRCodeDetectedEvent += OnQRCodeDetected;
    }

    private void OnDisable() {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        QRCodeDetector.QRCodeDetectedEvent -= OnQRCodeDetected;
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

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // TODO: Handle case when more than 1 AR marker image is visible (shouldn't happen in most cases)
        foreach (var newImage in eventArgs.added)
        {
            currentlyTrackedARImage = newImage;
            if (currentOverlays.Count == 0 && currentOverlayInformation.Count > 0)
            {
                SpawnOverlays();
            }
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

    private void processHyperlinkData(string id, int pageNum, ARDocumentData arDocumentData) {
        currentOverlayInformation = new List<OverlayData>();
        float[] markerCoords = arDocumentData.ar_marker_coordinates;

        // Iterate over the pages
        for (int i = 0; i < arDocumentData.pages.Count; i++)
        {
            // Check if the current page matches the specified page number
            if (i == pageNum)
            {
                Page page = arDocumentData.pages[i];

                foreach (Hyperlink hyperlink in page.hyperlinks)
                {
                    float[] hyperlinkCoords = hyperlink.coordinates;

                    float[] scale = CoordinateConverter.CalculateHyperlinkScale(markerCoords[0], markerCoords[1], markerCoords[2], markerCoords[3],
                                                                            hyperlinkCoords[0], hyperlinkCoords[1], hyperlinkCoords[2], hyperlinkCoords[3]);
                    float[] offset = CoordinateConverter.CalculateHyperlinkOffset(markerCoords[0], markerCoords[1], markerCoords[2], markerCoords[3],
                                                                            hyperlinkCoords[0], hyperlinkCoords[1], hyperlinkCoords[2], hyperlinkCoords[3]);

                    Vector3 scaleVector3 = new Vector3(scale[0], 0.001f, scale[1]);
                    Vector3 offsetVector3 = new Vector3(offset[0], 0.0f, offset[1]);
                    OverlayData newOverlayData = new OverlayData("Random ID", hyperlink.uri, scaleVector3, offsetVector3);
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

    private void SpawnOverlays() {
        foreach (var overlayData in currentOverlayInformation)
        {
            string url = overlayData.url;
            Vector3 offset = overlayData.offset;
            Vector3 scale = overlayData.scale;

            GameObject hyperlinkOverlay;
            if (URLAnalyzer.HasImageExtension(url)) {
                hyperlinkOverlay = Instantiate(imageHyperlinkOverlayPrefab, currentlyTrackedARImage.transform);
            } else if (URLAnalyzer.HaveSameRootDomain(url, "photographylife.com")) {
                hyperlinkOverlay = Instantiate(internalHyperlinkOverlayPrefab, currentlyTrackedARImage.transform);
            } else {
                hyperlinkOverlay = Instantiate(externalHyperlinkOverlayPrefab, currentlyTrackedARImage.transform);
            }

            hyperlinkOverlay.transform.localPosition = offset;
            hyperlinkOverlay.transform.localScale = scale;
            currentOverlays.Add(hyperlinkOverlay);

            // Set the URL metadata on the newly created overlay
            URLMetadata urlHolder = hyperlinkOverlay.GetComponent<URLMetadata>();
            urlHolder.url = url;

            GameObject legendOverlay = Instantiate(legendPrefab, currentlyTrackedARImage.transform);
            legendOverlay.transform.localPosition = new Vector3(-0.11f, 0, 0);
            legendOverlay.transform.localScale = currentlyTrackedARImage.transform.localScale;
            legendOverlay.transform.Rotate(90f, 0f, 0f, Space.Self);
        }
    }
}