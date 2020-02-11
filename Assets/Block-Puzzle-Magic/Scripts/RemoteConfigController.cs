using System;
using System.Linq;
using Unity.RemoteConfig;
using UnityEngine;

public class RemoteConfigController : Singleton<RemoteConfigController>
{
    // Declare any Settings variables you’ll want to configure remotely:
    // Optionally declare a unique assignmentId if you need it for tracking:
    public string assignmentId;
    public event EventHandler OnRemoteConfigFetched;

    private string EnvName { get; set; } = "N/A";
    public int GameLengthInSeconds { get; private set; } = 90;
    public bool AdsEnabled { get; private set; }
    public bool BannerAdsEnabled { get; private set; }
    public bool InterstitialAdsEnabled { get; private set; }
    public bool RewardVideoAdsEnabled { get; private set; }
    public bool DebugAds { get; private set; }
    public int MinutesPerAd { get; private set; } = 5;
    public int MinutesPerBannerAd { get; private set; } = 10;
    public int GamesPlayedBeforeAds { get; private set; } = 2;

    public string AndroidBannerAdUnitId { get; private set; } = "ca-app-pub-4216152597478324/8552528193";
    public string AndroidInterstitialAdUnitId { get; private set; } = "ca-app-pub-4216152597478324/7239446520";
    public string AndroidRewardVideoAdUnitId { get; private set; } = "ca-app-pub-4216152597478324/5926364856";
    public string[] AndroidTestDevices { get; private set; } = {"8CFBCAB8AE99E62800B95EBE7ED3FA62"};

    public string IPhoneBannerAdUnitId { get; private set; } = "ca-app-pub-4216152597478324/8169384819";
    public string IPhoneInterstitialAdUnitId { get; private set; } = "ca-app-pub-4216152597478324/2917058137";
    public string IPhoneRewardVideoAdUnitId { get; private set; } = "ca-app-pub-4216152597478324/6664731457";
    public string[] IPhoneTestDevices { get; private set; } = {"1eba537bec490fde807f330c68465605"};
    public string AndroidMoPubFullscreenAdUnitID { get; private set; } = "f0ae3360726649479b3d695f178242bf";
    public string IPhoneMoPubFullscreenAdUnitID { get; private set; } = "92e4f5793ced47b5877e011f1cb92c8b";

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
                EnvName = ConfigManager.appConfig.GetString("EnvName", EnvName);
                AdsEnabled = ConfigManager.appConfig.GetBool("AdsEnabled");
                DebugAds = ConfigManager.appConfig.GetBool("DebugAds");
                MinutesPerAd = ConfigManager.appConfig.GetInt("MinutesPerAd", MinutesPerAd);
                MinutesPerBannerAd = ConfigManager.appConfig.GetInt("MinutesPerBannerAd", MinutesPerBannerAd);
                GamesPlayedBeforeAds = ConfigManager.appConfig.GetInt("GamesPlayedBeforeAds", GamesPlayedBeforeAds);
                GameLengthInSeconds = ConfigManager.appConfig.GetInt("GameLengthInSeconds", GameLengthInSeconds);
                BannerAdsEnabled = ConfigManager.appConfig.GetBool("BannerAdsEnabled");
                InterstitialAdsEnabled = ConfigManager.appConfig.GetBool("InterstitialAdsEnabled");
                RewardVideoAdsEnabled = ConfigManager.appConfig.GetBool("RewardVideoAdsEnabled");

                var remoteAndroidTestDevices = ConfigManager.appConfig.GetString("AndroidTestDevices").Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();
                if (remoteAndroidTestDevices.Any()) AndroidTestDevices = remoteAndroidTestDevices;

                var remoteIPhoneTestDevices = ConfigManager.appConfig.GetString("IPhoneTestDevices").Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();
                if (remoteIPhoneTestDevices.Any()) IPhoneTestDevices = remoteIPhoneTestDevices;

                AndroidBannerAdUnitId = ConfigManager.appConfig.GetString("AndroidBannerAdUnitId", AndroidBannerAdUnitId);
                IPhoneBannerAdUnitId = ConfigManager.appConfig.GetString("IPhoneBannerAdUnitId", IPhoneBannerAdUnitId);
                AndroidInterstitialAdUnitId = ConfigManager.appConfig.GetString("AndroidInterstitialAdUnitId", AndroidInterstitialAdUnitId);
                IPhoneInterstitialAdUnitId = ConfigManager.appConfig.GetString("IPhoneInterstitialAdUnitId", IPhoneInterstitialAdUnitId);
                AndroidRewardVideoAdUnitId = ConfigManager.appConfig.GetString("AndroidRewardVideoAdUnitId", AndroidRewardVideoAdUnitId);
                IPhoneRewardVideoAdUnitId = ConfigManager.appConfig.GetString("IPhoneRewardVideoAdUnitId", IPhoneRewardVideoAdUnitId);
                AndroidMoPubFullscreenAdUnitID = ConfigManager.appConfig.GetString("AndroidMoPubFullscreenAdUnitID", AndroidMoPubFullscreenAdUnitID);
                IPhoneMoPubFullscreenAdUnitID = ConfigManager.appConfig.GetString("IPhoneMoPubFullscreenAdUnitID", IPhoneMoPubFullscreenAdUnitID);

                assignmentId = ConfigManager.appConfig.assignmentID;
                Debug.Log("New settings loaded this session; EnvName=" + EnvName
                                                                       + " AdsEnabled=" + AdsEnabled
                                                                       + " DebugAds=" + DebugAds
                                                                       + " MinutesPerAd=" + MinutesPerAd
                                                                       + " MinutesPerBannerAd=" + MinutesPerBannerAd
                                                                       + " GamesPlayedBeforeAds=" + GamesPlayedBeforeAds
                                                                       + " GameLengthInSeconds=" + GameLengthInSeconds
                                                                       + " BannerAdsEnabled=" + BannerAdsEnabled
                                                                       + " AndroidTestDevices=" + string.Join(",", AndroidTestDevices)
                                                                       + " IPhoneTestDevices=" + string.Join(",", IPhoneTestDevices)
                                                                       + " InterstitialAdsEnabled=" + InterstitialAdsEnabled
                                                                       + " RewardVideoAdsEnabled=" + RewardVideoAdsEnabled
                                                                       + " AndroidBannerAdUnitId=" + AndroidBannerAdUnitId
                                                                       + " IPhoneBannerAdUnitId=" + IPhoneBannerAdUnitId
                                                                       + " AndroidInterstitialAdUnitId=" + AndroidInterstitialAdUnitId
                                                                       + " IPhoneInterstitialAdUnitId=" + IPhoneInterstitialAdUnitId
                                                                       + " AndroidRewardVideoAdUnitId=" + AndroidRewardVideoAdUnitId
                                                                       + " IPhoneRewardVideoAdUnitId=" + IPhoneRewardVideoAdUnitId
                                                                       + " AndroidMoPubFullscreenAdUnitID=" + AndroidMoPubFullscreenAdUnitID
                                                                       + " IPhoneMoPubFullscreenAdUnitID=" + IPhoneMoPubFullscreenAdUnitID);
                OnRemoteConfigFetched?.Invoke(this, EventArgs.Empty);
                break;
        }
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