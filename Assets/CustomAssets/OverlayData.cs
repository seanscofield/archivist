using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayData
{
    public Vector3 scale;
    public Vector3 offset;
    public string url;
    public string id;

    public OverlayData(Vector3 scale, Vector3 offset, string url, string id)
    {
        this.scale = scale;
        this.offset = offset;
        this.url = url;
        this.id = id;
    }
}


[System.Serializable] // Mark it as serializable for JsonUtility 
public class VectorData
{
    public float x;
    public float y;
    public float z;
}



public class OverlayManager // Or whatever class is appropriate
{
    public OverlayData CreateOverlayDataFromJson(string jsonString)
    {
        // Step 1: Deserialize the JSON while using the VectorData helper
        var jsonData = JsonUtility.FromJson<OverlayDataWrapper>(jsonString);

        // Step 2: Create the OverlayData object using the deserialized data
        return new OverlayData(
            new Vector3(jsonData.scale.x, jsonData.scale.y, jsonData.scale.z),
            new Vector3(jsonData.offset.x, jsonData.offset.y, jsonData.offset.z),
            jsonData.url,
            jsonData.id
        );
    }

    // Wrapper class to match the JSON structure
    [System.Serializable] 
    private class OverlayDataWrapper 
    {
        public VectorData scale;
        public VectorData offset;
        public string url;
        public string id; 
    }
}