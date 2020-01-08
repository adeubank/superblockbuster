using System;
using UnityEngine;

public class MainScreen : MonoBehaviour
{
    private const string PrefsFirstPlay = "isFirstPlay";
    public GameObject showRewardedVideoButton;

    public void Start()
    {
        RemoteConfigController.Instance.OnRemoteConfigFetched += RefreshAds;
        AdController.Instance.OnAdsInitialized += RefreshAds;
        InvokeRepeating(nameof(RefreshAds), 0, 60);
    }

    private void RefreshAds(object sender, EventArgs e)
    {
        RefreshAds();
    }

    private void OnEnable()
    {
        CancelInvoke(nameof(RefreshAds));
        InvokeRepeating(nameof(RefreshAds), 0, 60);
    }

    private void OnDisable()
    {
        AdController.Instance.HideBanner();
        CancelInvoke(nameof(RefreshAds));
    }

    private void RefreshAds()
    {
        if (!RemoteConfigController.Instance.CanShowAd() || IsFirstPlay()) return;

        Debug.Log("Refreshing ads on main screen");

        CheckIfRewardedVideoIsAvailable();

        AdController.Instance.ShowBanner();
    }

    private void CheckIfRewardedVideoIsAvailable()
    {
        Debug.Log("unity-script: Checking if rewarded video ad is available");
        if (AdController.Instance.RewardVideoLoaded())
            showRewardedVideoButton.Activate();
        else
            showRewardedVideoButton.Deactivate();
    }

    public void RewardedVideoButtonClicked()
    {
        Debug.Log("unity-script: ShowRewardedVideoButtonClicked");
        if (AdController.Instance.RewardVideoLoaded())
        {
            AdController.Instance.ShowRewardedVideo();
        }
        else
        {
            showRewardedVideoButton.Deactivate();
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
                AdController.Instance.HideBanner();
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