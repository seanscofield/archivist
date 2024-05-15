using System;
using System.Text.RegularExpressions;
using UnityEngine;

/* This script provides some utility methods for analyzing urls. */
public static class URLAnalyzer
{
    public static bool HasImageExtension(string url)
    {
        // Get the file extension from the URL
        string extension = System.IO.Path.GetExtension(url);

        // Convert the extension to lowercase for case-insensitive comparison
        extension = extension.ToLower();

        // Common image file extensions
        string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };

        // Check if the extension is in the list of image extensions
        foreach (string imageExt in imageExtensions)
        {
            if (extension == imageExt)
            {
                return true;
            }
        }

        return false;
    }

    public static bool HaveSameRootDomain(string url1, string url2)
    {
        string rootDomain1 = GetRootDomainFromUrl(url1);
        string rootDomain2 = GetRootDomainFromUrl(url2);
        return rootDomain1.Equals(rootDomain2, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetRootDomainFromUrl(string url)
    {
        string domain = url.Replace("http://", "").Replace("https://", "");

        int index = domain.IndexOf('/');
        if (index != -1)
        {
            domain = domain.Substring(0, index);
        }

        // Split the domain by '.'
        string[] parts = domain.Split('.');

        // Check if the domain has at least two parts (e.g., "example.com")
        if (parts.Length >= 2)
        {
            // If the domain has more than two parts, concatenate the last two parts
            if (parts.Length > 2)
            {
                return parts[parts.Length - 2] + "." + parts[parts.Length - 1];
            }
            // If the domain has exactly two parts, return the domain itself
            else
            {
                return domain;
            }
        }

        // If the domain has less than two parts, return it as is
        return domain;
    }
}
