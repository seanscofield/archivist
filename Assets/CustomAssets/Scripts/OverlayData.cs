using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayData
{
    public string id;
    public string url;
    public Vector3 scale;
    public Vector3 offset;

    public OverlayData(string id, string url, Vector3 scale, Vector3 offset)
    {
        this.id = id;
        this.url = url;
        this.scale = scale;
        this.offset = offset;
    }
}

[System.Serializable]
public class VectorData
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class OverlayDataWrapper
{
    public List<OverlayDataItem> hyperlinks;
}

[System.Serializable]
public class OverlayDataItem
{
    public string id;
    public string url;
    public VectorData scale;
    public VectorData offset;

    public OverlayData ToOverlayData()
    {
        Vector3 scaleVector = new Vector3(scale.x, scale.y, scale.z);
        Vector3 offsetVector = new Vector3(offset.x, offset.y, offset.z);
        return new OverlayData(id,  url, scaleVector, offsetVector);
    }
}

public class OverlayManager
{
    public List<OverlayData> CreateOverlayDataFromJson(string jsonString)
    {
        // Step 1: Deserialize the JSON into an OverlayDataWrapper object
        var jsonDataWrapper = JsonUtility.FromJson<OverlayDataWrapper>(jsonString);

        // Step 2: Create a list to hold the OverlayData objects
        List<OverlayData> overlayDataList = new List<OverlayData>();

        // Step 3: Iterate over each item in the nested array and create OverlayData objects
        foreach (var overlayDataItem in jsonDataWrapper.hyperlinks)
        {
            OverlayData overlayData = overlayDataItem.ToOverlayData();
            overlayDataList.Add(overlayData);
        }

        return overlayDataList;
    }
}