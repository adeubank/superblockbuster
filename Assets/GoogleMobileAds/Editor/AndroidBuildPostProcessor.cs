#if UNITY_ANDROID

using GoogleMobileAds.Editor;
using UnityEditor;
using UnityEditor.Callbacks;

public static class AndroidBuildPostProcessor
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (!GoogleMobileAdsSettings.Instance.IsAdManagerEnabled && !GoogleMobileAdsSettings.Instance.IsAdMobEnabled) NotifyBuildFailure("Neither Ad Manager nor AdMob is enabled yet.");

        if (GoogleMobileAdsSettings.Instance.IsAdMobEnabled && GoogleMobileAdsSettings.Instance.AdMobAndroidAppId.Length == 0)
            NotifyBuildFailure(
                "Android AdMob app ID is empty. Please enter a valid app ID to run ads properly.");
    }

    private static void NotifyBuildFailure(string message)
    {
        var prefix = "[GoogleMobileAds] ";

        var openSettings = EditorUtility.DisplayDialog(
            "Google Mobile Ads", "Error: " + message, "Open Settings", "Close");
        if (openSettings) GoogleMobileAdsSettingsEditor.OpenInspector();
#if UNITY_2017_1_OR_NEWER
        throw new BuildPlayerWindow.BuildMethodException(prefix + message);
#else
        throw new OperationCanceledException(prefix + message);
#endif
    }
}

#endif