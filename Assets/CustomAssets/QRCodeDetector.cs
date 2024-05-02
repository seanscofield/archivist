using System;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using ZXing;
using ZXing.Common;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;

// This script is used to asynchronously detect QR code contents from
// an AR Camera. It runs on a configurable interval (default once per second),
// and stores the most recently detected QR code contents in a public var
// so that other scripts can access it.
public class QRCodeDetector : MonoBehaviour
{
    public ARCameraManager m_CameraManager;

    public float acquisitionCooldown = 5.0f; // Cooldown period in seconds
    private float lastAcquisitionTime = 0.0f;

    private BarcodeReader barcodeReader = new BarcodeReader();

    public string lastDetectedQRCodeData { get; private set; }

    public delegate void QRCodeDetectedEventHandler(string data);
    public static event QRCodeDetectedEventHandler QRCodeDetectedEvent;

    private void OnQRCodeDetected(string data)
    {
        QRCodeDetectedEvent?.Invoke(data);
    }
    
    void Update()
    {
        if (Time.time - lastAcquisitionTime >= acquisitionCooldown) {
            // Acquire an XRCpuImage
            if (m_CameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                // If successful, launch an asynchronous conversion coroutine
                lastAcquisitionTime = Time.time; // Update last acquisition time
                Debug.Log("STARTING COROUTINE...");
                Debug.Log(Time.time);
                StartCoroutine(DecodeQRCode(image));

                // Dispose the XRCpuImage after we're finished to prevent any memory leaks
                image.Dispose();
            }
        }
    }

    IEnumerator DecodeQRCode(XRCpuImage image)
    {
        // Create the async conversion request
        var request = image.ConvertAsync(new XRCpuImage.ConversionParams
        {
            // Use the full image
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Optionally downsample by 2
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

            // Output an RGB color image format
            outputFormat = TextureFormat.RGB24,

            // Flip across the Y axis
            transformation = XRCpuImage.Transformation.MirrorY
        });

        // Wait for the conversion to complete
        while (!request.status.IsDone())
            yield return null;

        // Check status to see if the conversion completed successfully
        if (request.status != XRCpuImage.AsyncConversionStatus.Ready)
        {
            // Something went wrong
            Debug.LogErrorFormat("Request failed with status {0}", request.status);

            // Dispose even if there is an error
            request.Dispose();
            yield break;
        }

        // Image data is ready. Let's apply it to a Texture2D
        var rawData = request.GetData<byte>();

        // Create a texture
        var texture = new Texture2D(
            request.conversionParams.outputDimensions.x,
            request.conversionParams.outputDimensions.y,
            request.conversionParams.outputFormat,
            false);

        // Copy the image data into the texture
        texture.LoadRawTextureData(rawData);
        texture.Apply();

        // Dispose the request including raw data
        Debug.Log("DISPOSING OF REQUEST...");
        request.Dispose();

        // Decode QR Code using ZXing
        var result = barcodeReader.Decode(texture.GetPixels32(), texture.width, texture.height);

        if (result != null)
        {
            // Debug.Log("QR Code Text: " + result.Text);
            lastDetectedQRCodeData = result.Text;
            OnQRCodeDetected(result.Text);
        }
        else
        {
            // Debug.Log("No QR Code detected.");
        }

        Destroy(texture);
    }
}