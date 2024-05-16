using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;


public class JsonBinHyperlinkDataFetcher
{

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api.jsonbin.io/v3/b";
    private readonly string _accessKey = "$2a$10$jZH0PVFEeUvJXMR5au2vBe7I.4f2HBaVFlBceD7E3yU6J3SqIOECG";

    public JsonBinHyperlinkDataFetcher()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-Access-Key", _accessKey);
    }

    public async void FetchJSONFromId(string id, int pageNum, Action<string, int, ARDocumentData> processHyperlinkData)
    {
        try
        {
            string url = string.Format("{0}/{1}/latest", _baseUrl, id);
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Throws exception for non-success status codes
            string json = await response.Content.ReadAsStringAsync();

            // Deserialize JSON string
            RecordWrapper<ARDocumentData> wrapper = JsonUtility.FromJson<RecordWrapper<ARDocumentData>>(json);

            // Access the original object
            ARDocumentData arDocumentData = wrapper.record;

            processHyperlinkData(id, pageNum, arDocumentData);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Error occurred: {0}", e.Message);
        }
    }
}