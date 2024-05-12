using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class Tapping : MonoBehaviour
{
    void Update()
    {
        // Check if there was a touch
        foreach (var touch in Touch.activeTouches) {
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
                Metadata urlHolder = hit.transform.GetComponent<Metadata>();
                if (urlHolder != null)
                {
                    // Open the URL
                    OpenURL(urlHolder.url);
                }
            }
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
