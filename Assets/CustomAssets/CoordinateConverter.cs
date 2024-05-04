using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateConverter : MonoBehaviour
{
private string jsonString = @"
    {
        ""ar_marker_coordinates"": [252.375, 73.5, 359.625, 180.75],
        ""pages"": [
            {
                ""hyperlinks"": [
                    {
                        ""uri"": ""https://www.google.com"",
                        ""coordinates"": [144.99790954589844, 214.0709228515625, 467.0020751953125, 226.7198486328125]
                    }
                ]
            }
        ]
    }";

    void Start()
    {
        ARData data = JsonUtility.FromJson<ARData>(jsonString);

        float[] markerCoords = data.ar_marker_coordinates;

        foreach (Page page in data.pages)
        {
            foreach (Hyperlink hyperlink in page.hyperlinks)
            {
                float[] hyperlinkCoords = hyperlink.coordinates;

                float[] scale = CalculateHyperlinkScale(markerCoords[0], markerCoords[1], markerCoords[2], markerCoords[3],
                                                        hyperlinkCoords[0], hyperlinkCoords[1], hyperlinkCoords[2], hyperlinkCoords[3]);
                float[] offset = CalculateHyperlinkOffset(markerCoords[0], markerCoords[1], markerCoords[2], markerCoords[3],
                                                          hyperlinkCoords[0], hyperlinkCoords[1], hyperlinkCoords[2], hyperlinkCoords[3]);

                Debug.Log($"Hyperlink URI: {hyperlink.uri}");
                Debug.Log($"Scale - X: {scale[0]}, Z: {scale[1]}");
                Debug.Log($"Offset - X: {offset[0]}, Z: {offset[1]}");
            }
        }
    }

    // Define classes to represent JSON structure
    [System.Serializable]
    public class ARData
    {
        public float[] ar_marker_coordinates;
        public List<Page> pages;
    }

    [System.Serializable]
    public class Page
    {
        public List<Hyperlink> hyperlinks;
    }

    [System.Serializable]
    public class Hyperlink
    {
        public string uri;
        public float[] coordinates;
    }

    float[] CalculateHyperlinkOffset(float marker_x0, float marker_y0, float marker_x1, float marker_y1,
                                     float hyperlink_x0, float hyperlink_y0, float hyperlink_x1, float hyperlink_y1)
    {
        float marker_width = marker_x1 - marker_x0;
        float marker_height = marker_y1 - marker_y0;

        float marker_width_m = marker_width / 72f * 0.0254f;
        float marker_height_m = marker_height / 72f * 0.0254f;

        float marker_ratio_x = marker_width_m / 0.1f;
        float marker_ratio_y = marker_height_m / 0.1f;

        float center_x_marker = (marker_x1 + marker_x0) / 2f;
        float center_y_marker = (marker_y1 + marker_y0) / 2f;

        float center_x_hyperlink = (hyperlink_x0 + hyperlink_x1) / 2f;
        float center_y_hyperlink = (hyperlink_y0 + hyperlink_y1) / 2f;

        float x_offset = center_x_hyperlink - center_x_marker;
        float y_offset = center_y_marker - center_y_hyperlink;

        float x_offset_m = Mathf.Round(x_offset / 72f * 0.0254f / marker_ratio_x * 100000f) / 100000f;
        float y_offset_m = Mathf.Round(y_offset / 72f * 0.0254f / marker_ratio_y * 100000f) / 100000f;

        return new float[] { x_offset_m, y_offset_m };
    }

    float[] CalculateHyperlinkScale(float marker_x0, float marker_y0, float marker_x1, float marker_y1,
                                    float hyperlink_x0, float hyperlink_y0, float hyperlink_x1, float hyperlink_y1)
    {
        float marker_width = marker_x1 - marker_x0;
        float marker_height = marker_y1 - marker_y0;

        float marker_width_m = marker_width / 72f * 0.0254f;
        float marker_height_m = marker_height / 72f * 0.0254f;

        float marker_ratio_x = marker_width_m / 0.1f;
        float marker_ratio_y = marker_height_m / 0.1f;

        float width = hyperlink_x1 - hyperlink_x0;
        float height = hyperlink_y1 - hyperlink_y0;

        float width_meters = Mathf.Round(width / 72f * 0.0254f / marker_ratio_x * 100000f) / 100000f;
        float height_meters = Mathf.Round(height / 72f * 0.0254f / marker_ratio_y * 100000f) / 100000f;

        return new float[] { width_meters, height_meters };
    }
}