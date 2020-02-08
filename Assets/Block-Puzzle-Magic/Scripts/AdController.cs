using System;
using System.Collections;
using System.Linq;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdController : Singleton<AdController>
{
    private DateTime _lastAdShownAt = DateTime.Now.AddMinutes(-5); // so we show banner immediately
    public bool adsInitialized;
    public event EventHandler OnAdsInitialized;

    // Start is called before the first frame update
    private void Start()
    {
        RemoteConfigController.Instance.OnRemoteConfigFetched += InitializeAds;
    }

    private void InitializeAds(object sender, EventArgs eventArgs)
    {
        if (adsInitialized) return;

        // Initialize the Google Mobile Ads SDK.
        // Do not call other ads until ads are initialized when using admob mediation
        MobileAds.Initialize(initStatus =>
        {
            adsInitialized = true;
            InitializeRewardedVideo();
            Debug.Log("MobileAds.Initialize CanShowAds=" + CanShowAds());
            OnAdsInitialized?.Invoke(this, EventArgs.Empty);
        });
    }

    private bool CanShowAds()
    {
        if (!RemoteConfigController.Instance.adsEnabled)
        {
            Debug.Log("Remote setting adsEnabled is not set");
            return false;
        }

        if (!adsInitialized)
        {
            Debug.Log("Ads have not been initialized yet");
            return false;
        }

        if (RemoteConfigController.Instance.gamesPlayedBeforeAds > 0 && GameController.GamesPlayed() % RemoteConfigController.Instance.gamesPlayedBeforeAds > 0)
        {
            Debug.Log("Not enough games played yet... GameController.GamesPlayed()=" + GameController.GamesPlayed()
                                                                                     + " gamesPlayedBeforeAds=" + RemoteConfigController.Instance.gamesPlayedBeforeAds
                                                                                     + " GameController.GamesPlayed() % RemoteConfigController.Instance.gamesPlayedBeforeAds=" +
                                                                                     GameController.GamesPlayed() % RemoteConfigController.Instance.gamesPlayedBeforeAds);
            return false;
        }

        // limit how many ads shown per minute
        if ((DateTime.Now - _lastAdShownAt).Minutes < RemoteConfigController.Instance.minutesPerAd)
        {
            Debug.Log("Too many ads shown. Last ad shown at " + _lastAdShownAt
                                                              + ". Minutes since last shown " + (DateTime.Now - _lastAdShownAt).Minutes
                                                              + ". Limiting ads to every + " + RemoteConfigController.Instance.minutesPerAd + " minutes.");
            return false;
        }

        return true;
    }

    private AdRequest NewAdRequest()
    {
        var builder = new AdRequest.Builder();

        if (Debug.isDebugBuild || RemoteConfigController.Instance.debugAds)
        {
            builder.AddTestDevice(AdRequest.TestDeviceSimulator);
            builder = RemoteConfigController.Instance.androidTestDevices.Aggregate(builder, (current, testDevice) =>
            {
                Debug.Log("Configuring android test device " + testDevice);
                return current.AddTestDevice(testDevice);
            });
            builder = RemoteConfigController.Instance.iPhoneTestDevices.Aggregate(builder, (current, testDevice) =>
            {
                Debug.Log("Configuring iPhone test device " + testDevice);
                return current.AddTestDevice(testDevice);
            });
        }

        return builder.Build();
    }

    #region Banner Ad

    private BannerView _bannerView;
    private bool _bannerIsVisible;
    private DateTime _lastBannerShownAt = DateTime.Now.AddMinutes(-5); // so we show banner immediately

    private string BannerAdUnitId()
    {
#if UNITY_ANDROID
        return RemoteConfigController.Instance.androidBannerAdUnitId;
#elif UNITY_IPHONE
        return RemoteConfigController.Instance.iPhoneBannerAdUnitId;
#else
            return "unexpected_platform";
#endif
    }

    public void ShowBanner()
    {
        if (!CanShowAds()) return;
        if (_bannerIsVisible)
        {
            Debug.Log("Banner is already shown");
            return;
        }

        if (!RemoteConfigController.Instance.bannerAdsEnabled)
        {
            Debug.Log("Banner is not enabled.");
            return;
        }

        // limit how many banner ads shown per minute
        // since there is no way if a banner has shown an ad
        if ((DateTime.Now - _lastBannerShownAt).Minutes < RemoteConfigController.Instance.minutesPerAd)
        {
            Debug.Log("Too many ads shown. Last ad shown at " + _lastBannerShownAt
                                                              + ". Minutes since last shown " + (DateTime.Now - _lastBannerShownAt).Minutes
                                                              + ". Limiting ads to every + " + RemoteConfigController.Instance.minutesPerAd + " minutes.");
            return;
        }

        // Create a 320x50 banner at the bottom of the screen.
        var adUnitId = BannerAdUnitId();
        _bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        _bannerView.OnAdFailedToLoad += BannerViewOnOnAdFailedToLoad;

        var adRequest = NewAdRequest();
        Debug.Log("Showing banner ad: " + adUnitId);
        _bannerView.LoadAd(adRequest);
        _bannerIsVisible = true;
        _lastBannerShownAt = DateTime.Now;
    }

    private void BannerViewOnOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
    {
        Debug.Log("Banner ad failed to load: " + e.Message);
        _bannerIsVisible = false;
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
#if UNITY_ANDROID
        return RemoteConfigController.Instance.androidInterstitialAdUnitId;
#elif UNITY_IPHONE
        return RemoteConfigController.Instance.iPhoneInterstitialAdUnitId;
#else
            return "unexpected_platform";
#endif
    }

    public void RequestInterstitial()
    {
        if (!CanShowAds()) return;
        if (!RemoteConfigController.Instance.interstitialAdsEnabled)
        {
            Debug.Log("Interstitial is not enabled.");
            return;
        }

        var adUnitId = InterstitialAdUnitId();
        interstitial?.Destroy();
        interstitial = new InterstitialAd(adUnitId);
        // Called when an ad request failed to load.
        interstitial.OnAdFailedToLoad += InterstitialOnOnAdFailedToLoad;
        var adRequest = NewAdRequest();
        Debug.Log("Loading interstitial ad: " + adUnitId);

        interstitial.LoadAd(adRequest);
    }

    private void InterstitialOnOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
    {
        Debug.Log("Interstitial ad failed to load: " + e.Message);
    }

    public void ShowInterstitial()
    {
        if (interstitial == null)
        {
            Debug.Log("interstitial was not initialized");
            return;
        }

        if (!CanShowAds()) return;

        if (!RemoteConfigController.Instance.interstitialAdsEnabled)
        {
            Debug.Log("Interstitial is not enabled.");
            return;
        }

        if (interstitial.IsLoaded())
        {
            interstitial.Show();
            _lastAdShownAt = DateTime.Now;
        }
        else
        {
            Debug.Log("Interstitial is not loaded.");
        }
    }

    #endregion

    #region Reward Video Ad

    public event EventHandler OnRewardVideoClosed;
    private RewardBasedVideoAd _rewardBasedVideo;
    private double _recentRewardAmount;

    public void ShowRewardedVideo()
    {
        if (!CanShowAds()) return;
        if (_rewardBasedVideo == null)
        {
            Debug.Log("Reward video is not set.");
            return;
        }

        if (!RemoteConfigController.Instance.rewardVideoAdsEnabled)
        {
            Debug.Log("Reward video is not enabled.");
            return;
        }

        if (_rewardBasedVideo.IsLoaded())
        {
            _rewardBasedVideo.Show();
            _lastAdShownAt = DateTime.Now;
        }
        else
        {
            Debug.Log("Reward video is not loaded.");
        }
    }

    public bool RewardVideoLoaded()
    {
        if (!CanShowAds()) return false;

        if (!RemoteConfigController.Instance.rewardVideoAdsEnabled)
        {
            Debug.Log("Reward video is not enabled.");
            return false;
        }

        if (_rewardBasedVideo == null)
        {
            Debug.Log("Reward video is not set.");
            return false;
        }

        if (_rewardBasedVideo.IsLoaded()) return true;

        Debug.Log("Reward video is not loaded.");
        return false;
    }

    private void InitializeRewardedVideo()
    {
        // Get singleton reward based video ad reference.
        _rewardBasedVideo = RewardBasedVideoAd.Instance;
        // Called when the user should be rewarded for watching a video.
        _rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        _rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
        _rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        _rewardBasedVideo.OnAdFailedToLoad += RewardBasedVideoOnOnAdFailedToLoad;
        RequestRewardVideoAd();
    }

    private void RewardBasedVideoOnOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
    {
        Debug.Log("Reward video ad failed to load: " + e.Message);
    }

    private string RewardVideoAdUnitId()
    {
#if UNITY_ANDROID
        return RemoteConfigController.Instance.androidRewardVideoAdUnitId;
#elif UNITY_IPHONE
        return RemoteConfigController.Instance.iPhoneRewardVideoAdUnitId;
#else
            return "unexpected_platform";
#endif
    }

    public void RequestRewardVideoAd()
    {
        var adUnitId = RewardVideoAdUnitId();
        var request = NewAdRequest();
        Debug.Log("Loading reward video ad: " + adUnitId);
        _rewardBasedVideo.LoadAd(request, adUnitId);
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        var type = args.Type;
        var amount = args.Amount;
        _recentRewardAmount = amount;
        Debug.Log("User rewarded with: " + amount + " " + type);
    }

    private IEnumerator AddCoins(int amount)
    {
        yield return new WaitForSecondsRealtime(0.8f);
        CurrencyManager.Instance.AddCoinBalance(amount);
    }

    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardBasedVideoLoaded event received");
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardBasedVideoClosed event received");
        if (_recentRewardAmount >= double.Epsilon)
        {
            StartCoroutine(AddCoins((int) _recentRewardAmount));
            _recentRewardAmount = 0;
        }

        RequestRewardVideoAd();
        OnRewardVideoClosed?.Invoke(sender, args);
    }

    #endregion
}