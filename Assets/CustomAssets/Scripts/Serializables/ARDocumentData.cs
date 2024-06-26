using System;
using System.Collections.Generic;

// Define classes to represent JSON structure
[System.Serializable]
public class ARDocumentData
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

// Define a wrapper class to handle optional "record" item
[System.Serializable]
public class RecordWrapper<T>
{
    public T record;
}
