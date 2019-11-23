using UnityEngine;

public class IronSourceInterstitialController : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        IronSourceEvents.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
        IronSourceEvents.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
        IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
        IronSourceEvents.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;
        IronSourceEvents.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
        IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
        IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;
    }

    public static void LoadInterstitial()
    {
        Debug.Log("unity-script: LoadInterstitialButtonClicked");
        IronSource.Agent.loadInterstitial();
    }

    public static void ShowInterstitial()
    {
        Debug.Log("unity-script: ShowInterstitialButtonClicked");
        if (IronSource.Agent.isInterstitialReady())
            IronSource.Agent.showInterstitial();
        else
            Debug.Log("unity-script: IronSource.Agent.isInterstitialReady - False");
    }

    private void InterstitialAdReadyEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdReadyEvent");
    }

    private void InterstitialAdLoadFailedEvent(IronSourceError error)
    {
        Debug.Log("unity-script: I got InterstitialAdLoadFailedEvent, code: " + error.getCode() + ", description : " + error.getDescription());
    }

    private void InterstitialAdShowSucceededEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdShowSucceededEvent");
    }

    private void InterstitialAdShowFailedEvent(IronSourceError error)
    {
        Debug.Log("unity-script: I got InterstitialAdShowFailedEvent, code :  " + error.getCode() + ", description : " + error.getDescription());
    }

    private void InterstitialAdClickedEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdClickedEvent");
    }

    private void InterstitialAdOpenedEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdOpenedEvent");
    }

    private void InterstitialAdClosedEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdClosedEvent");
    }

    private void InterstitialAdRewardedEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdRewardedEvent");
    }
}