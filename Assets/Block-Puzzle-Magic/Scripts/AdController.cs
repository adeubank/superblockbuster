using System;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdController : Singleton<AdController>
{
    public event EventHandler OnAdsInitialized;
    public event EventHandler OnRewardVideoClosed;
    private bool _adsInitialized;

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        // Do not call other ads until ads are initialized when using admob mediation
        MobileAds.Initialize(initStatus =>
        {
            _adsInitialized = true;
            InitializeRewardedVideo();
            OnAdsInitialized?.Invoke(this, EventArgs.Empty);
            Debug.Log("MobileAds.Initialize " + initStatus + " CanShowAds=" + CanShowAds());
        });
    }

    private bool CanShowAds()
    {
        return RemoteConfigController.Instance.adsEnabled && _adsInitialized;
    }

    #region Banner Ad

    private BannerView _bannerView;
    private bool _bannerIsVisible;

    private string BannerAdUnitId()
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
#if UNITY_ANDROID
        return "ca-app-pub-4216152597478324/8552528193";
#elif UNITY_IPHONE
        return "ca-app-pub-4216152597478324/8169384819";
#else
            return "unexpected_platform";
#endif
    }

    public void ShowBanner()
    {
        if (!CanShowAds()) return;
        if (_bannerIsVisible) return;

        // Create a 320x50 banner at the bottom of the screen.
        var adUnitId = BannerAdUnitId();
        _bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();
        Debug.Log("Showing banner ad: " + adUnitId);
        _bannerView.LoadAd(request);
        _bannerIsVisible = true;
    }

    public void HideBanner()
    {
        _bannerView?.Destroy();
        _bannerIsVisible = false;
    }

    #endregion

    #region Interstitial Ad

    private InterstitialAd interstitial;

    private string InterstitialAdUnitId()
    {
        if (Debug.isDebugBuild)
        {
#if UNITY_ANDROID
            return "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
            return "ca-app-pub-3940256099942544/4411468910";
#else
        return  "unexpected_platform";
#endif
        }

#if UNITY_ANDROID
        return "ca-app-pub-4216152597478324/7239446520";
#elif UNITY_IPHONE
        return "ca-app-pub-4216152597478324/2917058137";
#else
        return "unexpected_platform";
#endif
    }

    public void RequestInterstitial()
    {
        if (!CanShowAds()) return;

        var adUnitId = InterstitialAdUnitId();
        interstitial?.Destroy();
        interstitial = new InterstitialAd(adUnitId);
        AdRequest request = new AdRequest.Builder().Build();
        Debug.Log("Loading interstitial ad: " + adUnitId);

        interstitial.LoadAd(request);
    }

    public void ShowInterstitial()
    {
        if (interstitial == null) return;
        if (!CanShowAds()) return;
        
        if (interstitial.IsLoaded())
        {
            interstitial.Show();
        }
    }

    #endregion

    #region Reward Video Ad

    private RewardBasedVideoAd _rewardBasedVideo;
    public event EventHandler<EventArgs> OnAdLoaded;

    public bool RewardVideoLoaded()
    {
        return _rewardBasedVideo != null && _rewardBasedVideo.IsLoaded();
    }

    private void InitializeRewardedVideo()
    {
        // Get singleton reward based video ad reference.
        _rewardBasedVideo = RewardBasedVideoAd.Instance;
        // Called when the user should be rewarded for watching a video.
        _rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        _rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
        _rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        RequestRewardVideoAd();
    }

    private string RewardVideoAdUnitId()
    {
        if (Debug.isDebugBuild)
        {
#if UNITY_ANDROID
            string adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
            string adUnitId = "unexpected_platform";
#endif
        }
#if UNITY_ANDROID
        return "ca-app-pub-4216152597478324/5926364856";
#elif UNITY_IPHONE
        return "ca-app-pub-4216152597478324/6664731457";
#else
        return "unexpected_platform";
#endif
    }

    public void RequestRewardVideoAd()
    {
        var adUnitId = RewardVideoAdUnitId();
        AdRequest request = new AdRequest.Builder().Build();
        Debug.Log("Loading reward video ad: " + adUnitId);
        this._rewardBasedVideo.LoadAd(request, adUnitId);
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        Debug.Log("User rewarded with: " + amount + " " + type);
        StartCoroutine(AddCoins((int) amount));
    }

    private IEnumerator AddCoins(int amount)
    {
        yield return new WaitForSecondsRealtime(0.8f);
        CurrencyManager.Instance.AddCoinBalance(amount);
    }

    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardBasedVideoLoaded event received");
        OnAdLoaded?.Invoke(sender, args);
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardBasedVideoClosed event received");
        this.RequestRewardVideoAd();
        OnRewardVideoClosed?.Invoke(sender, args);
    }

    #endregion

    public void ShowRewardedVideo()
    {
        if (!_adsInitialized || _rewardBasedVideo == null)
        {
            return;
        }

        if (_rewardBasedVideo.IsLoaded())
        {
            _rewardBasedVideo.Show();
        }
    }
}