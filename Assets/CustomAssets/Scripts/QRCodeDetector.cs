using System;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using ZXing;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;

/* This script is used to asynchronously detect QR code contents from images captured
 * by the device camera (QR detection is done using ZXing library). It runs on a
 * configurable interval (default once per second), and publishes a notification containing
 * decoded QR code data whenever it detects a QR code.
 * 
 * Much of this code was adapted from
 * https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0/manual/features/camera/image-capture.html
*/
public class QRCodeDetector : MonoBehaviour
{
    public ARCameraManager m_CameraManager;
    public delegate void QRCodeDetectedEventHandler(string data);
    public static event QRCodeDetectedEventHandler QRCodeDetectedEvent;
    private BarcodeReader barcodeReader = new BarcodeReader();

    public float acquisitionCooldown = 1.0f; // Cooldown period in seconds
    private float lastAcquisitionTime = 0.0f;

    private void OnQRCodeDetected(string data)
    {
        QRCodeDetectedEvent?.Invoke(data);
    }
    
    void Update()
    {
        if (Time.time - lastAcquisitionTime >= acquisitionCooldown) {
            AsynchronousDetectQRCode();
            lastAcquisitionTime = Time.time; // Update last acquisition time
        }
    }

    void AsynchronousDetectQRCode()
    {
        // Acquire an XRCpuImage
        if (m_CameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            // If successful, launch an asynchronous conversion coroutine
            StartCoroutine(DetectQRCodeFromImageAsync(image));

            // It is safe to dispose the image before the async operation completes
            image.Dispose();
        }
    }

    IEnumerator DetectQRCodeFromImageAsync(XRCpuImage image)
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
        request.Dispose();

        // Decode QR Code using ZXing. Most of the above code was from the Unity docs, but
        // this piece is needed in order to scan the captured screenshot for QR codes.
        var result = barcodeReader.Decode(texture.GetPixels32(), texture.width, texture.height);

        if (result != null)
        {
            OnQRCodeDetected(result.Text);
        }

        // Destroy the texture when we're done with it
        Destroy(texture);
    }
}