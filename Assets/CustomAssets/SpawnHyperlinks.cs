using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using System.Collections;

public class SpawnHyperlinks : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;

    [SerializeField] private GameObject prefabToSpawn;
    private bool hasSpawned = false;  

    private void OnEnable() => m_TrackedImageManager.trackedImagesChanged += OnChanged;
    private void OnDisable() => m_TrackedImageManager.trackedImagesChanged -= OnChanged;

    private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
           Debug.Log("Newly tracked AR marker:");
           Debug.Log(newImage);

            // Spawn 2 "interactive hyperlinks" on or near the tracked AR marker
            if (!hasSpawned) 
            {
                // Spawn one or more prefabs, setting the AR marker as their parent
                // (so that when the marker moves on the screen, so do the prefabs)
                GameObject hyperlink_1 = Instantiate(prefabToSpawn, newImage.transform);
                GameObject hyperlink_2 = Instantiate(prefabToSpawn, newImage.transform);

                hasSpawned = true; // not currently used, but we could potetially use this

                // Try moving the second hyperlink object further down, and making it
                // a rectangle instead of a square
                Vector3 offset = new Vector3(-0.0335f, 0.0f, -0.143f);
                Vector3 scale = new Vector3(0.08f, 0.001f, 0.015f);
                hyperlink_2.transform.localPosition = offset;
                hyperlink_2.transform.localScale = scale;
            }
        }

        foreach (var updatedImage in eventArgs.updated)
        {
           // Existing code to update the rectangle's position would go here
        }

        foreach (var removedImage in eventArgs.removed)
        {
           // Handle removed event - e.g., destroy spawned object(s)?
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