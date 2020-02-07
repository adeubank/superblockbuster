using System;
using System.Linq;
using Unity.RemoteConfig;
using UnityEngine;

public class RemoteConfigController : Singleton<RemoteConfigController>
{
    // Declare any Settings variables you’ll want to configure remotely:
    public bool adsEnabled;
    public string androidBannerAdUnitId = "ca-app-pub-4216152597478324/8552528193";
    public string androidInterstitialAdUnitId = "ca-app-pub-4216152597478324/7239446520";
    public string androidRewardVideoAdUnitId = "ca-app-pub-4216152597478324/5926364856";

    public string[] androidTestDevices = {"8CFBCAB8AE99E62800B95EBE7ED3FA62"};

    // Optionally declare a unique assignmentId if you need it for tracking:
    public string assignmentId;
    public bool bannerAdsEnabled;
    public bool debugAds;
    public int gameLengthInSeconds = 30;
    public int gamesPlayedBeforeAds = 2;
    public bool interstitialAdsEnabled;
    public string iPhoneBannerAdUnitId = "ca-app-pub-4216152597478324/8169384819";
    public string iPhoneInterstitialAdUnitId = "ca-app-pub-4216152597478324/2917058137";
    public string iPhoneRewardVideoAdUnitId = "ca-app-pub-4216152597478324/6664731457";

    public string[] iPhoneTestDevices = {"1eba537bec490fde807f330c68465605"};

    public int minutesPerAd = 5;
    public int minutesPerBannerAd = 10;

    public bool rewardVideoAdsEnabled;
    public event EventHandler OnRemoteConfigFetched;

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
                debugAds = ConfigManager.appConfig.GetBool("debugAds");
                minutesPerAd = ConfigManager.appConfig.GetInt("minutesPerAd", minutesPerAd);
                minutesPerBannerAd = ConfigManager.appConfig.GetInt("minutesPerBannerAd", minutesPerBannerAd);
                gamesPlayedBeforeAds = ConfigManager.appConfig.GetInt("gamesPlayedBeforeAds", gamesPlayedBeforeAds);
                gameLengthInSeconds = ConfigManager.appConfig.GetInt("gameLengthInSeconds", gameLengthInSeconds);
                bannerAdsEnabled = ConfigManager.appConfig.GetBool("bannerAdsEnabled");
                interstitialAdsEnabled = ConfigManager.appConfig.GetBool("interstitialAdsEnabled");
                rewardVideoAdsEnabled = ConfigManager.appConfig.GetBool("rewardVideoAdsEnabled");

                var remoteAndroidTestDevices = ConfigManager.appConfig.GetString("androidTestDevices").Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();
                if (remoteAndroidTestDevices.Any()) androidTestDevices = remoteAndroidTestDevices;

                var remoteIPhoneTestDevices = ConfigManager.appConfig.GetString("iPhoneTestDevices").Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();
                if (remoteIPhoneTestDevices.Any()) iPhoneTestDevices = remoteIPhoneTestDevices;

                androidBannerAdUnitId = ConfigManager.appConfig.GetString("androidBannerAdUnitId", androidBannerAdUnitId);
                iPhoneBannerAdUnitId = ConfigManager.appConfig.GetString("iPhoneBannerAdUnitId", iPhoneBannerAdUnitId);
                androidInterstitialAdUnitId = ConfigManager.appConfig.GetString("androidInterstitialAdUnitId", androidInterstitialAdUnitId);
                iPhoneInterstitialAdUnitId = ConfigManager.appConfig.GetString("iPhoneInterstitialAdUnitId", iPhoneInterstitialAdUnitId);
                androidRewardVideoAdUnitId = ConfigManager.appConfig.GetString("androidRewardVideoAdUnitId", androidRewardVideoAdUnitId);
                iPhoneRewardVideoAdUnitId = ConfigManager.appConfig.GetString("iPhoneInterstitialAdUnitId", iPhoneRewardVideoAdUnitId);

                assignmentId = ConfigManager.appConfig.assignmentID;
                Debug.Log("New settings loaded this session; update values accordingly. adsEnabled=" + adsEnabled
                                                                                                     + " debugAds=" + debugAds
                                                                                                     + " minutesPerAd=" + minutesPerAd
                                                                                                     + " minutesPerBannerAd=" + minutesPerBannerAd
                                                                                                     + " gamesPlayedBeforeAds=" + gamesPlayedBeforeAds
                                                                                                     + " gameLengthInSeconds=" + gameLengthInSeconds
                                                                                                     + " bannerAdsEnabled=" + bannerAdsEnabled
                                                                                                     + " androidTestDevices=" + androidTestDevices
                                                                                                     + " iPhoneTestDevices=" + iPhoneTestDevices
                                                                                                     + " interstitialAdsEnabled=" + interstitialAdsEnabled
                                                                                                     + " rewardVideoAdsEnabled=" + rewardVideoAdsEnabled
                                                                                                     + " androidBannerAdUnitId=" + androidBannerAdUnitId
                                                                                                     + " iPhoneBannerAdUnitId=" + iPhoneBannerAdUnitId
                                                                                                     + " androidInterstitialAdUnitId=" + androidInterstitialAdUnitId
                                                                                                     + " iPhoneInterstitialAdUnitId=" + iPhoneInterstitialAdUnitId);
                OnRemoteConfigFetched?.Invoke(this, EventArgs.Empty);
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