using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Open dialog for app rating
/// </summary>
public class AppRating : MonoBehaviour
{
    private const string AndroidRatingURI = "http://play.google.com/store/apps/details?id={0}";
    private const string iOSRatingURI = "itms://itunes.apple.com/us/app/apple-store/{0}?mt=8";

    [Tooltip("iOS App ID (number), example: 1122334455")]
    public string iOSAppID = "";

    private string url;

    // Initialization
    void Start()
    {
#if UNITY_IOS
        if (!string.IsNullOrEmpty (iOSAppID)) {
            url = iOSRatingURI.Replace("{0}",iOSAppID);
        }
        else {
            Debug.LogWarning ("Please set iOSAppID variable");
        }

#elif UNITY_ANDROID
        url = AndroidRatingURI.Replace("{0}", Application.identifier);
#endif
    }

    /// <summary>
    /// Open rating url
    /// </summary>
    public void Open()
    {
        if (!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
        }
        else
        {
            Debug.LogWarning("Unable to open URL, invalid OS");
        }
    }
}