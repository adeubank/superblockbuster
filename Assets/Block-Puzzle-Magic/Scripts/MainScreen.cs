using System;
using System.Collections;
using UnityEngine;

public class MainScreen : MonoBehaviour
{
    private const string PrefsFirstPlay = "isFirstPlay";
    public GameObject showRewardedVideoButton;

    public void Start()
    {
        AdController.Instance.OnRewardVideoLoaded += InstanceOnOnRewardVideoLoaded;
        AdController.Instance.OnRewardVideoClosed += RewardedVideoClosed;
        AdController.Instance.OnAdsInitialized += InstanceOnOnAdsInitialized;
    }

    private void InstanceOnOnRewardVideoLoaded(object sender, EventArgs e)
    {
        StartCoroutine(RefreshAds());
    }

    private void InstanceOnOnAdsInitialized(object sender, EventArgs e)
    {
        StartCoroutine(RefreshAds());
    }

    private void OnEnable()
    {
        if (AdController.Instance.adsInitialized) StartCoroutine(RefreshAds());
    }

    private void OnDisable()
    {
        AdController.Instance.HideBanner();
    }

    private IEnumerator RefreshAds()
    {
        if (IsFirstPlay())
        {
            Debug.Log("Not refreshing ads on main screen. User hasn't played yet.");
            yield break;
        }

        Debug.Log("Refreshing ads on main screen");

        if (!AdController.Instance.CanShowAds())
        {
            yield break;
        }

        if (AdController.Instance.RewardVideoLoaded())
        {
            CheckIfRewardedVideoIsAvailable(true);
        }
        else
        {
            CheckIfRewardedVideoIsAvailable(false);
            AdController.Instance.RequestRewardVideoAd();
        }

        AdController.Instance.ShowBanner();
    }

    private void CheckIfRewardedVideoIsAvailable(bool showRewardVideoButton)
    {
        if (showRewardVideoButton)
            showRewardedVideoButton.Activate();
        else
            showRewardedVideoButton.Deactivate();
    }

    public void RewardedVideoButtonClicked()
    {
        if (AdController.Instance.RewardVideoLoaded())
            AdController.Instance.ShowRewardedVideo();
        else
            showRewardedVideoButton.Deactivate();
    }

    public void RewardedVideoClosed(object sender, EventArgs eventArgs)
    {
        showRewardedVideoButton.Deactivate();
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