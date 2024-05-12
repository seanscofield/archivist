using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class SpawnHyperlinks : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;

    [SerializeField] private GameObject hyperlinkOverlayPrefab;

    private string url = "https://raw.githubusercontent.com/seanscofield/archivist/main/Assets/CustomAssets";
    // private string url = "https://raw.githubusercontent.com/seanscofield/archivist_data";

    private ARTrackedImage currentlyTrackedARImage;

    // Create a dictionary to store the hyperlink url and spatial coordinates of each AR hyperlink overlay
    private List<OverlayData> currentOverlayInformation = new List<OverlayData>();
    private List<GameObject> currentOverlays = new List<GameObject>();

    private string lastDetectedQRCodeData;
    private string lastDetectedQRCodeId = "";
    private int lastDetectedQRCodePageNum = -1;


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

    [System.Serializable]
    public class QRData
    {
        public string id;
        public int page;
    }

    private void OnQRCodeDetected(string data)
    {
        Debug.Log("QR Code Detected: " + data);

        // If a detected QR code is different than the previously detected QR code, then
        // we should reset the OverlayData list, and destroy any Overlay game objects that
        // were previously spawned 
        if (data != lastDetectedQRCodeData)
        {
            // TODO: Don't update `lastDetectedQRCodeData` if JSON not decoded properly

            // updated last detected QR code
            lastDetectedQRCodeData = data;

            // Parse JSON string to extract URL and page number
            QRData qrData = JsonUtility.FromJson<QRData>(data);

            string id = qrData.id;
            int page = qrData.page;

            if (id != lastDetectedQRCodeId || page != lastDetectedQRCodePageNum) {
                StartCoroutine(FetchJSONFromId(id, page));
            }
        }
    }

    private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // TODO: Handle case when more than 1 AR marker image is visible (shouldn't happen in most cases)
        
        foreach (var newImage in eventArgs.added)
        {
            // Debug.Log("Event: Image added!");
            currentlyTrackedARImage = newImage;
            if (currentOverlays.Count == 0 && currentOverlayInformation.Count > 0)
            {
                SpawnOverlays();
            }
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            // Debug.Log("Event: Image updated!");
            if (currentOverlays.Count == 0 && currentOverlayInformation.Count > 0)
            {
                SpawnOverlays();
            }
        }

        foreach (var removedImage in eventArgs.removed)
        {
            // Debug.Log("Event: Image removed!");
            currentlyTrackedARImage = null;
        }
    }

    private void SpawnOverlays() {
        Debug.Log("Spawning overlays...");
        foreach (var overlayData in currentOverlayInformation)
        {
            // Debug.Log()
            GameObject hyperlinkOverlay = Instantiate(hyperlinkOverlayPrefab, currentlyTrackedARImage.transform);
            
            // Set the URL metadata
            Metadata urlHolder = hyperlinkOverlay.GetComponent<Metadata>();
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

    IEnumerator FetchJSONFromId(string id, int pageNum)
    {
        string fullUrl = $"{url}/{id}.json"; // Combine url variable with id to form the full URL

        Debug.Log("Full url: " + fullUrl);

        using (UnityWebRequest www = UnityWebRequest.Get(fullUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error fetching JSON from URL: {www.error}");
            }
            else {
                currentOverlayInformation = new List<OverlayData>();

                ARData jsonData = JsonUtility.FromJson<ARData>(www.downloadHandler.text);

                float[] markerCoords = jsonData.ar_marker_coordinates;

                // Iterate over the pages
                for (int i = 0; i < jsonData.pages.Count; i++)
                {
                    // Check if the current page matches the specified page number
                    if (i == pageNum)
                    {
                        Page page = jsonData.pages[i];

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
                            OverlayData newOverlayData = new OverlayData(scaleVector3, offsetVector3, hyperlink.uri, "<random-id>");
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