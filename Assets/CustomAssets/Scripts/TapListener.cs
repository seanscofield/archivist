using UnityEngine;
using System.Collections;

/* This script makes it so that whenever a user taps on a hyperlink
 * overlay, we open a browser page and navigate to that hyperlink's url.
 */
public class TapListener : MonoBehaviour
{

    private void OpenURL(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
        }
    }

    void Update()
    { 
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the object has the URLHolder component
                URLMetadata urlHolder = hit.transform.GetComponent<URLMetadata>();

                if (urlHolder != null)
                {
                    // Open the URL
                    OpenURL(urlHolder.url);
                }
            }
        }
    }
}
