using System;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.tvOS;

public class AdController : Singleton<AdController>
{
    public event EventHandler OnRewardVideoClosed;
    private bool _adsInitialized;
    private DateTime _lastAdShownAt = DateTime.Now.AddMinutes(-5); // so we show banner immediately

    // Start is called before the first frame update
    private void Start()
    {
        RemoteConfigController.Instance.OnRemoteConfigFetched += InitializeAds;
    }

    private void InitializeAds(object sender, EventArgs eventArgs)
    {
        if (_adsInitialized) return;

        // Initialize the Google Mobile Ads SDK.
        // Do not call other ads until ads are initialized when using admob mediation
        MobileAds.Initialize(initStatus =>
        {
            _adsInitialized = true;
            InitializeRewardedVideo();
            Debug.Log("MobileAds.Initialize CanShowAds=" + CanShowAds());
        });
    }

    private bool CanShowAds()
    {
        if (!RemoteConfigController.Instance.adsEnabled)
        {
            Debug.Log("Remote setting adsEnabled is not set");
            return false;
        }

        if (!_adsInitialized)
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
        if (_bannerIsVisible) return;
        if ((DateTime.Now - _lastBannerShownAt).Minutes < 2)
        {
            return;
        }

        // Create a 320x50 banner at the bottom of the screen.
        var adUnitId = BannerAdUnitId();
        _bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();
        Debug.Log("Showing banner ad: " + adUnitId);
        _bannerView.LoadAd(request);
        _bannerIsVisible = true;
        _lastBannerShownAt = DateTime.Now;
        _lastAdShownAt = DateTime.Now;
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

        var adUnitId = InterstitialAdUnitId();
        interstitial?.Destroy();
        interstitial = new InterstitialAd(adUnitId);
        AdRequest request = new AdRequest.Builder().Build();
        Debug.Log("Loading interstitial ad: " + adUnitId);

        interstitial.LoadAd(request);
    }

    public void ShowInterstitial()
    {
        if (interstitial == null)
        {
            Debug.Log("interstitial was not initialized");
            return;
        }

        if (!CanShowAds()) return;

        if (interstitial.IsLoaded())
        {
            interstitial.Show();
            _lastAdShownAt = DateTime.Now;
        }
    }

    #endregion

    #region Reward Video Ad

    private RewardBasedVideoAd _rewardBasedVideo;
    private double _recentRewardAmount;
    
    public event EventHandler<EventArgs> OnAdLoaded;

    public bool RewardVideoLoaded()
    {
        if (!CanShowAds()) return false;
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
        AdRequest request = new AdRequest.Builder().Build();
        Debug.Log("Loading reward video ad: " + adUnitId);
        this._rewardBasedVideo.LoadAd(request, adUnitId);
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
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
        OnAdLoaded?.Invoke(sender, args);
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardBasedVideoClosed event received");
        if (_recentRewardAmount >= Double.Epsilon)
        {
            StartCoroutine(AddCoins((int) _recentRewardAmount));
            _recentRewardAmount = 0;
        }

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
            _lastAdShownAt = DateTime.Now;
        }
    }
}