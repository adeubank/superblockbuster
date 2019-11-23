using UnityEngine;

public class IronSourceBannerController : MonoBehaviour
{
    private void Start()
    {
        // Add Banner Events
        IronSourceEvents.onBannerAdLoadedEvent += BannerAdLoadedEvent;
        IronSourceEvents.onBannerAdLoadFailedEvent += BannerAdLoadFailedEvent;
        IronSourceEvents.onBannerAdClickedEvent += BannerAdClickedEvent;
        IronSourceEvents.onBannerAdScreenPresentedEvent += BannerAdScreenPresentedEvent;
        IronSourceEvents.onBannerAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
        IronSourceEvents.onBannerAdLeftApplicationEvent += BannerAdLeftApplicationEvent;
    }

    public static void ShowBanner()
    {
        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
    }

    public static void HideBanner()
    {
        IronSource.Agent.hideBanner();
    }

    //Banner Events
    private void BannerAdLoadedEvent()
    {
        Debug.Log("unity-script: I got BannerAdLoadedEvent");
    }

    private void BannerAdLoadFailedEvent(IronSourceError error)
    {
        Debug.Log("unity-script: I got BannerAdLoadFailedEvent, code: " + error.getCode() + ", description : " + error.getDescription());
    }

    private void BannerAdClickedEvent()
    {
        Debug.Log("unity-script: I got BannerAdClickedEvent");
    }

    private void BannerAdScreenPresentedEvent()
    {
        Debug.Log("unity-script: I got BannerAdScreenPresentedEvent");
    }

    private void BannerAdScreenDismissedEvent()
    {
        Debug.Log("unity-script: I got BannerAdScreenDismissedEvent");
    }

    private void BannerAdLeftApplicationEvent()
    {
        Debug.Log("unity-script: I got BannerAdLeftApplicationEvent");
    }
}