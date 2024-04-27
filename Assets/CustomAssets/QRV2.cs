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

public class QRCodeDetector : MonoBehaviour
{
    public ARCameraManager m_CameraManager;

    // public string savePath = ""; // Path to save the image
    // public string pngFilePath = "Assets/SavedImages/qr4.png"; // Path to the PNG file

    public float acquisitionCooldown = 1.0f; // Cooldown period in seconds
    private float lastAcquisitionTime = 0.0f;

    void Start()
    {
        // Load the PNG file into Texture2D
        // LoadPNGFile(pngFilePath);
    }

    void Update()
    {
        if (Time.time - lastAcquisitionTime >= acquisitionCooldown) {
            // Acquire an XRCpuImage
            if (m_CameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                // If successful, launch an asynchronous conversion coroutine
                lastAcquisitionTime = Time.time; // Update last acquisition time
                StartCoroutine(DecodeQRCode(image));

                // It is safe to dispose the image before the async operation completes
                image.Dispose();
            }
        }

        // Dispose the XRCpuImage after we're finished to prevent any memory leaks
        // image.Dispose();
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
    request.Dispose();

    // Decode QR Code using ZXing
    var barcodeReader = new BarcodeReader();
    var result = barcodeReader.Decode(texture.GetPixels32(), texture.width, texture.height);

    if (result != null)
    {
        Debug.Log("QR Code Text: " + result.Text);
        // Process the QR code text here...
    }
    else
    {
        Debug.Log("No QR Code detected.");
    }
    }

    void SaveTextureToFile(Texture2D texture, string path)
    {
        // Create directory if it doesn't exist
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // Convert the texture to PNG bytes
        byte[] imageBytes = texture.EncodeToPNG();

        // Save the bytes to a file
        string filename = $"QRCode_{DateTime.Now:yyyyMMddHHmmss}.png";
        File.WriteAllBytes(Path.Combine(path, filename), imageBytes);

        Debug.Log($"Saved image to: {path}/{filename}");
    }

    void SaveTextureToPNG(Texture2D texture, string fileName)
    {
        byte[] bytes = texture.EncodeToPNG();

        // Get the current timestamp
        DateTime currentTime = DateTime.Now;

        // Convert the timestamp to a string in a desired format
        string timestampString = currentTime.ToString("yyyy-MM-dd-HH:mm:ss");

        string filePath = Path.Combine(Application.persistentDataPath, timestampString + ".png");
        
        File.WriteAllBytes(filePath, bytes);
        Debug.Log("Saved texture to: " + filePath);
    }

    // void LoadPNGFile(string filePath)
    // {
    //     if (File.Exists(filePath))
    //     {
    //         byte[] fileData = File.ReadAllBytes(filePath);
    //         m_Texture = new Texture2D(2, 2); // Create a new Texture2D
    //         m_Texture.LoadImage(fileData); // Load the PNG data into the Texture2D

    //         // Decode the loaded texture
    //         DecodeTexture(m_Texture);
    //     }
    //     else
    //     {
    //         Debug.LogError("PNG file not found at path: " + filePath);
    //     }
    // }

    // void DecodeTexture(Texture2D texture)
    // {
    //     // Decode QR Code using ZXing
    //     var barcodeReader = new BarcodeReader();
    //     var result = barcodeReader.Decode(texture.GetPixels32(), texture.width, texture.height);

    //     if (result != null)
    //     {
    //         Debug.Log("QR Code Text: " + result.Text);
    //         // Process the QR code text here...
    //     }
    //     else
    //     {
    //         Debug.Log("No QR Code detected in the PNG image.");
    //     }

    //     // SaveTextureToFile(m_Texture, savePath);
    // }
}