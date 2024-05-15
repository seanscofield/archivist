using UnityEngine;

/** This script contains some utility functions for converting PyMuPDF
  * coordinates to Unity, world-space coordinates (which seems to be in meters).
  * In, PyMuPDF each atomic unit is 1/72 inches.
  */
public class CoordinateConverter : MonoBehaviour
{
    public static float[] CalculateHyperlinkOffset(float marker_x0, float marker_y0, float marker_x1, float marker_y1,
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

    public static float[] CalculateHyperlinkScale(float marker_x0, float marker_y0, float marker_x1, float marker_y1,
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
