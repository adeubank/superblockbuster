using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdController : Singleton<AdController>
{
    private bool _adsInitialized;
    private BannerView _bannerView;

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

    public string BannerAdUnitId(string placementName)
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

        switch (placementName)
        {
            case "MainScreen":
            case "GamePlay":
            default:
                throw new NotImplementedException("No banner ad unit ID");
        }
    }

    public void ShowBanner(string placementName)
    {
        if (!CanShowAds()) return;

        // Create a 320x50 banner at the bottom of the screen.
        var adUnitId = BannerAdUnitId(placementName);
        _bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();
        Debug.Log("unity-script: Showing banner ad: " + adUnitId);
        _bannerView.LoadAd(request);
    }

    public void HideBanner()
    {
        _bannerView.Destroy();
    }

    private InterstitialAd interstitial;

    private string InterstitialAdUnitId(string placementName)
    {
        if (Debug.isDebugBuild)
        {
#if UNITY_ANDROID
        return  "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
            return "ca-app-pub-3940256099942544/4411468910";
#else
        return  "unexpected_platform";
#endif
        }

        switch (placementName)
        {
            case "GameOver":
            default:
                throw new NotImplementedException("No interstitial ad unit ID");
        }
    }

    public void RequestInterstitial(string placementName)
    {
        if (!CanShowAds()) return;

        var adUnitId = InterstitialAdUnitId(placementName);
        interstitial?.Destroy();
        interstitial = new InterstitialAd(adUnitId);
        AdRequest request = new AdRequest.Builder().Build();
        Debug.Log("unity-script: Loading interstitial ad: " + adUnitId);

        interstitial.LoadAd(request);
    }

    public void ShowInterstitial()
    {
        if (CanShowAds() && interstitial.IsLoaded())
        {
            interstitial.Show();
        }
    }
}