using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class Tapping : MonoBehaviour
{
    public Image imageDisplay; // Reference to the UI Image object to display the image

    bool URLHasImageExtension(string url)
    {
        // Get the file extension from the URL
        string extension = System.IO.Path.GetExtension(url);

        // Convert the extension to lowercase for case-insensitive comparison
        extension = extension.ToLower();

        // List of common image file extensions
        string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };

        // Check if the extension is in the list of image extensions
        foreach (string imageExt in imageExtensions)
        {
            if (extension == imageExt)
            {
                return true; // URL has an image extension
            }
        }

        return false; // URL does not have an image extension
    }

    void Update()
    {
        // Check if there was a touch
        foreach (var touch in Touch.activeTouches)
        {
            Debug.Log($"{touch.touchId}: {touch.screenPosition},{touch.phase}");
        }
        
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Debug.Log("Touching!!!");
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("hit!");

                // Check if the object has the URLHolder component
                URLMetadata urlHolder = hit.transform.GetComponent<URLMetadata>();

                if (urlHolder != null && URLHasImageExtension(urlHolder.url))
                {
                    Debug.Log("URL is likely an image");
                    
                    // Load and display the image from the URL
                    StartCoroutine(LoadAndDisplayImage(urlHolder.url));
                }
                else
                {
                    Debug.Log("URL is not an image");
                    if (urlHolder != null)
                    {
                        // Open the URL
                        OpenURL(urlHolder.url);
                    }
                }

            }
        }
    }

    IEnumerator LoadAndDisplayImage(string url)
    {
        // Create a new WWW request to download the image
        WWW www = new WWW(url);

        // Wait for the download to complete
        yield return www;

        // Check if there was an error downloading the image
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Error loading image: " + www.error);
        }
        else
        {
            // Create a sprite from the downloaded texture
            Sprite sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), Vector2.zero);

            // Set the sprite to the UI Image object
            imageDisplay.sprite = sprite;
        }
    }

    private void OpenURL(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
        }
    }
}
