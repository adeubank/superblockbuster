using Unity.RemoteConfig;
using UnityEngine;

public class RemoteConfigController : Singleton<RemoteConfigController>
{
    // Declare any Settings variables you’ll want to configure remotely:
    public bool adsEnabled;

    // Optionally declare a unique assignmentId if you need it for tracking:
    public string assignmentId;

    private void Awake()
    {
        // Add a listener to apply settings when successfully retrieved: 
        ConfigManager.FetchCompleted += ApplyRemoteSettings;

        // Fetch configuration setting from the remote service: 
        ConfigManager.FetchConfigs(new userAttributes(), new appAttributes());
    }

    // Create a function to set your variables to their keyed values:
    private void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        // Conditionally update settings, depending on the response's origin:
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.Log("No settings loaded this session; using default values.");
                break;
            case ConfigOrigin.Cached:
                Debug.Log("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                adsEnabled = ConfigManager.appConfig.GetBool("adsEnabled");
                assignmentId = ConfigManager.appConfig.assignmentID;
                Debug.Log("New settings loaded this session; update values accordingly. adsEnabled=" + adsEnabled);
                break;
        }
    }

    public bool CanShowAd()
    {
        return adsEnabled;
    }

    public struct userAttributes
    {
        // Optionally declare variables for any custom user attributes; if none keep an empty struct:
    }

    public struct appAttributes
    {
        // Optionally declare variables for any custom app attributes; if none keep an empty struct:
    }
}