using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class HyperlinkDataFetcher
{
    private static string url = "https://raw.githubusercontent.com/seanscofield/archivist/main/Assets/CustomAssets";

    public static IEnumerator FetchJSONFromId(string id, int pageNum, Action<string, int, ARDocumentData> processHyperlinkData)
    {
        string fullUrl = $"{url}/{id}.json";

        using (UnityWebRequest www = UnityWebRequest.Get(fullUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error fetching JSON from URL: {www.error}");
            }
            else {
                ARDocumentData arDocumentData = JsonUtility.FromJson<ARDocumentData>(www.downloadHandler.text);
                processHyperlinkData(id, pageNum, arDocumentData);
            }
        }
    }
}
