using UnityEngine;

public class MainScreen : MonoBehaviour
{
    private const string PrefsFirstPlay = "isFirstPlay";
    private bool _bannerShown;

    public GameObject showRewardedVideoButton;

    public void Start()
    {
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(RefreshAds), 0, 120);
    }

    private void OnDisable()
    {
        HideBannerAd();
        CancelInvoke(nameof(RefreshAds));
    }

    private void RefreshAds()
    {
        if (!RemoteConfigController.Instance.CanShowAd() || IsFirstPlay()) return;

        Debug.Log("Refreshing ads on main screen");

        CheckIfRewardedVideoIsAvailable();

        if (!IronSource.Agent.isBannerPlacementCapped(name) && !_bannerShown)
        {
            IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM, name);
            _bannerShown = true;
        }
    }

    private void HideBannerAd()
    {
        IronSource.Agent.hideBanner();
        _bannerShown = false;
    }

    private void CheckIfRewardedVideoIsAvailable()
    {
        Debug.Log("unity-script: Checking if rewarded video ad is available");
        IronSource.Agent.isRewardedVideoAvailable();
    }

    private void RewardedVideoAvailabilityChangedEvent(bool canShowAd)
    {
        Debug.Log("unity-script: I got RewardedVideoAvailabilityChangedEvent, value = " + canShowAd);
        if (canShowAd && RemoteConfigController.Instance.CanShowAd() && !IronSource.Agent.isRewardedVideoPlacementCapped(name))
            EnableRewardVideoButton();
        else
            DisableRewardVideoButton();
    }

    private void EnableRewardVideoButton()
    {
        showRewardedVideoButton.Activate();
    }

    private void DisableRewardVideoButton()
    {
        showRewardedVideoButton.Deactivate();
    }

    public void RewardedVideoButtonClicked()
    {
        Debug.Log("unity-script: ShowRewardedVideoButtonClicked");
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo(name);
        }
        else
        {
            DisableRewardVideoButton();
            Debug.Log("unity-script: IronSource.Agent.isRewardedVideoAvailable - False");
        }
    }

    /// <summary>
    ///     Raises the play button pressed event.
    /// </summary>
    public void OnPlayButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            if (IsFirstPlay())
            {
                MarkFirstPlay();
                PowerupController.Instance.LoadSavedPurchasedPowerups();
                PowerupController.Instance.LoadSavedEquippedPowerups();
                AudioManager.Instance.PlayButtonClickSound();
                GameController.gameMode = GameMode.TIMED;
                StackManager.Instance.ActivateGamePlay();
                StackManager.Instance.mainMenu.Deactivate();
            }
            else
            {
                HideBannerAd();
                StackManager.Instance.powerupSelectScreen.Activate();
            }
        }
    }

    public bool IsFirstPlay()
    {
        return PlayerPrefs.GetInt(PrefsFirstPlay, 0) == 0 ? true : false;
    }

    public void MarkFirstPlay()
    {
        PlayerPrefs.SetInt(PrefsFirstPlay, 1);
    }
}