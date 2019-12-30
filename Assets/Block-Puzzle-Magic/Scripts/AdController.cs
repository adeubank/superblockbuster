using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdController : Singleton<AdController>
{
    private bool _adsInitialized;
    private BannerView bannerView;

    public bool CanShowAds()
    {
        return RemoteConfigController.Instance.adsEnabled && _adsInitialized;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        // Do not call other ads until ads are initialized when using admob mediation
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("unity-script: MobileAds.Initialize " + initStatus);
            _adsInitialized = true; 
        });
        _adsInitialized = true;

        Debug.Log("unity-script: AdController finished");
    }

    public string BannerAdUnitId()
    {
        if (Debug.isDebugBuild)
        {
#if UNITY_ANDROID
            return "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
            return "ca-app-pub-3940256099942544/2934735716";
#else
            return "unexpected_platform";
#endif
        }

        throw new NotImplementedException("No banner ad unit ID");
    }

    public void ShowBanner()
    {
        if (!CanShowAds()) return;

        // Create a 320x50 banner at the bottom of the screen.
        Debug.Log("unity-script: Showing banner ad");
        bannerView = new BannerView(BannerAdUnitId(), AdSize.Banner, AdPosition.Bottom);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder()
          // .AddTestDevice("8CFBCAB8AE99E62800B95EBE7ED3FA62")
          .Build();
        // Load the banner with the request.
        bannerView.LoadAd(request);
    }
}