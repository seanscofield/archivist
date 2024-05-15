using System;
using System.Net.Http;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

public class JsonBinApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api.jsonbin.io/v3/b";
    private readonly string _masterKey = "$2a$10$Z6B/b820H97jIfVer0qXVem.TJ5c69MZdAppCDKQ/1C6KwBtYYX/2";

    public JsonBinApiClient()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-Master-Key", _masterKey);
    }

    public async Task<string> ReadData(string binId)
    {
        try
        {
            string url = string.Format("{0}/{1}/latest", _baseUrl, binId);
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Throws exception for non-success status codes
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Error occurred: {0}", e.Message);
            return null;
        }
    }
}